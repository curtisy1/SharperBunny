using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SharperBunny.Tests")]

namespace SharperBunny.Connection {
  using System.Linq;
  using SharperBunny.Interfaces;

  public class ConnectionCluster : IConnectionCluster {
    private const string defaultConnection = "amqp://guest:guest@localhost:5672/";

    public IConnectionCluster AddNode(string amqpUri) {
      Bunny.Endpoints.Add(amqpUri);
      return this;
    }

    public IConnectionCluster AddNode(IConnectionPipe parameters) {
      Bunny.Endpoints.Add(parameters.ToString("amqp", null));
      return this;
    }

    public IConnectionCluster AddNode(ConnectionParameters pipe) {
      Bunny.Endpoints.Add(pipe.ToString("amqp", null));
      return this;
    }

    public IBunny Connect() {
      if (!Bunny.Endpoints.Any()) {
        Bunny.Endpoints.Add(defaultConnection);
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