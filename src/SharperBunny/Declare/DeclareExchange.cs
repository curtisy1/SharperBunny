namespace SharperBunny.Declare {
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using RabbitMQ.Client;
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;
  using SharperBunny.Utils;

  public class DeclareExchange : IExchange {
    private readonly Dictionary<string, object> args = new Dictionary<string, object>();

    public DeclareExchange(IBunny bunny, string name, string type) {
      this.Bunny = bunny;
      this.Name = name;
      this.ExchangeType = type;
    }

    internal bool Durable { get; set; }
    internal bool AutoDelete { get; set; }
    internal string ExchangeType { get; set; } = "direct";

    public IBunny Bunny { get; }

    public string Name { get; set; }

    public IExchange AlternateExchange(string alternate) {
      this.args.Add("alternate-exchange", alternate);
      return this;
    }

    public IExchange AsAutoDelete() {
      this.AutoDelete = true;
      return this;
    }

    public IExchange AsDurable() {
      this.Durable = true;
      return this;
    }

    public async Task DeclareAsync() {
      var exists = await this.Bunny.ExchangeExistsAsync(this.Name);
      if (exists) {
        return;
      }

      IModel channel = null;
      try {
        channel = this.Bunny.Channel(true);

        await Task.Run(() => { channel.ExchangeDeclare(this.Name, this.ExchangeType, this.Durable, this.AutoDelete, this.args); });
      } catch (Exception exc) {
        throw DeclarationException.DeclareFailed(exc, "exchange-declare failed!");
      } finally {
        channel.Close();
      }
    }
  }
}