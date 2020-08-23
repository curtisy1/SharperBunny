namespace SharperBunny.Tests.Declaration {
  using System.Threading.Tasks;
  using SharperBunny.Declare;
  using SharperBunny.Extensions;
  using SharperBunny.Interfaces;
  using SharperBunny.Tests.Connection;
  using Xunit;

  public class DeleteTests {
    [Fact]
    public async Task DeclareAndDeleteQueueNotExistsAfterWards() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var declare = bunny.Setup()
        .Queue("to-delete");

      declare.Declare();
      var exists = declare.QueueExists(declare.Name);
      Assert.True(exists);
      declare.DeleteQueue(declare.Name);
      exists = declare.QueueExists(declare.Name);
      Assert.False(exists);
    }

    [Fact]
    public async Task DeclareAndDeleteExchangeNotExistsAfterWards() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var declare = bunny.Setup()
        .Exchange("to-delete-ex", "fanout");

      declare.Declare();
      var exists = declare.ExchangeExists(declare.Name);
      Assert.True(exists);
      declare.DeleteExchange(declare.Name);
      exists = declare.ExchangeExists(declare.Name);
      Assert.False(exists);
    }
  }
}