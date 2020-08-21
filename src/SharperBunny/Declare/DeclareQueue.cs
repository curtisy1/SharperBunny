using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RabbitMQ.Client;
using SharperBunny.Exceptions;
using SharperBunny.Utils;

[assembly : InternalsVisibleTo ("tests")]
namespace SharperBunny.Declare {
    public class DeclareQueue : IQueue {
        public IBunny Bunny { get; set; }
        public string Name { get; }
        internal DeclareQueue (IBunny bunny, string name) {
            Name = name;
            Bunny = bunny;
        }
        internal bool? Durable { get; set; } = false;
        internal (string ex, string rKey) ? BindingKey { get; set; }
        public string RoutingKey {
            get {
                if (BindingKey.HasValue)
                    return BindingKey.Value.rKey;
                else
                    return Name;
            }
        }
        internal bool? AutoDelete { get; set; }
        private readonly Dictionary<string, object> _arguments = new Dictionary<string, object> ();
        private bool _wasDeclared;
        public async Task DeclareAsync () {
            if (_wasDeclared) {
                return;
            }
            bool exists = await Bunny.QueueExistsAsync (Name);
            if (exists) {
                return;
            }
            IModel channel = null;
            try {
                channel = Bunny.Channel (newOne: true);

                await Declare (channel);
                await Bind (channel);
            } catch (System.Exception exc) {
                throw DeclarationException.DeclareFailed (exc, "queue-declare failed");
            } finally {
                channel.Close ();
                _wasDeclared = true;
            }
        }

        public IQueue AddTag (string key, object value) {
            _arguments.Add (key, value);
            return this;
        }

        private Task Declare (IModel channel) {
            return Task.Run (() =>
                channel.QueueDeclare (Name,
                    durable : Durable.HasValue ? Durable.Value : true,
                    exclusive : false,
                    autoDelete : AutoDelete.HasValue ? AutoDelete.Value : false,
                    arguments : _arguments.Any () ? _arguments : null)
            );
        }

        private async Task Bind (IModel channel) {
            if (BindingKey.HasValue) {
                var (ex, bkey) = BindingKey.Value;
                await Task.Run (() => {
                    if (channel.IsClosed) {
                        channel = Bunny.Channel (newOne: true);
                    }
                    channel.QueueBind (Name, ex, bkey, null);
                });
            }
        }

        public IQueue AsAutoDelete () {
            AutoDelete = true;
            return this;
        }

        public IQueue Bind (string exchangeName, string routingKey) {
            if (exchangeName == null || string.IsNullOrWhiteSpace (routingKey)) {
                throw DeclarationException.Argument (new System.ArgumentException ("exchangename must not be null and routingKey must not be Null, Empty or Whitespace"));
            }
            BindingKey = (exchangeName, routingKey);
            return this;
        }

        public IQueue AsDurable () {
            Durable = true;
            return this;
        }
    }
}