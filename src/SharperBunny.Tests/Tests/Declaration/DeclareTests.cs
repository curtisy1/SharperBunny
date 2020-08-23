namespace SharperBunny.Tests.Declaration {
  using System;
  using System.Threading.Tasks;
  using SharperBunny.Declare;
  using SharperBunny.Exceptions;
  using SharperBunny.Extensions;
  using SharperBunny.Interfaces;
  using SharperBunny.Tests.Connection;
  using Xunit;

  public class DeclareTests {
    [Fact]
    public void QueueCallThrowsIfNotDeclareBase() {
      var fail = new PurposeIsFail();
      Assert.Throws<DeclarationException>(() => fail.Queue("name"));
    }

    [Fact]
    public void QueueCallReturnsTypeQueueDeclareIfIsBase() {
      var @base = new DeclareBase();
      var queue = @base.Queue("my-queue");
      Assert.Equal(typeof(DeclareQueue), queue.GetType());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("255")]
    public void ThrowsOnNameNullOrToLong(string name) {
      if (name == "255") {
        name = name.PadRight(500, '-');
      }

      var @base = new DeclareBase();
      Assert.Throws<DeclarationException>(() => @base.Queue(name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("255")]
    public void CheckBindingDeclare(string name) {
      if (name == "255") {
        name = name.PadRight(500, '-');
      }

      var @base = new DeclareQueue(ConnectSimple.Connect(), "queue");
      Assert.Throws<DeclarationException>(() => @base.Bind(name, ""));
    }

    [Fact]
    public void BindAsSetsBindingKeyOn() {
      var @base = new DeclareQueue(ConnectSimple.Connect(), "queue");
      @base.Bind("ex", "bind-key");
      Assert.Equal("bind-key", @base.BindingKey.HasValue ? @base.BindingKey.Value.rKey : "null");
    }

    [Fact]
    public async Task DeclareAndBindDefaultAmqDirectSucceeds() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var declare = bunny.Setup()
        .Queue("bind-test")
        .Bind("amq.direct", "bind-test-key")
        .AsDurable()
        .QueueExpiry(1500)
        .WithTtl(500)
        .MaxLength(10);

      await declare.Declare();

      Assert.Equal(1, 1);
    }

    private class PurposeIsFail : IDeclare {
      public IBunny Bunny => throw new NotImplementedException();

      public Task Declare() {
        throw new NotImplementedException();
      }
    }
  }
}