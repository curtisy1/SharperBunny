namespace SharperBunny.Consume {
  using System;
  using System.Threading.Tasks;
  using SharperBunny.Configuration;
  using SharperBunny.Connect;
  using SharperBunny.Declare;
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;

  public class DeclareResponder<TRequest, TResponse> : IRespond<TRequest, TResponse>
    where TRequest : class
    where TResponse : class {
    public const string DIRECT_REPLY_TO = "amq.rabbitmq.reply-to";
    private const string DEFAULT_EXCHANGE = "";

    public DeclareResponder(IBunny bunny, string rpcExchange, string fromQueue, Func<TRequest, Task<TResponse>> respond) {
      if (respond == null) {
        throw DeclarationException.Argument(new ArgumentException("respond delegate must not be null"));
      }

      this._bunny = bunny;
      this._respond = respond;
      this._rpcExchange = rpcExchange;
      this._serialize = Config.Serialize;
      this._consumeFromQueue = fromQueue;
      this._thisChannel = new PermanentChannel(bunny);
      this._deserialize = Config.Deserialize<TRequest>;
    }

    public async Task<OperationResult<TResponse>> StartRespondingAsync() {
      var result = new OperationResult<TResponse>();
      var publisher = this._bunny.Publisher<TResponse>(DEFAULT_EXCHANGE)
        .WithSerialize(this._serialize);

      publisher.UseUniqueChannel(this._useUniqueChannel);
      Func<ICarrot<TRequest>, Task> _receiver = async carrot => {
        var request = carrot.Message;
        try {
          var response = await this._respond(request);
          var reply_to = carrot.MessageProperties.ReplyTo;

          publisher.WithRoutingKey(reply_to);
          result = await publisher.SendAsync(response);
        } catch (Exception ex) {
          result.IsSuccess = false;
          result.State = OperationState.RpcReplyFailed;
          result.Error = ex;
        }
      };

      // consume
      var forceDeclare = this._bunny.Setup()
        .Queue(this._consumeFromQueue)
        .AsDurable()
        .Bind(this._rpcExchange, this._consumeFromQueue);

      var consumeResult = await this._bunny.Consumer<TRequest>(this._consumeFromQueue)
                            .DeserializeMessage(this._deserialize)
                            .Callback(_receiver)
                            .StartConsumingAsync(forceDeclare);

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
    private readonly Func<TRequest, Task<TResponse>> _respond;

    #endregion

    #region declarations

    public IRespond<TRequest, TResponse> WithSerialize(Func<TResponse, byte[]> serialize) {
      this._serialize = serialize;
      return this;
    }

    public IRespond<TRequest, TResponse> WithDeserialize(Func<ReadOnlyMemory<byte>, TRequest> deserialize) {
      this._deserialize = deserialize;
      return this;
    }

    public IRespond<TRequest, TResponse> WithUniqueChannel(bool useUniqueChannel = true) {
      this._useUniqueChannel = useUniqueChannel;
      return this;
    }

    #endregion

    #region IDisposable Support

    private bool disposedValue;

    protected virtual void Dispose(bool disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          this._thisChannel.Dispose();
        }

        this.disposedValue = true;
      }
    }

    public void Dispose() {
      this.Dispose(true);
    }

    #endregion
  }
}