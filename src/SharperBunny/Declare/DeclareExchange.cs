namespace SharperBunny.Declare {
  using System;
  using System.Collections.Generic;
  using RabbitMQ.Client;
  using SharperBunny.Exceptions;
  using SharperBunny.Extensions;
  using SharperBunny.Interfaces;

  public class DeclareExchange : DeclareBase, IExchange {
    private readonly Dictionary<string, object> args = new Dictionary<string, object>();

    public DeclareExchange(IBunny bunny, string name, string type)
      : base(bunny, name) {
      this.ExchangeType = type;
    }

    internal string ExchangeType { get; set; } = "direct";

    public IExchange AlternateExchange(string alternate) {
      this.args.Add("alternate-exchange", alternate);
      return this;
    }

    public override void Declare() {
      if (this.Bunny.ExchangeExists(this.Name)) {
        return;
      }

      IModel channel = null;
      try {
        channel = this.Bunny.Channel(true);

        channel.ExchangeDeclare(this.Name, this.ExchangeType, this.Durable, this.AutoDelete, this.args);
      } catch (Exception exc) {
        throw DeclarationException.DeclareFailed(exc, "exchange-declare failed!");
      } finally {
        channel?.Close();
      }
    }
  }
}