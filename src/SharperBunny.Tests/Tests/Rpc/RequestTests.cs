namespace SharperBunny.Tests.Rpc {
  using System.Threading.Tasks;
  using SharperBunny.Extensions;
  using SharperBunny.Tests.Connection;
  using Xunit;

  public class RequestTests {
    [Fact]
    public async Task DirectReplyWorks() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var rpcExchange = "rpc-exchange";

      await bunny.Respond<MyRequest, MyResponse>(rpcExchange, rq => new MyResponse())
        .StartRespondingAsync();

      var result = bunny.Request<MyRequest, MyResponse>(rpcExchange)
                     .Request(new MyRequest(), true);

      await Task.Delay(500);

      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Message);
    }

    [Fact]
    public async Task WithTemporaryQueueWorksAlso() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var rpcExchange = "rpc-exchange";

      await bunny.Respond<MyRequest, MyResponse>(rpcExchange, rq => new MyResponse())
        .StartRespondingAsync();

      var result = bunny.Request<MyRequest, MyResponse>(rpcExchange)
                     .Request(new MyRequest(), true);

      await Task.Delay(500);

      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Message);
    }

    private class MyRequest { }

    private class MyResponse { }
  }
}