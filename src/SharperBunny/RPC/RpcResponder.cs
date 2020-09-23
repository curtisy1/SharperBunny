namespace SharperBunny.RPC {
  using System;
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;

  public class RpcResponder<TRequest, TResponse> : RpcBase<TResponse, TRequest>, IRespond<TResponse>
    where TRequest : class
    where TResponse : class {
    private const string defaultExchange = "";

    private readonly string consumeFromQueue;
    private readonly Func<TRequest, TResponse> respond;
    private readonly string rpcExchange;

    public RpcResponder(IBunny bunny, string rpcExchange, string fromQueue, Func<TRequest, TResponse> respond)
    : base(bunny) {
      this.respond = respond ?? throw DeclarationException.Argument(new ArgumentException("respond delegate must not be null"));
      this.rpcExchange = rpcExchange;
      this.consumeFromQueue = fromQueue;
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
      var forceDeclare = this.bunny.Queue(this.consumeFromQueue)
        .Bind(this.rpcExchange, this.consumeFromQueue)
        .SetDurable();

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

    public void Dispose() => this.Dispose(true);

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