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
    
    
    /// <summary>
    ///   Enter Queue DeclarationMode
    /// </summary>
    public IQueue Queue(string name) {
      var bunny = this.CheckGetBunny(name, "queue");
      return new DeclareQueue(bunny, name);
    }

    public bool PurgeQueue(string name) {
      var bunny = this.CheckGetBunny(name, "queue");
      return this.ExecuteOnChannel(bunny, model => model.QueuePurge(name));
    }

    public bool DeleteQueue(string queue, bool force = false) {
      var bunny = this.CheckGetBunny(queue, "queue");
      return this.ExecuteOnChannel(bunny, model => model.QueueDelete(queue, !force, !force));
    }

    public bool QueueExists(string queue) {
      return this.CheckGetBunny(queue, "queue").QueueExists(queue);
    }

    public IExchange Exchange(string exchangeName, string type = "direct") {
      var @base = this.CheckGetBunny(exchangeName, "exchange");
      return new DeclareExchange(@base, exchangeName, type);
    }

    public bool DeleteExchange(string exchangeName, bool force = false) {
      var bunny = this.CheckGetBunny(exchangeName, "exchange");
      return this.ExecuteOnChannel(bunny, model => model.ExchangeDelete(exchangeName, !force));
    }

    public bool ExchangeExists(string exchangeName) {
      return this.CheckGetBunny(exchangeName, "exchange").ExchangeExists(exchangeName);
    }

    public virtual void Declare() {
      throw DeclarationException.BaseNotValid();
    }
    
    private IBunny CheckGetBunny(string toCheck, string errorPrefix) {
      if (string.IsNullOrWhiteSpace(toCheck)) {
        throw DeclarationException.Argument(new ArgumentException($"{errorPrefix}-name must not be null-or-whitespace"));
      }

      if (toCheck.Length <= 255) {
        return this.Bunny;
      }

      throw DeclarationException.Argument(new ArgumentException($"{errorPrefix}-length must be less than or equal to 255 characters"));
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