using System;
using System.Threading.Tasks;
using SharperBunny.Connect;
using SharperBunny.Declare;
using SharperBunny.Exceptions;

namespace SharperBunny.Consume {
    public class DeclareResponder<TRequest, TResponse> : IRespond<TRequest, TResponse>
        where TRequest : class
    where TResponse : class {
        public const string DIRECT_REPLY_TO = "amq.rabbitmq.reply-to";
        private const string DEFAULT_EXCHANGE = "";

        #region immutable fields
        private readonly IBunny _bunny;
        private readonly string _rpcExchange;
        private readonly string _consumeFromQueue;
        private readonly PermanentChannel _thisChannel;
        #endregion 

        #region mutable fields
        private bool _useTempQueue;
        private bool _useUniqueChannel;
        private Func<ReadOnlyMemory<byte>, TRequest> _deserialize;
        private Func<TResponse, byte[]> _serialize;
        private Func<TRequest, Task<TResponse>> _respond;
        #endregion
        public DeclareResponder (IBunny bunny, string rpcExchange, string fromQueue, Func<TRequest, Task<TResponse>> respond) {
            if (respond == null) {
                throw DeclarationException.Argument (new ArgumentException ("respond delegate must not be null"));
            }
            _bunny = bunny;
            _respond = respond;
            _rpcExchange = rpcExchange;
            _serialize = Config.Serialize;
            _consumeFromQueue = fromQueue;
            _thisChannel = new PermanentChannel (bunny);
            _deserialize = Config.Deserialize<TRequest>;
        }

        public async Task<OperationResult<TResponse>> StartRespondingAsync () {
            var result = new OperationResult<TResponse> ();
            var publisher = _bunny.Publisher<TResponse> (DEFAULT_EXCHANGE)
                .WithSerialize (_serialize);

            publisher.UseUniqueChannel (uniqueChannel: _useUniqueChannel);
            Func<ICarrot<TRequest>, Task> _receiver = async carrot => {
                var request = carrot.Message;
                try {
                    TResponse response = await _respond (request);
                    string reply_to = carrot.MessageProperties.ReplyTo;

                    publisher.WithRoutingKey (reply_to);
                    result = await publisher.SendAsync (response);
                } catch (System.Exception ex) {
                    result.IsSuccess = false;
                    result.State = OperationState.RpcReplyFailed;
                    result.Error = ex;
                }
            };

            // consume
            IQueue forceDeclare = _bunny.Setup ()
                .Queue (_consumeFromQueue)
                .AsDurable ()
                .Bind (_rpcExchange, _consumeFromQueue);

            var consumeResult = await _bunny.Consumer<TRequest> (_consumeFromQueue)
                .DeserializeMessage (_deserialize)
                .Callback (_receiver)
                .StartConsumingAsync (forceDeclare);

            if (consumeResult.IsSuccess) {
                result.IsSuccess = true;
                result.State = OperationState.Response;
            } else {
                result.IsSuccess = false;
                result.Error = consumeResult.Error;
                result.State = consumeResult.State;
            }
            return result;
        }

        #region declarations
        public IRespond<TRequest, TResponse> WithSerialize (Func<TResponse, byte[]> serialize) {
            _serialize = serialize;
            return this;
        }

        public IRespond<TRequest, TResponse> WithDeserialize (Func<ReadOnlyMemory<byte>, TRequest> deserialize) {
            _deserialize = deserialize;
            return this;
        }

        public IRespond<TRequest, TResponse> WithUniqueChannel (bool useUniqueChannel = true) {
            _useUniqueChannel = useUniqueChannel;
            return this;
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose (bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _thisChannel.Dispose ();
                }
                disposedValue = true;
            }
        }

        public void Dispose () {
            Dispose (true);
        }
        #endregion
    }
}