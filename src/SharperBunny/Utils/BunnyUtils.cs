using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using SharperBunny.Connect;
using SharperBunny.Exceptions;

namespace SharperBunny.Utils {
    public static class BunnyUtils {
        internal static async Task<bool> QueueExistsAsync (this IBunny bunny, string name) {
            try {
                var channel = bunny.Channel (newOne: true);
                var result = await new TaskFactory ().StartNew<QueueDeclareOk> (() => channel.QueueDeclarePassive (name));

                return true;
            } catch (RabbitMQ.Client.Exceptions.OperationInterruptedException) {
                return false;
            } catch (Exception ex) {
                throw DeclarationException.DeclareFailed (ex);
            }
        }

        internal static async Task<bool> ExchangeExistsAsync (this IBunny bunny, string name) {
            try {
                var channel = bunny.Channel (newOne: true);
                await Task.Run (() => channel.ExchangeDeclarePassive (name));

                return true;
            } catch (RabbitMQ.Client.Exceptions.OperationInterruptedException) {
                return false;
            } catch (Exception ex) {
                throw DeclarationException.DeclareFailed (ex);
            }
        }

        internal static PermanentChannel ToPermanentChannel (this IBunny bunny) => new PermanentChannel (bunny);
    }
}