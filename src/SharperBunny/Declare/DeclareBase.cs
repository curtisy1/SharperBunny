namespace SharperBunny.Declare {
  using System;
  using RabbitMQ.Client;
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   entry for declaration builder
  /// </summary>
  public class DeclareBase : IDeclare {
    public IBunny Bunny { get; set; }
    
    public string Name { get; set; }

    internal bool Durable { get; set; }
    
    internal bool AutoDelete { get; set; }

    public DeclareBase(IBunny bunny) {
      this.Bunny = bunny;
    }

    public DeclareBase(IBunny bunny, string name) {
      this.Bunny = bunny;
      this.Name = name;
    }

    public virtual IDeclare SetDurable(bool durable = true) {
      this.Durable = durable;
      return this;
    }

    public virtual IDeclare SetAutoDelete(bool autoDelete = false) {
      this.AutoDelete = autoDelete;
      return this;
    }

    public bool PurgeQueue(string name) => this.ExecuteOnChannel(this.Bunny, model => model.QueuePurge(name));

    public bool DeleteQueue(string queue, bool force = false)
      => this.ExecuteOnChannel(this.Bunny, model => model.QueueDelete(queue, !force, !force));

    public bool QueueExists(string queue) => this.Bunny.QueueExists(queue);

    public bool DeleteExchange(string exchangeName, bool force = false)
      => this.ExecuteOnChannel(this.Bunny, model => model.ExchangeDelete(exchangeName, !force));

    public bool ExchangeExists(string exchangeName) => this.Bunny.ExchangeExists(exchangeName);

    public virtual void Declare() {
      throw DeclarationException.BaseNotValid();
    }

    private bool ExecuteOnChannel(IBunny bunny, Action<IModel> execute) {
      IModel channel = null;
      try {
        channel = bunny.Channel(true);
        execute(channel);
        return true;
      } catch {
        return false;
      } finally {
        channel?.Close();
      }
    }
  }
}