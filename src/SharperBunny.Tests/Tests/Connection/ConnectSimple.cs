namespace SharperBunny.Tests.Connection {
  using SharperBunny.Interfaces;
  using Xunit;

  public class ConnectSimple {
    private static readonly string VirtualHost = "unittests";
    internal static string BasicAmqp => $"amqp://guest:guest@localhost:5672/{VirtualHost}";

    internal static IBunny Connect() {
      return Bunny.ConnectSingle(BasicAmqp);
    }

    [Fact]
    public void ConnectToSingleNodeViaAmqp() {
      var bunny = Bunny.ConnectSingle(BasicAmqp);
      Assert.NotNull(bunny);
    }

    [Fact]
    public void ConnectToSingleNodeWithConnectionPipe() {
      var pipe = Bunny.ConnectSingleWith();
      // not configuring anything uses default
      var bunny = pipe.Connect();

      Assert.NotNull(bunny);
    }

    [Fact]
    public void ConfigurePipeWorks() {
      var pipe = Bunny.ConnectSingleWith();
      pipe.ToHost()
        .ToVirtualHost("unittests")
        .ToPort()
        .AuthenticatePlain();

      var bunny = pipe.Connect();

      Assert.NotNull(bunny);
    }

    [Fact]
    public void ConnectMultipleFailsFirstConnectsSecond() {
      var node1 = $"amqp://guest:guest@localhost:5673/{VirtualHost}";
      var node2 = $"amqp://guest:guest@localhost:5672/{VirtualHost}";

      var multi = Bunny.ClusterConnect();
      multi.AddNode(node1);
      multi.AddNode(node2);

      var bunny = multi.Connect();

      Assert.NotNull(bunny);
    }

    [Fact]
    public void ReconnectWithCluster() {
      // TODO
      // start cluster
      // connect with all nodes
      // remove one node
      // still connected (reconnected)
      //  --> to other node --> check channel, connection etc.
    }
  }
}