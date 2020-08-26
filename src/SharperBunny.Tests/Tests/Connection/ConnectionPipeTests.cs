namespace SharperBunny.Tests.Connection {
  using FluentAssertions;
  using RabbitMQ.Client.Exceptions;
  using SharperBunny.Connection;
  using Xunit;

  public class ConnectionPipeTests {
    [Fact]
    public void Connect_WithNoParameters_ReturnsDefaults() {
      var connectionPipe = new ConnectionPipe();
      Bunny.RetryCount = 0;

      try {
        connectionPipe.Connect();
      } catch (BrokerUnreachableException) {
        // this is expected, we don't want to connect anyway
      }

      Bunny.Endpoints.Should().HaveCount(1).And.Contain(e => e.HostName == "localhost");
    }
  }
}