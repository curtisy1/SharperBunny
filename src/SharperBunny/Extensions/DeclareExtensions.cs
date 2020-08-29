namespace SharperBunny.Extensions {
  using System;
  using RabbitMQ.Client;
  using SharperBunny.Declare;
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;

  public static class DeclareExtensions {
    private static IBunny CheckGetBunny(IDeclare declare, string toCheck, string errorPrefix) {
      if (string.IsNullOrWhiteSpace(toCheck)) {
        throw DeclarationException.Argument(new ArgumentException($"{errorPrefix}-name must not be null-or-whitespace"));
      }

      if (toCheck.Length <= 255) {
        return declare.Bunny;
      }

      throw DeclarationException.Argument(new ArgumentException($"{errorPrefix}-length must be less than or equal to 255 characters"));
    }


    private static bool ExecuteOnChannel(IBunny bunny, Action<IModel> execute) {
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


    /// <summary>
    ///   Enter Queue DeclarationMode
    /// </summary>
    public static IQueue Queue(this IDeclare declare, string name) {
      if (declare is not DeclareBase) {
        throw DeclarationException.WrongType(typeof(DeclareBase), declare);
      }

      var bunny = CheckGetBunny(declare, name, "queue");
      return new DeclareQueue(bunny, name);
    }

    public static bool PurgeQueue(this IDeclare declare, string name) {
      var bunny = CheckGetBunny(declare, name, "queue");
      return ExecuteOnChannel(bunny, model => model.QueuePurge(name));
    }

    public static bool DeleteQueue(this IDeclare declare, string queue, bool force = false) {
      var bunny = CheckGetBunny(declare, queue, "queue");
      return ExecuteOnChannel(bunny, model => model.QueueDelete(queue, !force, !force));
    }

    public static bool QueueExists(this IDeclare declare, string queue) {
      return CheckGetBunny(declare, queue, "queue").QueueExists(queue);
    }


    public static IExchange Exchange(this IDeclare declare, string exchangeName, string type = "direct") {
      var @base = CheckGetBunny(declare, exchangeName, "exchange");
      return new DeclareExchange(@base, exchangeName, type);
    }

    public static bool DeleteExchange(this IDeclare declare, string exchangeName, bool force = false) {
      var bunny = CheckGetBunny(declare, exchangeName, "exchange");
      return ExecuteOnChannel(bunny, model => model.ExchangeDelete(exchangeName, !force));
    }

    public static bool ExchangeExists(this IDeclare declare, string exchangeName) {
      return CheckGetBunny(declare, exchangeName, "exchange").ExchangeExists(exchangeName);
    }
  }
}