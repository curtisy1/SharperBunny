using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharperBunny.Connect;
using SharperBunny.Declare;

namespace SharperBunny.Publish {
    public class DeclareRequest<TRequest, TResponse> : IRequest<TRequest, TResponse>
        where TRequest : class
    where TResponse : class {
        public const string DIRECT_REPLY_TO = "amq.rabbitmq.reply-to";
        #region immutable fields
        private readonly IBunny _bunny;
        private readonly string _toExchange;
        private readonly string _routingKey;
        private readonly PermanentChannel _thisChannel;
        #endregion 

        #region mutable fields
        private int _timeOut = 1500;
        private Func<ReadOnlyMemory<byte>, TResponse> _deserialize;
        private Func<TRequest, byte[]> _serialize;
        private bool _useTempQueue;
        private bool _useUniqueChannel;
        private IQueue _queueDeclare;
        private string RoutingKey {
            get {
                if (_queueDeclare != null) {
                    return _queueDeclare.RoutingKey;
                }

                return _routingKey;
            }
        }
        #endregion
        internal DeclareRequest (IBunny bunny, string toExchange, string routingKey) {
            _bunny = bunny;
            _toExchange = toExchange;
            _routingKey = routingKey;
            _serialize = Config.Serialize;
            _deserialize = Config.Deserialize<TResponse>;
            _thisChannel = new PermanentChannel (_bunny);
        }

        public async Task<OperationResult<TResponse>> RequestAsync (TRequest request, bool force = false) {
            var bytes = _serialize (request);
            var result = new OperationResult<TResponse> ();
            var mre = new ManualResetEvent (false);

            var channel = _thisChannel.Channel;
            if (force) {
                channel.ExchangeDeclare (_toExchange,
                    type: "direct",
                    durable : true,
                    autoDelete : false,
                    arguments : null);
            }

            string correlationId = Guid.NewGuid ().ToString ();

            string reply_to = _useTempQueue ? channel.QueueDeclare ().QueueName : DIRECT_REPLY_TO;
            result = await ConsumeAsync (channel, reply_to, result, mre, correlationId);

            if (result.IsSuccess) {
                result = await PublishAsync (channel, reply_to, bytes, result, correlationId);
                mre.WaitOne (_timeOut);
            }

            if (_useUniqueChannel) {
                _thisChannel.Channel.Close ();
            }

            return result;
        }

        public IRequest<TRequest, TResponse> WithTimeOut (uint timeOut) {
            _timeOut = (int) timeOut;
            return this;
        }

        private async Task<OperationResult<TResponse>> PublishAsync (IModel channel, string reply_to, byte[] payload, OperationResult<TResponse> result, string correlationId) {
            // publish
            var props = channel.CreateBasicProperties ();
            props.ReplyTo = reply_to;
            props.CorrelationId = correlationId;
            props.Persistent = false;

            DeclarePublisher<TRequest>.ConstructProperties (props, persistent : false, expires : 1500);
            try {
                await Task.Run (() => {
                    channel.BasicPublish (_toExchange, RoutingKey, mandatory : false, props, payload);
                });
                result.IsSuccess = true;
                result.State = OperationState.RpcPublished;
            } catch (System.Exception ex) {
                result.IsSuccess = false;
                result.Error = ex;
                result.State = OperationState.RequestFailed;
            }
            return result;
        }

        private async Task<OperationResult<TResponse>> ConsumeAsync (IModel channel, string reply_to, OperationResult<TResponse> result, ManualResetEvent mre, string correlationId) {
            var consumer = new EventingBasicConsumer (channel);
            EventHandler<BasicDeliverEventArgs> handle = null;
            string tag = $"temp-consumer {typeof(TRequest)}-{typeof(TResponse)}-{Guid.NewGuid()}";

            handle = (s, ea) => {
                try {
                    TResponse response = _deserialize (ea.Body);
                    result.Message = response;
                    result.IsSuccess = true;
                    result.State = OperationState.RpcSucceeded;
                } catch (System.Exception ex) {
                    result.Error = ex;
                    result.IsSuccess = false;
                    result.State = OperationState.ResponseFailed;
                } finally {
                    mre.Set ();
                    consumer.Received -= handle;
                    channel.BasicCancel (tag);
                    mre.Dispose ();
                }
            };
            consumer.Received += handle;

            try {
                await Task.Run (() => channel.BasicConsume (reply_to,
                    autoAck : true,
                    consumerTag: $"temp-consumer {typeof(TRequest)}-{typeof(TResponse)}",
                    noLocal : false,
                    exclusive : false,
                    arguments : null,
                    consumer : consumer));

                result.IsSuccess = true;
                result.State = OperationState.RpcPublished;
            } catch (System.Exception ex) {
                result.IsSuccess = false;
                result.State = OperationState.RpcReplyFailed;
                result.Error = ex;
            }

            return result;
        }

        public IRequest<TRequest, TResponse> WithTemporaryQueue (bool useTempQueue = true) {
            _useTempQueue = useTempQueue;
            return this;
        }

        public IRequest<TRequest, TResponse> WithQueueDeclare (string queue = null, string exchange = null, string routingKey = null) {
            string name = queue ?? typeof (TRequest).FullName;
            string rKey = routingKey ?? typeof (TRequest).FullName;
            _queueDeclare = _bunny.Setup ().Queue (name).Bind (_toExchange, rKey).AsDurable ();

            return this;
        }

        public IRequest<TRequest, TResponse> WithQueueDeclare (IQueue queue) {
            _queueDeclare = queue;
            return this;
        }

        public IRequest<TRequest, TResponse> SerializeRequest (Func<TRequest, byte[]> serialize) {
            _serialize = serialize;
            return this;
        }

        public IRequest<TRequest, TResponse> DeserializeResponse (Func<ReadOnlyMemory<byte>, TResponse> deserialize) {
            _deserialize = deserialize;
            return this;
        }

        public IRequest<TRequest, TResponse> UseUniqueChannel (bool useUnique = true) {
            _useUniqueChannel = useUnique;
            return this;
        }

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