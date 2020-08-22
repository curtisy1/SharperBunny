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
    public const string DIRECT_REPLY_TO = "amq.rabbitmq.reply-to";

    internal DeclareRequest(IBunny bunny, string toExchange, string routingKey) {
      this._bunny = bunny;
      this._toExchange = toExchange;
      this._routingKey = routingKey;
      this._serialize = Config.Serialize;
      this._deserialize = Config.Deserialize<TResponse>;
      this._thisChannel = new PermanentChannel(this._bunny);
    }

    public async Task<OperationResult<TResponse>> RequestAsync(TRequest request, bool force = false) {
      var bytes = this._serialize(request);
      var result = new OperationResult<TResponse>();
      var mre = new ManualResetEvent(false);

      var channel = this._thisChannel.Channel;
      if (force) {
        channel.ExchangeDeclare(this._toExchange,
                                "direct",
                                true,
                                false,
                                null);
      }

      var correlationId = Guid.NewGuid().ToString();

      var reply_to = this._useTempQueue ? channel.QueueDeclare().QueueName : DIRECT_REPLY_TO;
      result = await this.ConsumeAsync(channel, reply_to, result, mre, correlationId);

      if (result.IsSuccess) {
        result = await this.PublishAsync(channel, reply_to, bytes, result, correlationId);
        mre.WaitOne(this._timeOut);
      }

      if (this._useUniqueChannel) {
        this._thisChannel.Channel.Close();
      }

      return result;
    }

    public IRequest<TRequest, TResponse> WithTimeOut(uint timeOut) {
      this._timeOut = (int)timeOut;
      return this;
    }

    public IRequest<TRequest, TResponse> WithTemporaryQueue(bool useTempQueue = true) {
      this._useTempQueue = useTempQueue;
      return this;
    }

    public IRequest<TRequest, TResponse> WithQueueDeclare(string queue = null, string exchange = null, string routingKey = null) {
      var name = queue ?? typeof(TRequest).FullName;
      var rKey = routingKey ?? typeof(TRequest).FullName;
      this._queueDeclare = this._bunny.Setup().Queue(name).Bind(this._toExchange, rKey).AsDurable();

      return this;
    }

    public IRequest<TRequest, TResponse> WithQueueDeclare(IQueue queue) {
      this._queueDeclare = queue;
      return this;
    }

    public IRequest<TRequest, TResponse> SerializeRequest(Func<TRequest, byte[]> serialize) {
      this._serialize = serialize;
      return this;
    }

    public IRequest<TRequest, TResponse> DeserializeResponse(Func<ReadOnlyMemory<byte>, TResponse> deserialize) {
      this._deserialize = deserialize;
      return this;
    }

    public IRequest<TRequest, TResponse> UseUniqueChannel(bool useUnique = true) {
      this._useUniqueChannel = useUnique;
      return this;
    }

    private async Task<OperationResult<TResponse>> PublishAsync(IModel channel, string reply_to, byte[] payload, OperationResult<TResponse> result, string correlationId) {
      // publish
      var props = channel.CreateBasicProperties();
      props.ReplyTo = reply_to;
      props.CorrelationId = correlationId;
      props.Persistent = false;

      DeclarePublisher<TRequest>.ConstructProperties(props, false, 1500);
      try {
        await Task.Run(() => { channel.BasicPublish(this._toExchange, this.RoutingKey, false, props, payload); });
        result.IsSuccess = true;
        result.State = OperationState.RpcPublished;
      } catch (Exception ex) {
        result.IsSuccess = false;
        result.Error = ex;
        result.State = OperationState.RequestFailed;
      }

      return result;
    }

    private async Task<OperationResult<TResponse>> ConsumeAsync(IModel channel, string reply_to, OperationResult<TResponse> result, ManualResetEvent mre, string correlationId) {
      var consumer = new EventingBasicConsumer(channel);
      EventHandler<BasicDeliverEventArgs> handle = null;
      var tag = $"temp-consumer {typeof(TRequest)}-{typeof(TResponse)}-{Guid.NewGuid()}";

      handle = (s, ea) => {
        try {
          var response = this._deserialize(ea.Body);
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
        await Task.Run(() => channel.BasicConsume(reply_to,
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
        if (this._queueDeclare != null) {
          return this._queueDeclare.RoutingKey;
        }

        return this._routingKey;
      }
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