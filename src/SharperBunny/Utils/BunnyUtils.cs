namespace SharperBunny.Utils {
  using System;
  using System.Threading.Tasks;
  using RabbitMQ.Client.Exceptions;
  using SharperBunny.Connect;
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;

  public static class BunnyUtils {
    internal static async Task<bool> QueueExistsAsync(this IBunny bunny, string name) {
      try {
        var channel = bunny.Channel(true);
        var result = await new TaskFactory().StartNew(() => channel.QueueDeclarePassive(name));

        return true;
      } catch (OperationInterruptedException) {
        return false;
      } catch (Exception ex) {
        throw DeclarationException.DeclareFailed(ex);
      }
    }

    internal static async Task<bool> ExchangeExistsAsync(this IBunny bunny, string name) {
      try {
        var channel = bunny.Channel(true);
        await Task.Run(() => channel.ExchangeDeclarePassive(name));

        return true;
      } catch (OperationInterruptedException) {
        return false;
      } catch (Exception ex) {
        throw DeclarationException.DeclareFailed(ex);
      }
    }

    internal static PermanentChannel ToPermanentChannel(this IBunny bunny) {
      return new PermanentChannel(bunny);
    }
  }
}