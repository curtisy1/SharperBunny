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

      await declare.DeclareAsync();
      var exists = await declare.QueueExistsAsync(declare.Name);
      Assert.True(exists);
      await declare.DeleteQueueAsync(declare.Name);
      exists = await declare.QueueExistsAsync(declare.Name);
      Assert.False(exists);
    }

    [Fact]
    public async Task DeclareAndDeleteExchangeNotExistsAfterWards() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var declare = bunny.Setup()
        .Exchange("to-delete-ex", "fanout");

      await declare.DeclareAsync();
      var exists = await declare.ExchangeExistsAsync(declare.Name);
      Assert.True(exists);
      await declare.DeleteExchangeAsync(declare.Name);
      exists = await declare.ExchangeExistsAsync(declare.Name);
      Assert.False(exists);
    }
  }
}