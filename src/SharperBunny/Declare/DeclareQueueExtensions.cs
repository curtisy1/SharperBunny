namespace SharperBunny.Declare {
  using System;
  using System.Threading.Tasks;
  using RabbitMQ.Client;
  using SharperBunny.Exceptions;
  using SharperBunny.Extensions;
  using SharperBunny.Interfaces;

  public static class DeclareQueueExtensions {
    #region Checks

    private static IBunny CheckGetBunny(IDeclare declare, string toCheck, string errorPrefix) {
      if (string.IsNullOrWhiteSpace(toCheck)) {
        var arg = new ArgumentException($"{errorPrefix}-name must not be null-or-whitespace");
        throw DeclarationException.Argument(arg);
      }

      if (toCheck.Length > 255) {
        var arg = new ArgumentException($"{errorPrefix}-length must be less than or equal to 255 character");
        throw DeclarationException.Argument(arg);
      }

      return declare.Bunny;
    }

    #endregion

    private static async Task<bool> ExecuteOnChannelAsync(IBunny bunny, Action<IModel> execute) {
      IModel channel = null;
      try {
        channel = bunny.Channel(true);
        await Task.Run(() => execute(channel));
        return true;
      } catch {
        return false;
      } finally {
        channel.Close();
      }
    }

    #region Queue

    /// <summary>
    ///   Enter Queue DeclarationMode
    /// </summary>
    public static IQueue Queue(this IDeclare declare, string name) {
      if (declare is DeclareBase == false) {
        throw DeclarationException.WrongType(typeof(DeclareBase), declare);
      }

      var bunny = CheckGetBunny(declare, name, "queue");
      return new DeclareQueue(bunny, name);
    }

    public static Task<bool> PurgeQueueAsync(this IDeclare declare, string name) {
      var bunny = CheckGetBunny(declare, name, "queue");
      return ExecuteOnChannelAsync(bunny, model => model.QueuePurge(name));
    }

    public static Task<bool> DeleteQueueAsync(this IDeclare declare, string queue, bool force = false) {
      var bunny = CheckGetBunny(declare, queue, "queue");
      return ExecuteOnChannelAsync(bunny, model => model.QueueDelete(queue, !force, !force));
    }

    public static Task<bool> QueueExistsAsync(this IDeclare declare, string queue) {
      var bunny = CheckGetBunny(declare, queue, "queue");
      return bunny.QueueExistsAsync(queue);
    }

    #endregion

    #region Exchange

    public static IExchange Exchange(this IDeclare declare, string exchangeName, string type = "direct") {
      var @base = CheckGetBunny(declare, exchangeName, "exchange");
      return new DeclareExchange(@base, exchangeName, type);
    }

    public static Task<bool> DeleteExchangeAsync(this IDeclare declare, string exchangeName, bool force = false) {
      var bunny = CheckGetBunny(declare, exchangeName, "exchange");
      return ExecuteOnChannelAsync(bunny, model => model.ExchangeDelete(exchangeName, !force));
    }

    public static Task<bool> ExchangeExistsAsync(this IDeclare declare, string exchangeName) {
      var bunny = CheckGetBunny(declare, exchangeName, "exchange");
      return bunny.ExchangeExistsAsync(exchangeName);
    }

    #endregion
  }
}