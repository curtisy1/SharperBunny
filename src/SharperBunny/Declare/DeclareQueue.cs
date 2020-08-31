namespace SharperBunny.Declare {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using RabbitMQ.Client;
  using SharperBunny.Exceptions;
  using SharperBunny.Extensions;
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
      } catch (Exception exc) {
        throw DeclarationException.DeclareFailed(exc, "queue-declare failed");
      } finally {
        channel?.Close();
        this.wasDeclared = true;
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