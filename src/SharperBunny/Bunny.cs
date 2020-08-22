namespace SharperBunny {
  using System;
  using System.Threading;
  using RabbitMQ.Client;
  using RabbitMQ.Client.Exceptions;
  using SharperBunny.Connect;
  using SharperBunny.Interfaces;

  public static class Bunny {
    /// <summary>
    ///   Default is 3
    /// </summary>
    public static uint RetryCount { get; set; } = 3;

    /// <summary>
    ///   Default is 1500 ms
    /// </summary>
    public static uint RetryPauseInMs { get; set; } = 1500;

    /// <summary>
    ///   Create a permanent connection by using parameters.
    /// </summary>
    public static IBunny ConnectSingle(ConnectionParameters parameters) {
      return Connect(parameters);
    }

    /// <summary>
    ///   Create a permanent connection by using an amqp_uri.
    /// </summary>
    public static IBunny ConnectSingle(string amqpUri) {
      return Connect(new AmqpTransport { Amqp = amqpUri });
    }

    /// <summary>
    ///   Connect with fluent interface
    /// </summary>
    public static IConnectPipe ConnectSingleWith() {
      return new ConnectionPipe();
    }

    /// <summary>
    ///   Connect to a cluster with a builder interface
    /// </summary>
    public static IConnector ClusterConnect() {
      return new Cnnctr();
    }

    private static IBunny Connect(IFormattable formattable) {
      var factory = new ConnectionFactory();
      var amqp = formattable.ToString("amqp", null);
      factory.Uri = new Uri(amqp);

      var count = 0;
      while (count <= RetryCount) {
        try {
          return new Facade.Bunny(factory);
        } catch {
          count++;
          Thread.Sleep((int)RetryPauseInMs);
        }
      }

      throw new BrokerUnreachableException(new InvalidOperationException($"cannot find any broker at {amqp}"));
    }

    private class AmqpTransport : IFormattable {
      public string Amqp { get; set; }

      public string ToString(string format, IFormatProvider formatProvider) {
        return this.Amqp;
      }
    }
  }
}