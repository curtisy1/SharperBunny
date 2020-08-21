using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client;
using SharperBunny.Exceptions;
using SharperBunny.Utils;

namespace SharperBunny.Declare {
    public class DeclareExchange : IExchange {
        private readonly IBunny _bunny;
        public IBunny Bunny => _bunny;
        public DeclareExchange (IBunny bunny, string name, string type) {
            _bunny = bunny;
            Name = name;
            ExchangeType = type;
        }

        public string Name { get; set; }
        internal bool Durable { get; set; } = false;
        internal bool AutoDelete { get; set; } = false;
        internal string ExchangeType { get; set; } = "direct";

        private Dictionary<string, object> _args = new Dictionary<string, object> ();

        public IExchange AlternateExchange (string alternate) {
            _args.Add ("alternate-exchange", alternate);
            return this;
        }

        public IExchange AsAutoDelete () {
            AutoDelete = true;
            return this;
        }

        public IExchange AsDurable () {
            Durable = true;
            return this;
        }

        public async Task DeclareAsync () {
            bool exists = await _bunny.ExchangeExistsAsync (Name);
            if (exists) {
                return;
            }
            IModel channel = null;
            try {
                channel = _bunny.Channel (newOne: true);

                await Task.Run (() => {
                    channel.ExchangeDeclare (Name, ExchangeType, Durable, AutoDelete, _args);
                });
            } catch (System.Exception exc) {
                throw DeclarationException.DeclareFailed (exc, "exchange-declare failed!");
            } finally {
                channel.Close ();
            }
        }

        internal bool _Internal { get; set; }
        public IExchange Internal () {
            _Internal = true;
            return this;
        }
    }
}