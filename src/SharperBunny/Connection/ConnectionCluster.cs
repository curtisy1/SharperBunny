namespace SharperBunny.Connection {
  using System.Collections.Generic;
  using RabbitMQ.Client;
  using SharperBunny;
  using SharperBunny.Extensions;
  using SharperBunny.Interfaces;

  public class ConnectionCluster : IConnectionCluster {
    private readonly IList<AmqpTcpEndpoint> endpoints = new List<AmqpTcpEndpoint>();

    public IConnectionCluster AddNode(string amqpUri) {
      Bunny.Endpoints.Add(amqpUri.ParseEndpoint());
      return this;
    }

    public IConnectionCluster AddNode(IConnectionPipe parameters) {
      Bunny.Endpoints.Add(parameters.ToString("amqp", null).ParseEndpoint());
      return this;
    }

    public IConnectionCluster AddNode(ConnectionParameters pipe) {
      Bunny.Endpoints.Add(pipe.ToString("amqp", null).ParseEndpoint());
      return this;
    }

    public IBunny Connect() {
      return Bunny.Connect();
    }

    public IConnectionCluster WithRetries(int retry = 5, int timeout = 2) {
      Bunny.RetryCount = retry;
      Bunny.RetryPauseInMs = timeout;
      return this;
    }
  }
}