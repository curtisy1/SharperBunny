namespace SharperBunny.Publish {
  using System;
  using System.Threading.Tasks;
  using RabbitMQ.Client;
  using RabbitMQ.Client.Events;
  using SharperBunny.Configuration;
  using SharperBunny.Connect;
  using SharperBunny.Declare;
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;

  public class DeclarePublisher<T> : IPublish<T>
    where T : class {
    internal DeclarePublisher(IBunny bunny, string publishTo) {
      this._bunny = bunny;
      this._publishTo = publishTo;
      this._serialize = Config.Serialize;
      this._thisChannel = new PermanentChannel(bunny);
    }

    #region Send

    public virtual async Task<OperationResult<T>> SendAsync(T msg, bool force = false) {
      var operationResult = new OperationResult<T>();
      operationResult.Message = msg;
      IModel channel = null;
      try {
        channel = this._thisChannel.Channel;

        var properties = ConstructProperties(channel.CreateBasicProperties(), this.Persistent, this.Expires);
        this.Handlers(channel);

        if (this._queueDeclare != null) {
          await this._queueDeclare.DeclareAsync();
        }

        if (force) {
          await this._bunny.Setup()
            .Exchange(this._publishTo)
            .AsDurable()
            .DeclareAsync();
        }

        await Task.Run(() => {
          if (this._useConfirm) {
            channel.ConfirmSelect();
          }

          channel.BasicPublish(this._publishTo, this.RoutingKey, this.Mandatory, properties, this._serialize(msg));
          if (this._useConfirm) {
            channel.WaitForConfirmsOrDie();
          }
        });

        operationResult.IsSuccess = true;
        operationResult.State = OperationState.Published;
      } catch (Exception ex) {
        operationResult.IsSuccess = false;
        operationResult.Error = ex;
        operationResult.State = OperationState.Failed;
      } finally {
        if (this._uniqueChannel) {
          this.Handlers(channel, true);
          channel?.Close();
        }
      }

      return operationResult;
    }

    #endregion

    #region immutable fields

    private readonly IBunny _bunny;
    private readonly PermanentChannel _thisChannel;
    private readonly string _publishTo;

    #endregion

    #region mutable fields

    private Func<T, byte[]> _serialize;
    private bool Mandatory { get; set; }
    private bool ConfirmActivated { get; set; }
    private bool Persistent { get; set; }
    private int? Expires { get; set; }

    private string RoutingKey {
      get {
        if (this._routingKey != null) {
          return this._routingKey;
        }

        if (this._queueDeclare != null) {
          return this._queueDeclare.RoutingKey;
        }

        return typeof(T).FullName;
      }
    }

    private string _routingKey;
    private bool _uniqueChannel;
    private IQueue _queueDeclare;
    private bool _useConfirm;
    private Func<BasicReturnEventArgs, Task> _returnCallback = context => Task.CompletedTask;
    private Func<BasicAckEventArgs, Task> _ackCallback = context => Task.CompletedTask;
    private Func<BasicNackEventArgs, Task> _nackCallback = context => Task.CompletedTask;

    #endregion

    #region PublisherConfirm

    private void Handlers(IModel channel, bool dismantle = false) {
      if (this.Mandatory) {
        if (dismantle) {
          channel.BasicReturn -= this.HandleReturn;
        } else {
          channel.BasicReturn += this.HandleReturn;
        }
      }

      if (this._useConfirm) {
        if (dismantle) {
          channel.BasicNacks -= this.HandleNack;
          channel.BasicAcks -= this.HandleAck;
        } else {
          channel.BasicNacks += this.HandleNack;
          channel.BasicAcks += this.HandleAck;
        }
      }
    }

    private async void HandleReturn(object sender, BasicReturnEventArgs eventArgs) {
      await this._returnCallback(eventArgs);
    }

    private async void HandleAck(object sender, BasicAckEventArgs eventArgs) {
      await this._ackCallback(eventArgs);
    }

    private async void HandleNack(object sender, BasicNackEventArgs eventArgs) {
      await this._nackCallback(eventArgs);
    }

    public static IBasicProperties ConstructProperties(IBasicProperties basicProperties, bool persistent, int? expires) {
      basicProperties.Persistent = persistent;
      basicProperties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
      basicProperties.Type = typeof(T).FullName;
      if (expires.HasValue) {
        basicProperties.Expiration = expires.Value.ToString();
      }

      basicProperties.CorrelationId = Guid.NewGuid().ToString();
      basicProperties.ContentType = Config.ContentType;
      basicProperties.ContentEncoding = Config.ContentEncoding;

      return basicProperties;
    }

    #endregion

    #region Declarations

    public IPublish<T> AsMandatory(Func<BasicReturnEventArgs, Task> onReturn) {
      this._returnCallback = onReturn;
      this.Mandatory = true;
      return this;
    }

    public IPublish<T> AsPersistent() {
      this.Persistent = true;
      return this;
    }

    public IPublish<T> WithConfirm(Func<BasicAckEventArgs, Task> onAck, Func<BasicNackEventArgs, Task> onNack) {
      if (onAck == null || onNack == null) {
        throw DeclarationException.Argument(new ArgumentException("handlers for ack and nack must not be null"));
      }

      this._useConfirm = true;
      this._ackCallback = onAck;
      this._nackCallback = onNack;
      return this;
    }

    public IPublish<T> WithExpire(uint expire) {
      this.Expires = (int)expire;
      return this;
    }

    public IPublish<T> WithSerialize(Func<T, byte[]> serialize) {
      this._serialize = serialize;
      return this;
    }

    public IPublish<T> WithRoutingKey(string routingKey) {
      this._routingKey = routingKey;
      return this;
    }

    public IPublish<T> UseUniqueChannel(bool uniqueChannel = true) {
      this._uniqueChannel = uniqueChannel;
      return this;
    }

    public IPublish<T> WithQueueDeclare(string queueName = null, string routingKey = null, string exchangeName = "amq.direct") {
      var name = queueName ?? typeof(T).FullName;
      var rKey = routingKey ?? typeof(T).FullName;
      this._queueDeclare = this._bunny.Setup().Queue(name).Bind(exchangeName, rKey).AsDurable();
      return this;
    }

    public IPublish<T> WithQueueDeclare(IQueue queueDeclare) {
      this._queueDeclare = queueDeclare;
      return this;
    }

    #endregion

    #region IDisposable Support

    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          this.Handlers(this._thisChannel.Channel, true);
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