namespace SharperBunny.Declare {
  using System;
  using SharperBunny.Configuration;
  using SharperBunny.Connection;
  using SharperBunny.Exceptions;
  using SharperBunny.Extensions;
  using SharperBunny.Interfaces;

  public class DeclareResponder<TRequest, TResponse> : IRespond<TRequest, TResponse>
    where TRequest : class
    where TResponse : class {
    public const string directReplyTo = "amq.rabbitmq.reply-to";
    private const string defaultExchange = "";

    private readonly IBunny bunny;
    private readonly string consumeFromQueue;
    private readonly Func<TRequest, TResponse> respond;
    private readonly string rpcExchange;
    private readonly PermanentChannel thisChannel;
    private Func<ReadOnlyMemory<byte>, TRequest> deserialize;

    private bool disposedValue;
    private Func<TResponse, byte[]> serialize;

    private bool useTempQueue;
    private bool useUniqueChannel;

    public DeclareResponder(IBunny bunny, string rpcExchange, string fromQueue, Func<TRequest, TResponse> respond) {
      this.bunny = bunny;
      this.respond = respond ?? throw DeclarationException.Argument(new ArgumentException("respond delegate must not be null"));
      this.rpcExchange = rpcExchange;
      this.serialize = Config.Serialize;
      this.consumeFromQueue = fromQueue;
      this.thisChannel = new PermanentChannel(bunny);
      this.deserialize = Config.Deserialize<TRequest>;
    }

    public OperationResult<TResponse> StartResponding() {
      var result = new OperationResult<TResponse>();
      var publisher = this.bunny.Publisher<TResponse>(defaultExchange)
        .WithSerialize(this.serialize);

      publisher.UseUniqueChannel(this.useUniqueChannel);

      void Receiver(ICarrot<TRequest> carrot) {
        var request = carrot.Message;
        try {
          var response = this.respond(request);
          var replyTo = carrot.MessageProperties.ReplyTo;

          publisher.WithRoutingKey(replyTo);
          result = publisher.Send(response);
        } catch (Exception ex) {
          result.IsSuccess = false;
          result.State = OperationState.RpcReplyFailed;
          result.Error = ex;
        }
      }

      // consume
      var forceDeclare = this.bunny.Setup()
        .Queue(this.consumeFromQueue)
        .Bind(this.rpcExchange, this.consumeFromQueue)
        .AsDurable();

      var consumeResult = this.bunny.Consumer<TRequest>(this.consumeFromQueue)
        .DeserializeMessage(this.deserialize)
        .Callback(Receiver)
        .StartConsuming(forceDeclare as IQueue);

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

    public IRespond<TRequest, TResponse> WithSerialize(Func<TResponse, byte[]> serialize) {
      this.serialize = serialize;
      return this;
    }

    public IRespond<TRequest, TResponse> WithDeserialize(Func<ReadOnlyMemory<byte>, TRequest> deserialize) {
      this.deserialize = deserialize;
      return this;
    }

    public IRespond<TRequest, TResponse> WithUniqueChannel(bool useUniqueChannel = true) {
      this.useUniqueChannel = useUniqueChannel;
      return this;
    }

    public void Dispose() {
      this.Dispose(true);
    }

    protected virtual void Dispose(bool disposing) {
      if (this.disposedValue) {
        return;
      }

      if (disposing) {
        this.thisChannel.Dispose();
      }

      this.disposedValue = true;
    }
  }
}