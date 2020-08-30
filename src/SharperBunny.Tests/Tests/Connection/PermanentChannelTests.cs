namespace SharperBunny.Tests.Connection {
  using NSubstitute;
  using SharperBunny.Connection;
  using SharperBunny.Interfaces;
  using Xunit;

  public class PermanentChannelTests {
    [Fact]
    public void Model_GettingEmpty_ReturnsNew() {
      var substituteBunny = Substitute.For<IBunny>();
      var permanentChannel = new PermanentChannel(substituteBunny);

      var model = permanentChannel.Channel;

      substituteBunny.Received().Channel(true);
    }
  }
}