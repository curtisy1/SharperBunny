namespace SharperBunny.Publish {
  using System;
  using RabbitMQ.Client;
  using RabbitMQ.Client.Events;
  using SharperBunny.Connection;
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;
  using SharperBunny.Serializer;

  public class DeclarePublisher<T> : Serializable<T>, IPublish<T>
    where T : class {
    private const string contentEncoding = "utf-8";
    private const string contentType = "application/json";
    
    private readonly IBunny bunny;
    private readonly string publishTo;
    private readonly PermanentChannel thisChannel;
    private Action<BasicAckEventArgs> ackCallback = context => { };
    private bool disposedValue;
    private Action<BasicNackEventArgs> nackCallback = context => { };
    private IQueue queueDeclare;
    private Action<BasicReturnEventArgs> returnCallback = context => { };

    private string routingKey;

    private Func<T, byte[]> serialize;
    private bool uniqueChannel;
    private bool useConfirm;

    public DeclarePublisher(IBunny bunny, string publishTo) {
      this.bunny = bunny;
      this.publishTo = publishTo;
      this.serialize = this.InternalSerialize;
      this.thisChannel = new PermanentChannel(bunny);
    }

    private bool Mandatory { get; set; }
    private bool ConfirmActivated { get; set; }
    private bool Persistent { get; set; }
    private int? Expires { get; set; }

    private string RoutingKey {
      get {
        if (this.routingKey != null) {
          return this.routingKey;
        }

        return this.queueDeclare != null ? this.queueDeclare.RoutingKey : typeof(T).FullName;
      }
    }

    public virtual OperationResult<T> Send(T msg, bool force = false) {
      var operationResult = new OperationResult<T> { Message = msg };
      IModel channel = null;
      try {
        channel = this.thisChannel.Channel;

        var properties = ConstructProperties(channel.CreateBasicProperties(), this.Persistent, this.Expires);
        this.Handlers(channel);

        this.queueDeclare?.Declare();

        if (force) {
          this.bunny.Exchange(this.publishTo)
            .SetDurable()
            .Declare();
        }

        if (this.useConfirm) {
          channel.ConfirmSelect();
        }

        channel.BasicPublish(this.publishTo, this.RoutingKey, this.Mandatory, properties, this.serialize(msg));
        if (this.useConfirm) {
          channel.WaitForConfirmsOrDie();
        }

        operationResult.IsSuccess = true;
        operationResult.State = OperationState.Published;
      } catch (Exception ex) {
        operationResult.IsSuccess = false;
        operationResult.Error = ex;
        operationResult.State = OperationState.Failed;
      } finally {
        if (this.uniqueChannel) {
          this.Handlers(channel, true);
          channel?.Close();
        }
      }

      return operationResult;
    }

    public IPublish<T> AsMandatory(Action<BasicReturnEventArgs> onReturn) {
      this.returnCallback = onReturn;
      this.Mandatory = true;
      return this;
    }

    public IPublish<T> AsPersistent() {
      this.Persistent = true;
      return this;
    }

    public IPublish<T> WithConfirm(Action<BasicAckEventArgs> onAck, Action<BasicNackEventArgs> onNack) {
      if (onAck == null || onNack == null) {
        throw DeclarationException.Argument(new ArgumentException("handlers for ack and nack must not be null"));
      }

      this.useConfirm = true;
      this.ackCallback = onAck;
      this.nackCallback = onNack;
      return this;
    }

    public IPublish<T> WithExpire(int expire) {
      this.Expires = expire;
      return this;
    }

    public IPublish<T> WithSerialize(Func<T, byte[]> serialize) {
      this.serialize = serialize;
      return this;
    }

    public IPublish<T> WithRoutingKey(string routingKey) {
      this.routingKey = routingKey;
      return this;
    }

    public IPublish<T> UseUniqueChannel(bool uniqueChannel = true) {
      this.uniqueChannel = uniqueChannel;
      return this;
    }

    public IPublish<T> WithQueueDeclare(string queueName = null, string routingKey = null, string exchangeName = "amq.direct") {
      var name = queueName ?? typeof(T).FullName;
      var rKey = routingKey ?? typeof(T).FullName;
      this.queueDeclare = (IQueue)this.bunny.Queue(name).Bind(exchangeName, rKey).SetDurable();
      return this;
    }

    public IPublish<T> WithQueueDeclare(IQueue queueDeclare) {
      this.queueDeclare = queueDeclare;
      return this;
    }

    public void Dispose() => this.Dispose(true);

    private void Handlers(IModel channel, bool dismantle = false) {
      if (this.Mandatory) {
        if (dismantle) {
          channel.BasicReturn -= this.HandleReturn;
        } else {
          channel.BasicReturn += this.HandleReturn;
        }
      }

      if (!this.useConfirm) {
        return;
      }

      if (dismantle) {
        channel.BasicNacks -= this.HandleNack;
        channel.BasicAcks -= this.HandleAck;
      } else {
        channel.BasicNacks += this.HandleNack;
        channel.BasicAcks += this.HandleAck;
      }
    }

    private void HandleReturn(object sender, BasicReturnEventArgs eventArgs) => this.returnCallback(eventArgs);

    private void HandleAck(object sender, BasicAckEventArgs eventArgs) => this.ackCallback(eventArgs);

    private void HandleNack(object sender, BasicNackEventArgs eventArgs) => this.nackCallback(eventArgs);

    public static IBasicProperties ConstructProperties(IBasicProperties basicProperties, bool persistent, int? expires) {
      basicProperties.Persistent = persistent;
      basicProperties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
      basicProperties.Type = typeof(T).FullName;
      if (expires.HasValue) {
        basicProperties.Expiration = expires.Value.ToString();
      }

      basicProperties.CorrelationId = Guid.NewGuid().ToString();
      basicProperties.ContentType = contentType;
      basicProperties.ContentEncoding = contentEncoding;

      return basicProperties;
    }

    protected virtual void Dispose(bool disposing) {
      if (this.disposedValue) {
        return;
      }

      if (disposing) {
        this.Handlers(this.thisChannel.Channel, true);
        this.thisChannel.Dispose();
      }

      this.disposedValue = true;
    }
  }
}