using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SharperBunny.Tests")]

namespace SharperBunny.Connection {
  using System.Linq;
  using SharperBunny.Extensions;
  using SharperBunny.Interfaces;

  public class ConnectionCluster : IConnectionCluster {
    private const string defaultConnection = "amqp://guest:guest@localhost:5672/";

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
      if (!Bunny.Endpoints.Any()) {
        Bunny.Endpoints.Add(defaultConnection.ParseEndpoint());
      }

      return Bunny.Connect();
    }

    public IConnectionCluster WithRetries(int retry = 5, int timeout = 1500) {
      Bunny.RetryCount = retry;
      Bunny.RetryPauseInMs = timeout;
      return this;
    }

    public IConnectionCluster UseAsyncEvents(bool useAsync = true) {
      Bunny.UseAsyncEvents = useAsync;
      return this;
    }
  }
}