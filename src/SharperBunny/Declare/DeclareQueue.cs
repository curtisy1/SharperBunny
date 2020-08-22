using System.Runtime.CompilerServices;

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

    internal DeclareQueue(IBunny bunny, string name) {
      this.Name = name;
      this.Bunny = bunny;
    }

    private bool? Durable { get; set; } = false;
    internal (string ex, string rKey)? BindingKey { get; set; }
    private bool? AutoDelete { get; set; }
    public IBunny Bunny { get; set; }
    public string Name { get; }

    public string RoutingKey => this.BindingKey.HasValue ? this.BindingKey.Value.rKey : this.Name;

    public async Task DeclareAsync() {
      if (this.wasDeclared) {
        return;
      }

      var exists = await this.Bunny.QueueExistsAsync(this.Name);
      if (exists) {
        return;
      }

      IModel channel = null;
      try {
        channel = this.Bunny.Channel(true);

        await this.Declare(channel);
        await this.Bind(channel);
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

    public IQueue Bind(string exchangeName, string routingKey) {
      if (exchangeName == null || string.IsNullOrWhiteSpace(routingKey)) {
        throw DeclarationException.Argument(new ArgumentException("exchangename must not be null and routingKey must not be Null, Empty or Whitespace"));
      }

      this.BindingKey = (exchangeName, routingKey);
      return this;
    }

    public IQueue AsDurable() {
      this.Durable = true;
      return this;
    }

    private Task Declare(IModel channel) {
      return Task.Run(() =>
                        channel.QueueDeclare(this.Name,
                                             this.Durable.HasValue ? this.Durable.Value : true,
                                             false,
                                             this.AutoDelete.HasValue ? this.AutoDelete.Value : false,
                                             this.arguments.Any() ? this.arguments : null));
    }

    private async Task Bind(IModel channel) {
      if (this.BindingKey.HasValue) {
        var (ex, bkey) = this.BindingKey.Value;
        await Task.Run(() => {
          if (channel.IsClosed) {
            channel = this.Bunny.Channel(true);
          }

          channel.QueueBind(this.Name, ex, bkey, null);
        });
      }
    }
  }
}