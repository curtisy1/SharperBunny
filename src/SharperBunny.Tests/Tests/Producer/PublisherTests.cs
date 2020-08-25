namespace SharperBunny.Tests.Producer {
  using System;
  using System.Threading.Tasks;
  using RabbitMQ.Client.Events;
  using SharperBunny.Extensions;
  using SharperBunny.Tests.Connection;
  using Xunit;

  public class PublisherTests {
    private readonly string exchange = ""; // default --> " "

    [Fact]
    public async Task PublisherSimplySendsWithoutQueueReturnsFailure() {
      var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      var publisher = bunny.Publisher<TestMessage>(this.exchange);

      var result = publisher.Send(new TestMessage());

      Assert.True(result.IsSuccess);
      bunny.Dispose();
    }

    [Fact]
    public async Task PublisherSimplySendsWitQueueReturnsSuccess() {
      var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      var publisher = bunny.Publisher<TestMessage>(this.exchange);

      var result = publisher
        .WithQueueDeclare()
        .Send(new TestMessage());

      var success = bunny.Setup().DeleteQueue(typeof(TestMessage).FullName, true);

      Assert.True(result.IsSuccess);
      Assert.True(success);
      bunny.Dispose();
    }

    [Fact]
    public async Task ForceCreatesTheExchangeIfNotExists() {
      var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      var publisher = bunny.Publisher<TestMessage>("test-exchange");

      var result = publisher.Send(new TestMessage(), true);

      Assert.True(result.IsSuccess);
      var removedExchange = bunny.Setup().DeleteExchange("test-exchange", true);
      Assert.True(removedExchange);
      bunny.Dispose();
    }

    [Fact]
    public async Task ConfirmsAndAcksWork() {
      var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
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

      publisher.WithQueueDeclare(queue)
        .WithConfirm(acker, nacker)
        .WithRoutingKey("constraint-key")
        .Send(new TestMessage { Text = "Confirm-1st" });

      publisher.WithQueueDeclare(queue)
        .WithConfirm(acker, nacker)
        .Send(new TestMessage { Text = "Confirm-2nd" });

      Assert.True(isAcked);
      Assert.True(isNacked);
      bunny.Dispose();
    }

    [Fact]
    public async Task MandatoryFailsWithoutQueue() {
      var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);

      var isReturned = false;
      var publisher = bunny.Publisher<TestMessage>("amq.direct");
      Func<BasicReturnEventArgs, Task> nacker = ea => {
        isReturned = true;
        return Task.CompletedTask;
      };

      publisher.AsMandatory(nacker)
        .WithRoutingKey("not-any-bound-queue")
        .Send(new TestMessage());

      await Task.Delay(500);
      Assert.True(isReturned);
      bunny.Dispose();
    }

    [Fact]
    public async Task MandatoryWorksWithQueue() {
      var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);

      var isReturned = true;
      var publisher = bunny.Publisher<TestMessage>("amq.direct");
      Func<BasicReturnEventArgs, Task> nacker = ea => {
        isReturned = false;
        return Task.CompletedTask;
      };

      publisher.AsMandatory(nacker)
        .WithQueueDeclare()
        .Send(new TestMessage { Text = "Mandatory-succeeds" });

      var removed = bunny.Setup().DeleteQueue(typeof(TestMessage).FullName, true);

      Assert.True(isReturned);
      bunny.Dispose();
    }

    [Fact]
    public async Task MultiplePublishOnSinglePublisher() {
      var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      var publisher = bunny.Publisher<TestMessage>("amq.direct");

      var queueName = "polymorph-queue";
      publisher.WithQueueDeclare(queueName, "poly")
        .Send(new OtherMessage());
      publisher.Send(new YetAnotherMessage());
      await Task.Delay(150);
      var count = bunny.Channel().MessageCount(queueName);

      Assert.Equal(2, (int)count);
      bunny.Setup().DeleteQueue(queueName, true);
    }

    [Fact]
    public async Task OverWriteRoutingKeySendsToDifferentQueuesEachTime() {
      var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      var publisher = bunny.Publisher<TestMessage>("amq.direct");

      var queueName = "polymorph-queue-other";
      var queueNameYetOther = "polymorph-queue-yet-another";
      publisher.WithQueueDeclare(queueName, "poly")
        .Send(new OtherMessage());
      publisher.WithQueueDeclare(queueNameYetOther, "poly-2")
        .WithRoutingKey("poly-2")
        .Send(new YetAnotherMessage());

      var otherCount = bunny.Channel().MessageCount(queueName);
      var yetOtherCount = bunny.Channel().MessageCount(queueNameYetOther);

      Assert.Equal(1, (int)otherCount);
      Assert.Equal(1, (int)yetOtherCount);

      bunny.Setup().DeleteQueue(queueName, true);
      bunny.Setup().DeleteQueue(queueNameYetOther, true);
    }

    public class TestMessage {
      public string Text { get; set; } = "Test";
      public int Number { get; set; } = 42;
    }

    public class OtherMessage : TestMessage { }

    public class YetAnotherMessage : OtherMessage { }
  }
}