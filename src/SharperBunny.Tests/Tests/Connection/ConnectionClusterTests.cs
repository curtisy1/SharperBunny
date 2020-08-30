namespace SharperBunny.Tests.Connection {
  using FluentAssertions;
  using RabbitMQ.Client.Exceptions;
  using SharperBunny.Connection;
  using Xunit;

  public class ConnectionClusterTests {
    private const string basicAmqp = "amqp://guest:guest@localhost:5672";

    [Fact]
    public void AddNode_AddsNewUriToEndpoints() {
      var connectionCluster = new ConnectionCluster();
      
      connectionCluster.AddNode(basicAmqp);

      Bunny.Endpoints.Should().HaveCount(1).And.Contain(x => x.HostName == "localhost");
      Bunny.Endpoints.Clear();
    }

    [Fact]
    public void Connect_WithoutPreviousCalls_ReturnsDefaultConnection() {
      var connectionCluster = new ConnectionCluster();
      Bunny.RetryCount = 0;
      
      try {
        connectionCluster.Connect();
      } catch (BrokerUnreachableException) {
        // this is expected, we don't want to connect anyway
      }

      Bunny.Endpoints.Should().HaveCount(1).And.Contain(x => x.HostName == "localhost");
      Bunny.Endpoints.Clear();
    }
  }
}