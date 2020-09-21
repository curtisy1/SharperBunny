namespace SharperBunny.RPC {
  using System;
  using RabbitMQ.Client;
  using RabbitMQ.Client.Events;
  using SharperBunny.Interfaces;
  using SharperBunny.Publish;

  public class RpcRequest<TRequest, TResponse> : RpcBase<TRequest, TResponse>, IRequest<TRequest, TResponse>
    where TRequest : class
    where TResponse : class {
    private readonly string routingKey;
    private readonly string toExchange;
    private IQueue queueDeclare;

    private int timeOut = 1500;

    internal RpcRequest(IBunny bunny, string toExchange, string routingKey)
    : base(bunny) {
      this.toExchange = toExchange;
      this.routingKey = routingKey;
    }

    private string RoutingKey => this.queueDeclare != null ? this.queueDeclare.RoutingKey : this.routingKey;

    public OperationResult<TResponse> Request(TRequest request, bool force = false) {
      var bytes = this.serialize(request);
      var result = new OperationResult<TResponse>();

      var channel = this.thisChannel.Channel;
      if (force) {
        channel.ExchangeDeclare(this.toExchange,
                                "direct",
                                true,
                                false,
                                null);
      }

      var correlationId = Guid.NewGuid().ToString();

      var replyTo = this.useTempQueue ? channel.QueueDeclare().QueueName : directReplyTo;
      result = this.Consume(channel, replyTo, result);

      if (result.IsSuccess) {
        result = this.Publish(channel, replyTo, bytes, result, correlationId);
      }

      if (this.useUniqueChannel) {
        this.thisChannel.Channel.Close();
      }

      return result;
    }

    public IRequest<TRequest, TResponse> WithTimeOut(int timeOut) {
      this.timeOut = timeOut;
      return this;
    }

    public IRequest<TRequest, TResponse> WithQueueDeclare(string queue = null, string exchange = null, string routingKey = null) {
      var name = queue ?? typeof(TRequest).FullName;
      var rKey = routingKey ?? typeof(TRequest).FullName;
      this.queueDeclare = (IQueue)this.bunny.Setup().Queue(name).Bind(exchange ?? this.toExchange, rKey).SetDurable();

      return this;
    }

    public IRequest<TRequest, TResponse> WithQueueDeclare(IQueue queue) {
      this.queueDeclare = queue;
      return this;
    }

    public void Dispose() {
      this.Dispose(true);
    }

    private OperationResult<TResponse> Publish(IModel channel, string replyTo, byte[] payload, OperationResult<TResponse> result, string correlationId) {
      var props = channel.CreateBasicProperties();
      props.ReplyTo = replyTo;
      props.CorrelationId = correlationId;
      props.Persistent = false;

      DeclarePublisher<TRequest>.ConstructProperties(props, false, this.timeOut);
      try {
        channel.BasicPublish(this.toExchange, this.RoutingKey, false, props, payload);
        result.IsSuccess = true;
        result.State = OperationState.RpcPublished;
      } catch (Exception ex) {
        result.IsSuccess = false;
        result.Error = ex;
        result.State = OperationState.RequestFailed;
      }

      return result;
    }

    private OperationResult<TResponse> Consume(IModel channel, string replyTo, OperationResult<TResponse> result) {
      var consumer = new EventingBasicConsumer(channel);
      EventHandler<BasicDeliverEventArgs> handle = null;
      var tag = $"temp-consumer {typeof(TRequest)}-{typeof(TResponse)}-{Guid.NewGuid()}";

      handle = (s, ea) => {
        try {
          var response = this.deserialize(ea.Body);
          result.Message = response;
          result.IsSuccess = true;
          result.State = OperationState.RpcSucceeded;
        } catch (Exception ex) {
          result.Error = ex;
          result.IsSuccess = false;
          result.State = OperationState.ResponseFailed;
        } finally {
          consumer.Received -= handle;
          channel.BasicCancel(tag);
        }
      };
      consumer.Received += handle;

      try {
        channel.BasicConsume(replyTo,
                             true,
                             $"temp-consumer {typeof(TRequest)}-{typeof(TResponse)}",
                             false,
                             false,
                             null,
                             consumer);

        result.IsSuccess = true;
        result.State = OperationState.RpcPublished;
      } catch (Exception ex) {
        result.IsSuccess = false;
        result.State = OperationState.RpcReplyFailed;
        result.Error = ex;
      }

      return result;
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