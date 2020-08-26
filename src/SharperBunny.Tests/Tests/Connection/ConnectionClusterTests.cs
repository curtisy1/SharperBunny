namespace SharperBunny.Tests.Connection {
  using FluentAssertions;
  using RabbitMQ.Client.Exceptions;
  using SharperBunny.Connection;
  using SharperBunny.Interfaces;
  using Xunit;

  public class ConnectionClusterTests {
    private static readonly string VirtualHost = "unittests";
    internal static string BasicAmqp => $"amqp://guest:guest@localhost:5672/{VirtualHost}";

    internal static IBunny Connect() {
      return Bunny.ConnectSingle(BasicAmqp);
    }

    [Fact]
    public void AddNode_AddsNewUriToEndpoints() {
      var connectionCluster = new ConnectionCluster();
      
      connectionCluster.AddNode(BasicAmqp);

      Bunny.Endpoints.Should().HaveCount(1).And.Contain(x => x.HostName == "localhost");
    }

    [Fact]
    public void Connect_WithoutPreviousCalls_ReturnsDefaultConnection() {
      var connectionCluster = new ConnectionCluster();
      Bunny.RetryCount = 0;
      
      try {
        connectionCluster.Connect(); // will fail because it's not yet implemented
      } catch (BrokerUnreachableException) {
        // this is expected, we don't want to connect anyway
      }

      Bunny.Endpoints.Should().HaveCount(1).And.Contain(x => x.HostName == "localhost");
    }
  }
}