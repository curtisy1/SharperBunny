namespace SharperBunny.Tests.IntegrationTests {
  using System;

  internal static class Program {
    private static void Main(string[] args) {
      var pipe = Bunny.ConnectSingleWith();
      var bunny = pipe.Connect();

      // TODO: The rest is pretty much an integration test.
      // Migrate once remaining methods are covered
      // Regions are the original test locations

      #region Consume/ConsumerTests
      // private const string get = "get-queue";
      // private const string queue = "consume-queue";
      // private const string nackQueue = "nack-no-requeue";
      // private const string nackReQueue = "nack-requeue";
      // [Fact]
      // public async Task ConsumerAttachReturnsOperationResult() {
      //   await this.ConsumeGeneric(async carrot => {
      //     var result = carrot.Message;
      //     var opResult = carrot.SendAck();
      //     Assert.True(opResult.IsSuccess);
      //
      //     Assert.NotNull(result);
      //     Assert.Equal(nameof(ConsumeMessage), result.MyText);
      //   });
      // }
      //
      // private async Task<IBunny> ConsumeGeneric(Action<ICarrot<ConsumeMessage>> carrot, string toQueue = queue) {
      //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      //
      //   var consumer = bunny.Consumer<ConsumeMessage>(toQueue).Callback(carrot);
      //   var operationResult = consumer.StartConsuming();
      //
      //   Assert.True(operationResult.IsSuccess);
      //   Assert.Equal(OperationState.ConsumerAttached, operationResult.State);
      //
      //   this.SetupAndPublish(bunny, toQueue);
      //   return bunny;
      // }
      //
      // private void SetupAndPublish(IBunny bunny, string queueName = queue) {
      //   var msg = new ConsumeMessage();
      //   var bytes = JsonSerializer.SerializeToUtf8Bytes(msg);
      //
      //   var channel = bunny.Channel(true);
      //   var prop = channel.CreateBasicProperties();
      //   prop.Persistent = true;
      //
      //   channel.BasicPublish("", queueName, false, prop, bytes);
      // }
      //
      // [Fact]
      // public async Task MultipleCallToConsumeAlwaysReturnSuccess() {
      //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      //
      //   ConsumeMessage result = null;
      //   var consumer = bunny.Consumer<ConsumeMessage>(queue).Callback(async carrot => {
      //     result = carrot.Message;
      //     var opResult = carrot.SendAck();
      //     Assert.True(opResult.IsSuccess);
      //     Assert.NotNull(result);
      //     Assert.Equal(nameof(ConsumeMessage), result.MyText);
      //   });
      //   var result1 = consumer.StartConsuming();
      //   var result2 = consumer.StartConsuming();
      //   var result3 = consumer.StartConsuming();
      //
      //   Assert.Equal(result1, result2, new EqualityOpResult());
      //   Assert.Equal(result1, result3, new EqualityOpResult());
      //   Assert.Equal(result2, result2, new EqualityOpResult());
      // }
      //
      // [Fact]
      // public async Task StartConsumingAsyncWithForceDeclaresTheQueue() {
      //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      //
      //   var before = bunny.Setup().QueueExists("force-declared");
      //   var queue = bunny.Setup().Queue("force-declared");
      //   var consumer = bunny.Consumer<ConsumeMessage>()
      //     .StartConsuming(queue);
      //
      //   var after = bunny.Setup().QueueExists("force-declared");
      //   Assert.False(before);
      //   Assert.True(after);
      //
      //   var deleted = bunny.Setup().DeleteQueue("force-declared", true);
      //   Assert.True(deleted);
      // }
      //
      // [Fact]
      // public void NackRequeues() {
      //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      //
      //   var consumer = bunny.Consumer<ConsumeMessage>(queue).Callback(async carrot => {
      //     var result = carrot.SendNack(withRequeue: true);
      //     Assert.Equal(OperationState.Nacked, result.State);
      //     var count = bunny.Channel().MessageCount(nackReQueue);
      //     Assert.Equal((uint)1, count);
      //   });
      //   this.SetupAndPublish(bunny, nackReQueue);
      // }
      //
      // [Fact]
      // public async Task NackNoRequeue() {
      //   var bunny = await this.ConsumeGeneric(async carrot => {
      //     var result = carrot.SendNack();
      //     Assert.Equal(OperationState.Nacked, result.State);
      //   }, nackQueue);
      //
      //   var count = bunny.Channel().MessageCount(nackQueue);
      //   Assert.Equal((uint)0, count);
      // }
      //
      // private class EqualityOpResult : IEqualityComparer<OperationResult<ConsumeMessage>> {
      //   public bool Equals(OperationResult<ConsumeMessage> x, OperationResult<ConsumeMessage> y) {
      //     return x.State == y.State && x.IsSuccess == y.IsSuccess;
      //   }
      //
      //   public int GetHashCode(OperationResult<ConsumeMessage> obj) {
      //     // does not matter
      //     return 42;
      //   }
      // }

      // public class ConsumeMessage {
      //   public string MyText { get; set; } = nameof(ConsumeMessage);
      // }

      // [Theory]
      //     [InlineData("")]
      //     [InlineData("   ")]
      //     [InlineData(null)]
      //     [InlineData("255")]
      //     public void ThrowsOnNameNullOrToLong(string name) {
      //       if (name == "255") {
      //         name = name.PadRight(500, '-');
      //       }
      //
      //       var @base = new DeclareBase();
      //       Assert.Throws<DeclarationException>(() => @base.Queue(name));
      //     }
      //
      //     [Theory]
      //     [InlineData("")]
      //     [InlineData("   ")]
      //     [InlineData(null)]
      //     [InlineData("255")]
      //     public void CheckBindingDeclare(string name) {
      //       if (name == "255") {
      //         name = name.PadRight(500, '-');
      //       }
      //
      //       var @base = new DeclareQueue(ConnectionClusterTests.Connect(), "queue");
      //       Assert.Throws<DeclarationException>(() => @base.Bind(name));
      //     }
      //
      //     [Fact]
      //     public void BindAsSetsBindingKeyOn() {
      //       var @base = new DeclareQueue(ConnectionClusterTests.Connect(), "queue");
      //       @base.Bind("ex", "bind-key");
      //       Assert.Equal("bind-key", @base.BindingKey.HasValue ? @base.BindingKey.Value.rKey : "null");
      //     }
      //
      //     [Fact]
      //     public async Task DeclareAndBindDefaultAmqDirectSucceeds() {
      //       var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      //       var declare = bunny.Setup()
      //         .Queue("bind-test")
      //         .Bind("amq.direct", "bind-test-key")
      //         .AsDurable()
      //         .QueueExpiry(1500)
      //         .WithTtl(500)
      //         .MaxLength(10);
      //
      //       declare.Declare();
      //
      //       Assert.Equal(1, 1);
      //     }
      
      #endregion
      
      #region Declaration/DeleteTests
      // [Fact]
      // public async Task DeclareAndDeleteQueueNotExistsAfterWards() {
      //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      //   var declare = bunny.Setup()
      //     .Queue("to-delete");
      //
      //   declare.Declare();
      //   var exists = declare.QueueExists(declare.Name);
      //   Assert.True(exists);
      //   declare.DeleteQueue(declare.Name);
      //   exists = declare.QueueExists(declare.Name);
      //   Assert.False(exists);
      // }
      //
      // [Fact]
      // public async Task DeclareAndDeleteExchangeNotExistsAfterWards() {
      //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
      //   var declare = bunny.Setup()
      //     .Exchange("to-delete-ex", "fanout");
      //
      //   declare.Declare();
      //   var exists = declare.ExchangeExists(declare.Name);
      //   Assert.True(exists);
      //   declare.DeleteExchange(declare.Name);
      //   exists = declare.ExchangeExists(declare.Name);
      //   Assert.False(exists);
      // }
      
      #endregion
      
      #region Producer/PublisherTests
      // [Fact]
    // public async Task PublisherSimplySendsWithoutQueueReturnsFailure() {
    //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
    //   var publisher = bunny.Publisher<TestMessage>(this.exchange);
    //
    //   var result = publisher.Send(new TestMessage());
    //
    //   Assert.True(result.IsSuccess);
    //   bunny.Dispose();
    // }
    //
    // [Fact]
    // public async Task PublisherSimplySendsWitQueueReturnsSuccess() {
    //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
    //   var publisher = bunny.Publisher<TestMessage>(this.exchange);
    //
    //   var result = publisher
    //     .WithQueueDeclare()
    //     .Send(new TestMessage());
    //
    //   var success = bunny.Setup().DeleteQueue(typeof(TestMessage).FullName, true);
    //
    //   Assert.True(result.IsSuccess);
    //   Assert.True(success);
    //   bunny.Dispose();
    // }
    //
    // [Fact]
    // public async Task ForceCreatesTheExchangeIfNotExists() {
    //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
    //   var publisher = bunny.Publisher<TestMessage>("test-exchange");
    //
    //   var result = publisher.Send(new TestMessage(), true);
    //
    //   Assert.True(result.IsSuccess);
    //   var removedExchange = bunny.Setup().DeleteExchange("test-exchange", true);
    //   Assert.True(removedExchange);
    //   bunny.Dispose();
    // }
    //
    // [Fact]
    // public async Task ConfirmsAndAcksWork() {
    //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
    //   var queue = bunny.Setup()
    //     .Queue("constraint")
    //     .MaxLength(1)
    //     .QueueExpiry(1500)
    //     .Bind("amq.direct", "constraint-key")
    //     .OverflowReject();
    //
    //   var isNacked = false;
    //   var isAcked = false;
    //   var publisher = bunny.Publisher<TestMessage>("amq.direct");
    //   Func<BasicNackEventArgs, Task> nacker = ea => {
    //     isNacked = true;
    //     return Task.CompletedTask;
    //   };
    //   Func<BasicAckEventArgs, Task> acker = ea => {
    //     isAcked = true;
    //     return Task.CompletedTask;
    //   };
    //
    //   publisher.WithQueueDeclare(queue)
    //     .WithConfirm(acker, nacker)
    //     .WithRoutingKey("constraint-key")
    //     .Send(new TestMessage { Text = "Confirm-1st" });
    //
    //   publisher.WithQueueDeclare(queue)
    //     .WithConfirm(acker, nacker)
    //     .Send(new TestMessage { Text = "Confirm-2nd" });
    //
    //   Assert.True(isAcked);
    //   Assert.True(isNacked);
    //   bunny.Dispose();
    // }
    //
    // [Fact]
    // public async Task MandatoryFailsWithoutQueue() {
    //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
    //
    //   var isReturned = false;
    //   var publisher = bunny.Publisher<TestMessage>("amq.direct");
    //   Func<BasicReturnEventArgs, Task> nacker = ea => {
    //     isReturned = true;
    //     return Task.CompletedTask;
    //   };
    //
    //   publisher.AsMandatory(nacker)
    //     .WithRoutingKey("not-any-bound-queue")
    //     .Send(new TestMessage());
    //
    //   await Task.Delay(500);
    //   Assert.True(isReturned);
    //   bunny.Dispose();
    // }
    //
    // [Fact]
    // public async Task MandatoryWorksWithQueue() {
    //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
    //
    //   var isReturned = true;
    //   var publisher = bunny.Publisher<TestMessage>("amq.direct");
    //   Func<BasicReturnEventArgs, Task> nacker = ea => {
    //     isReturned = false;
    //     return Task.CompletedTask;
    //   };
    //
    //   publisher.AsMandatory(nacker)
    //     .WithQueueDeclare()
    //     .Send(new TestMessage { Text = "Mandatory-succeeds" });
    //
    //   var removed = bunny.Setup().DeleteQueue(typeof(TestMessage).FullName, true);
    //
    //   Assert.True(isReturned);
    //   bunny.Dispose();
    // }
    //
    // [Fact]
    // public async Task MultiplePublishOnSinglePublisher() {
    //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
    //   var publisher = bunny.Publisher<TestMessage>("amq.direct");
    //
    //   var queueName = "polymorph-queue";
    //   publisher.WithQueueDeclare(queueName, "poly")
    //     .Send(new OtherMessage());
    //   publisher.Send(new YetAnotherMessage());
    //   await Task.Delay(150);
    //   var count = bunny.Channel().MessageCount(queueName);
    //
    //   Assert.Equal(2, (int)count);
    //   bunny.Setup().DeleteQueue(queueName, true);
    // }
    //
    // [Fact]
    // public async Task OverWriteRoutingKeySendsToDifferentQueuesEachTime() {
    //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
    //   var publisher = bunny.Publisher<TestMessage>("amq.direct");
    //
    //   var queueName = "polymorph-queue-other";
    //   var queueNameYetOther = "polymorph-queue-yet-another";
    //   publisher.WithQueueDeclare(queueName, "poly")
    //     .Send(new OtherMessage());
    //   publisher.WithQueueDeclare(queueNameYetOther, "poly-2")
    //     .WithRoutingKey("poly-2")
    //     .Send(new YetAnotherMessage());
    //
    //   var otherCount = bunny.Channel().MessageCount(queueName);
    //   var yetOtherCount = bunny.Channel().MessageCount(queueNameYetOther);
    //
    //   Assert.Equal(1, (int)otherCount);
    //   Assert.Equal(1, (int)yetOtherCount);
    //
    //   bunny.Setup().DeleteQueue(queueName, true);
    //   bunny.Setup().DeleteQueue(queueNameYetOther, true);
    // }
    
    // private readonly string exchange = ""; // default --> " "
    //
    //
    //
    // public class TestMessage {
    //   public string Text { get; set; } = "Test";
    //   public int Number { get; set; } = 42;
    // }
    //
    // public class OtherMessage : TestMessage { }
    //
    // public class YetAnotherMessage : OtherMessage { }
    
    #endregion
    
    #region Rpc/RequestTests
    // [Fact]
    // public async Task DirectReplyWorks() {
    //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
    //   var rpcExchange = "rpc-exchange";
    //
    //   bunny.Respond<MyRequest, MyResponse>(rpcExchange, rq => new MyResponse())
    //     .StartResponding();
    //
    //   var result = bunny.Request<MyRequest, MyResponse>(rpcExchange)
    //     .Request(new MyRequest(), true);
    //
    //   await Task.Delay(500);
    //
    //   Assert.True(result.IsSuccess);
    //   Assert.NotNull(result.Message);
    // }
    //
    // [Fact]
    // public async Task WithTemporaryQueueWorksAlso() {
    //   var bunny = Bunny.ConnectSingle(ConnectionClusterTests.BasicAmqp);
    //   var rpcExchange = "rpc-exchange";
    //
    //   bunny.Respond<MyRequest, MyResponse>(rpcExchange, rq => new MyResponse())
    //     .StartResponding();
    //
    //   var result = bunny.Request<MyRequest, MyResponse>(rpcExchange)
    //     .Request(new MyRequest(), true);
    //
    //   await Task.Delay(500);
    //
    //   Assert.True(result.IsSuccess);
    //   Assert.NotNull(result.Message);
    // }
    
    // private class MyRequest { }
    //
    // private class MyResponse { }
    
    #endregion

      Console.ReadLine();
    }
  }
}