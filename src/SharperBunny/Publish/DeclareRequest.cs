namespace SharperBunny.Publish {
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using RabbitMQ.Client;
  using RabbitMQ.Client.Events;
  using SharperBunny.Configuration;
  using SharperBunny.Connect;
  using SharperBunny.Declare;
  using SharperBunny.Interfaces;

  public class DeclareRequest<TRequest, TResponse> : IRequest<TRequest, TResponse>
    where TRequest : class
    where TResponse : class {
    public const string directReplyTo = "amq.rabbitmq.reply-to";

    internal DeclareRequest(IBunny bunny, string toExchange, string routingKey) {
      this.bunny = bunny;
      this.toExchange = toExchange;
      this.routingKey = routingKey;
      this.serialize = Config.Serialize;
      this.deserialize = Config.Deserialize<TResponse>;
      this.thisChannel = new PermanentChannel(this.bunny);
    }

    public async Task<OperationResult<TResponse>> RequestAsync(TRequest request, bool force = false) {
      var bytes = this.serialize(request);
      var result = new OperationResult<TResponse>();
      var mre = new ManualResetEvent(false);

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
      result = await this.ConsumeAsync(channel, replyTo, result, mre, correlationId);

      if (result.IsSuccess) {
        result = await this.PublishAsync(channel, replyTo, bytes, result, correlationId);
        mre.WaitOne(this.timeOut);
      }

      if (this.useUniqueChannel) {
        this.thisChannel.Channel.Close();
      }

      return result;
    }

    public IRequest<TRequest, TResponse> WithTimeOut(uint timeOut) {
      this.timeOut = (int)timeOut;
      return this;
    }

    public IRequest<TRequest, TResponse> WithTemporaryQueue(bool useTempQueue = true) {
      this.useTempQueue = useTempQueue;
      return this;
    }

    public IRequest<TRequest, TResponse> WithQueueDeclare(string queue = null, string exchange = null, string routingKey = null) {
      var name = queue ?? typeof(TRequest).FullName;
      var rKey = routingKey ?? typeof(TRequest).FullName;
      this.queueDeclare = this.bunny.Setup().Queue(name).Bind(this.toExchange, rKey).AsDurable();

      return this;
    }

    public IRequest<TRequest, TResponse> WithQueueDeclare(IQueue queue) {
      this.queueDeclare = queue;
      return this;
    }

    public IRequest<TRequest, TResponse> SerializeRequest(Func<TRequest, byte[]> serialize) {
      this.serialize = serialize;
      return this;
    }

    public IRequest<TRequest, TResponse> DeserializeResponse(Func<ReadOnlyMemory<byte>, TResponse> deserialize) {
      this.deserialize = deserialize;
      return this;
    }

    public IRequest<TRequest, TResponse> UseUniqueChannel(bool useUnique = true) {
      this.useUniqueChannel = useUnique;
      return this;
    }

    private async Task<OperationResult<TResponse>> PublishAsync(IModel channel, string replyTo, byte[] payload, OperationResult<TResponse> result, string correlationId) {
      // publish
      var props = channel.CreateBasicProperties();
      props.ReplyTo = replyTo;
      props.CorrelationId = correlationId;
      props.Persistent = false;

      DeclarePublisher<TRequest>.ConstructProperties(props, false, 1500);
      try {
        await Task.Run(() => { channel.BasicPublish(this.toExchange, this.RoutingKey, false, props, payload); });
        result.IsSuccess = true;
        result.State = OperationState.RpcPublished;
      } catch (Exception ex) {
        result.IsSuccess = false;
        result.Error = ex;
        result.State = OperationState.RequestFailed;
      }

      return result;
    }

    private async Task<OperationResult<TResponse>> ConsumeAsync(IModel channel, string replyTo, OperationResult<TResponse> result, ManualResetEvent mre, string correlationId) {
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
          mre.Set();
          consumer.Received -= handle;
          channel.BasicCancel(tag);
          mre.Dispose();
        }
      };
      consumer.Received += handle;

      try {
        await Task.Run(() => channel.BasicConsume(replyTo,
                                                  true,
                                                  $"temp-consumer {typeof(TRequest)}-{typeof(TResponse)}",
                                                  false,
                                                  false,
                                                  null,
                                                  consumer));

        result.IsSuccess = true;
        result.State = OperationState.RpcPublished;
      } catch (Exception ex) {
        result.IsSuccess = false;
        result.State = OperationState.RpcReplyFailed;
        result.Error = ex;
      }

      return result;
    }

    #region immutable fields

    private readonly IBunny bunny;
    private readonly string toExchange;
    private readonly string routingKey;
    private readonly PermanentChannel thisChannel;

    #endregion

    #region mutable fields

    private int timeOut = 1500;
    private Func<ReadOnlyMemory<byte>, TResponse> deserialize;
    private Func<TRequest, byte[]> serialize;
    private bool useTempQueue;
    private bool useUniqueChannel;
    private IQueue queueDeclare;

    private string RoutingKey {
      get {
        if (this.queueDeclare != null) {
          return this.queueDeclare.RoutingKey;
        }

        return this.routingKey;
      }
    }

    #endregion

    #region IDisposable Support

    private bool disposedValue;

    protected virtual void Dispose(bool disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          this.thisChannel.Dispose();
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