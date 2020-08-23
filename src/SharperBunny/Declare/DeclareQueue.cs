namespace SharperBunny.Declare {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using RabbitMQ.Client;
  using SharperBunny.Exceptions;
  using SharperBunny.Extensions;
  using SharperBunny.Interfaces;

  public class DeclareQueue : IQueue {
    private readonly Dictionary<string, object> arguments = new Dictionary<string, object>();
    private bool wasDeclared;

    public DeclareQueue(IBunny bunny, string name) {
      this.Name = name;
      this.Bunny = bunny;
    }

    private bool? Durable { get; set; } = false;
    public (string ex, string rKey)? BindingKey { get; set; }
    private bool? AutoDelete { get; set; }
    public IBunny Bunny { get; set; }
    public string Name { get; }

    public string RoutingKey => this.BindingKey.HasValue ? this.BindingKey.Value.rKey : this.Name;

    public void Declare() {
      if (this.wasDeclared) {
        return;
      }

      if (this.Bunny.QueueExists(this.Name)) {
        return;
      }

      IModel channel = null;
      try {
        channel = this.Bunny.Channel(true);

        this.Declare(channel);
        this.Bind(channel);
      } catch (Exception exc) {
        throw DeclarationException.DeclareFailed(exc, "queue-declare failed");
      } finally {
        channel?.Close();
        this.wasDeclared = true;
      }
    }

    public IQueue AddTag(string key, object value) {
      this.arguments.Add(key, value);
      return this;
    }

    public IQueue AsAutoDelete() {
      this.AutoDelete = true;
      return this;
    }

    public IQueue Bind(string exchangeName, string routingKey = "") {
      if (exchangeName == null) {
        throw DeclarationException.Argument(new ArgumentException("exchangename must not be null"));
      }

      this.BindingKey = (exchangeName, routingKey);
      return this;
    }

    public IQueue AsDurable() {
      this.Durable = true;
      return this;
    }

    private void Declare(IModel channel) {
      channel.QueueDeclare(this.Name,
                           this.Durable ?? true,
                           false,
                           this.AutoDelete ?? false,
                           this.arguments.Any() ? this.arguments : null);
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