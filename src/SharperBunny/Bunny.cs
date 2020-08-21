using System;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using SharperBunny.Connect;

namespace SharperBunny {
    public static class Bunny {
        ///<summary>
        /// Default is 3
        ///</summary>
        public static uint RetryCount { get; set; } = 3;
        ///<summary>
        /// Default is 1500 ms
        ///</summary>
        public static uint RetryPauseInMS { get; set; } = 1500;
        ///<summary>
        /// Create a permanent connection by using parameters.
        ///</summary>
        public static IBunny ConnectSingle (ConnectionParameters parameters) {
            return Connect (parameters);
        }
        ///<summary>
        /// Create a permanent connection by using an amqp_uri.
        ///</summary>
        public static IBunny ConnectSingle (string amqp_uri) {
            return Connect (new AmqpTransport { AMQP = amqp_uri });
        }

        private class AmqpTransport : IFormattable {
            public string AMQP { get; set; }

            public string ToString (string format, IFormatProvider formatProvider) {
                return AMQP;
            }
        }
        ///<summary>
        /// Connect with fluent interface
        ///</summary>
        public static IConnectPipe ConnectSingleWith () {
            return new ConnectionPipe ();
        }

        ///<summary>
        /// Connect to a cluster with a builder interface
        ///</summary>
        public static IConnector ClusterConnect () {
            return new Cnnctr ();
        }

        private static IBunny Connect (IFormattable formattable) {
            var factory = new ConnectionFactory ();
            var amqp = formattable.ToString ("amqp", null);
            factory.Uri = new Uri (amqp);

            int count = 0;
            while (count <= RetryCount) {
                try {
                    return new Facade.Bunny (factory);
                } catch {
                    count++;
                    Thread.Sleep ((int) RetryPauseInMS);
                }
            }
            throw new BrokerUnreachableException (new InvalidOperationException ($"cannot find any broker at {amqp}"));
        }
    }
}