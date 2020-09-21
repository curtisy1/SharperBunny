namespace SharperBunny.Declare {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using RabbitMQ.Client;
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;

  public class DeclareQueue : DeclareBase, IQueue {
    private readonly Dictionary<string, object> arguments = new Dictionary<string, object>();
    private bool wasDeclared;

    public DeclareQueue(IBunny bunny, string name)
      : base(bunny, name) { }

    public (string ex, string rKey)? BindingKey { get; set; }
    
    private bool Exclusive { get; set; }

    public string RoutingKey => this.BindingKey.HasValue ? this.BindingKey.Value.rKey : this.Name;

    public override void Declare() {
      if (this.wasDeclared) {
        return;
      }

      if (this.Bunny.QueueExists(this.Name)) {
        return;
      }

      IModel channel = null;
      try {
        channel = this.Bunny.Channel(true);
        channel.QueueDeclare(this.Name, this.Durable, this.Exclusive, this.AutoDelete, this.arguments.Any() ? this.arguments : null);
        this.Bind(channel);
        this.wasDeclared = true;
      } catch (Exception exc) {
        throw DeclarationException.DeclareFailed(exc, "queue-declare failed");
      } finally {
        channel?.Close();
      }
    }

    public IQueue SetExclusive(bool exclusive = false) {
      this.Exclusive = exclusive;
      return this;
    }

    public IQueue AddTag(string key, object value) {
      this.arguments.Add(key, value);
      return this;
    }

    public IQueue Bind(string exchangeName, string routingKey = "") {
      if (exchangeName == null) {
        throw DeclarationException.Argument(new ArgumentException("exchangename must not be null"));
      }

      this.BindingKey = (exchangeName, routingKey);
      return this;
    }
    
    /// <summary>
    ///   Republish Messages to this Exchange if the are either expired, or not requeued on BasicReject/BasicNack.
    /// </summary>
    public IQueue DeadLetterExchange(string deadLetterExchange) {
      return this.AddTag("x-dead-letter-exchange", deadLetterExchange);
    }

    /// <summary>
    ///   If send to the DeadLetter Exchange, use this routing-key instead of the original.
    /// </summary>
    public IQueue DeadLetterRoutingKey(string routingKey) {
      return this.AddTag("x-dead-letter-routing-key", routingKey);
    }

    /// <summary>
    ///   Set a time when the Queue will expire if no action is taken on the queue
    /// </summary>
    public IQueue QueueExpiry(int expiry) {
      return this.AddTag("x-expires", expiry);
    }

    /// <summary>
    ///   Set max length of Messages on the Queue
    /// </summary>
    public IQueue MaxLength(int length) {
      return this.AddTag("x-max-length", length);
    }

    /// <summary>
    ///   Set max bytes on this Queue.
    /// </summary>
    public IQueue MaxLengthBytes(int lengthBytes) {
      return this.AddTag("x-max-length-bytes", lengthBytes);
    }

    /// <summary>
    ///   Define this Queue as Lazy. Writes all messages automatically to Disk.
    /// </summary>
    public IQueue AsLazy() {
      return this.AddTag("x-queue-mode", "lazy");
    }

    /// <summary>
    ///   Define all incoming messages to this queue with a Time to live (Message expiry)
    /// </summary>
    public IQueue WithTtl(uint ttl) {
      return this.AddTag("x-message-ttl", ttl);
    }

    /// <summary>
    ///   only takes effect if MaxLength/MaxLengthBytes is set and is overflown
    /// </summary>
    public IQueue OverflowReject() {
      return this.AddTag("x-overflow", "reject-publish");
    }

    private void Bind(IModel channel) {
      if (!this.BindingKey.HasValue) {
        return;
      }

      var (ex, bkey) = this.BindingKey.Value;
      if (channel.IsClosed) {
        channel = this.Bunny.Channel(true);
      }

      channel.QueueBind(this.Name, ex, bkey, null);
    }
  }
}