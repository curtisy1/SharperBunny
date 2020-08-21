using System.Collections.Generic;
using RabbitMQ.Client;
using SharperBunny.Utils;

namespace SharperBunny.Connect {
    public class Cnnctr : IConnector {
        private readonly IList<AmqpTcpEndpoint> _endpoints = new List<AmqpTcpEndpoint> ();
        public IConnector AddNode (string amqp_uri) {
            _endpoints.Add (amqp_uri.ParseEndpoint ());
            return this;
        }

        public IConnector AddNode (IConnectPipe pipe) {
            string amqp = pipe.ToString ("amqp", null);
            _endpoints.Add (amqp.ParseEndpoint ());
            return this;
        }

        public IConnector AddNode (ConnectionParameters pipe) {
            string amqp = pipe.ToString ("amqp", null);
            _endpoints.Add (amqp.ParseEndpoint ());
            return this;
        }

        public IBunny Connect () {
            var factory = new ConnectionFactory ();

            return new Facade.MultiBunny (factory, _endpoints);
        }

        public IConnector WithRetry (int retry = 5, int timeout = 2) {
            return this;
        }
    }
}