namespace SharperBunny.Tests.Producer {
  using System;
  using System.Threading.Tasks;
  using RabbitMQ.Client.Events;
  using SharperBunny.Declare;
  using SharperBunny.Extensions;
  using SharperBunny.Interfaces;
  using SharperBunny.Tests.Connection;
  using Xunit;

  public class PublisherTests {
    private readonly string exchange = ""; // default --> " "

    [Fact]
    public async Task PublisherSimplySendsWithoutQueueReturnsFailure() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var publisher = bunny.Publisher<TestMessage>(this.exchange);

      var result = await publisher.SendAsync(new TestMessage());

      Assert.True(result.IsSuccess);
      bunny.Dispose();
    }

    [Fact]
    public async Task PublisherSimplySendsWitQueueReturnsSuccess() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var publisher = bunny.Publisher<TestMessage>(this.exchange);

      var result = await publisher
                     .WithQueueDeclare()
                     .SendAsync(new TestMessage());

      var success = await bunny.Setup().DeleteQueueAsync(typeof(TestMessage).FullName, force: true);

      Assert.True(result.IsSuccess);
      Assert.True(success);
      bunny.Dispose();
    }

    [Fact]
    public async Task ForceCreatesTheExchangeIfNotExists() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var publisher = bunny.Publisher<TestMessage>("test-exchange");

      var result = await publisher.SendAsync(new TestMessage(), true);

      Assert.True(result.IsSuccess);
      var removedExchange = await bunny.Setup().DeleteExchangeAsync("test-exchange", force: true);
      Assert.True(removedExchange);
      bunny.Dispose();
    }

    [Fact]
    public async Task ConfirmsAndAcksWork() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var queue = bunny.Setup()
        .Queue("constraint")
        .MaxLength(1)
        .QueueExpiry(1500)
        .Bind("amq.direct", "constraint-key")
        .OverflowReject();

      var isNacked = false;
      var isAcked = false;
      var publisher = bunny.Publisher<TestMessage>("amq.direct");
      Func<BasicNackEventArgs, Task> nacker = ea => {
        isNacked = true;
        return Task.CompletedTask;
      };
      Func<BasicAckEventArgs, Task> acker = ea => {
        isAcked = true;
        return Task.CompletedTask;
      };

      await publisher.WithQueueDeclare(queue)
        .WithConfirm(acker, nacker)
        .WithRoutingKey("constraint-key")
        .SendAsync(new TestMessage { Text = "Confirm-1st" });

      await publisher.WithQueueDeclare(queue)
        .WithConfirm(acker, nacker)
        .SendAsync(new TestMessage { Text = "Confirm-2nd" });

      Assert.True(isAcked);
      Assert.True(isNacked);
      bunny.Dispose();
    }

    [Fact]
    public async Task MandatoryFailsWithoutQueue() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);

      var isReturned = false;
      var publisher = bunny.Publisher<TestMessage>("amq.direct");
      Func<BasicReturnEventArgs, Task> nacker = ea => {
        isReturned = true;
        return Task.CompletedTask;
      };

      await publisher.AsMandatory(nacker)
        .WithRoutingKey("not-any-bound-queue")
        .SendAsync(new TestMessage());

      await Task.Delay(500);
      Assert.True(isReturned);
      bunny.Dispose();
    }

    [Fact]
    public async Task MandatoryWorksWithQueue() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);

      var isReturned = true;
      var publisher = bunny.Publisher<TestMessage>("amq.direct");
      Func<BasicReturnEventArgs, Task> nacker = ea => {
        isReturned = false;
        return Task.CompletedTask;
      };

      await publisher.AsMandatory(nacker)
        .WithQueueDeclare()
        .SendAsync(new TestMessage { Text = "Mandatory-succeeds" });

      var removed = await bunny.Setup().DeleteQueueAsync(typeof(TestMessage).FullName, force: true);

      Assert.True(isReturned);
      bunny.Dispose();
    }

    [Fact]
    public async Task MultiplePublishOnSinglePublisher() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var publisher = bunny.Publisher<TestMessage>("amq.direct");

      var queueName = "polymorph-queue";
      await publisher.WithQueueDeclare(queueName, "poly")
        .SendAsync(new OtherMessage());
      await publisher.SendAsync(new YetAnotherMessage());
      await Task.Delay(150);
      var count = bunny.Channel().MessageCount(queueName);

      Assert.Equal(2, (int)count);
      await bunny.Setup().DeleteQueueAsync(queueName, force: true);
    }

    [Fact]
    public async Task OverWriteRoutingKeySendsToDifferentQueuesEachTime() {
      var bunny = Bunny.ConnectSingle(ConnectSimple.BasicAmqp);
      var publisher = bunny.Publisher<TestMessage>("amq.direct");

      var queueName = "polymorph-queue-other";
      var queueNameYetOther = "polymorph-queue-yet-another";
      await publisher.WithQueueDeclare(queueName, "poly")
        .SendAsync(new OtherMessage());
      await publisher.WithQueueDeclare(queueNameYetOther, "poly-2")
        .WithRoutingKey("poly-2")
        .SendAsync(new YetAnotherMessage());

      var otherCount = bunny.Channel().MessageCount(queueName);
      var yetOtherCount = bunny.Channel().MessageCount(queueNameYetOther);

      Assert.Equal(1, (int)otherCount);
      Assert.Equal(1, (int)yetOtherCount);

      await bunny.Setup().DeleteQueueAsync(queueName, force: true);
      await bunny.Setup().DeleteQueueAsync(queueNameYetOther, force: true);
    }

    public class TestMessage {
      public string Text { get; set; } = "Test";
      public int Number { get; set; } = 42;
    }

    public class OtherMessage : TestMessage { }

    public class YetAnotherMessage : OtherMessage { }
  }
}