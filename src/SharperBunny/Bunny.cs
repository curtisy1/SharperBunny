namespace SharperBunny {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using RabbitMQ.Client;
  using RabbitMQ.Client.Exceptions;
  using SharperBunny.Connection;
  using SharperBunny.Extensions;
  using SharperBunny.Facade;
  using SharperBunny.Interfaces;

  public static class Bunny {
    internal static int RetryCount { get; set; } = 3;
    internal static int RetryPauseInMs { get; set; } = 1500;

    internal static readonly IList<AmqpTcpEndpoint> Endpoints = new List<AmqpTcpEndpoint>();

    /// <summary>
    ///   Create a permanent connection by using parameters.
    /// </summary>
    public static IBunny ConnectSingle(ConnectionParameters parameters) {
      Endpoints.Clear();
      Endpoints.Add(parameters.ToString().ParseEndpoint());
      return Connect();
    }

    /// <summary>
    ///   Create a permanent connection by using an amqp_uri.
    /// </summary>
    public static IBunny ConnectSingle(string amqpUri) {
      Endpoints.Clear();
      Endpoints.Add(amqpUri.ParseEndpoint());
      return Connect();
    }

    /// <summary>
    ///   Connect with fluent interface
    /// </summary>
    public static IConnectionPipe ConnectSingleWith() {
      return new ConnectionPipe();
    }

    /// <summary>
    ///   Connect to a cluster with a builder interface
    /// </summary>
    public static IConnectionCluster ClusterConnect() {
      return new ConnectionCluster();
    }

    internal static IBunny Connect() {
      var factory = new ConnectionFactory();
      var count = 0;
      while (count <= RetryCount) {
        try {
          return new MultiBunny(factory, Endpoints);
        } catch {
          count++;
          Thread.Sleep(RetryPauseInMs);
        }
      }

      throw new BrokerUnreachableException(new InvalidOperationException($"Broker not reachable at {Endpoints.FirstOrDefault()}"));
    }
  }
}