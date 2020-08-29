namespace SharperBunny.Tests.Consume {
  using System;
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Threading.Tasks;
  using FluentAssertions;
  using NSubstitute;
  using NSubstitute.ReceivedExtensions;
  using SharperBunny.Configuration;
  using SharperBunny.Consume;
  using SharperBunny.Extensions;
  using SharperBunny.Interfaces;
  using SharperBunny.Tests.Connection;
  using Xunit;

  public class ConsumerTests {
    private const string get = "get-queue";
    private const string queue = "consume-queue";
    private const string nackQueue = "nack-no-requeue";
    private const string nackReQueue = "nack-requeue";

    [Fact]
    public void Consumer_StartConsuming_WithDefaults_TakesDefaultCallbacks() {
      var bunny = Substitute.For<IBunny>();
      var consumer = new Consumer<object>(bunny, string.Empty);

      var result = consumer.StartConsuming();

      result.State.Should().HaveFlag(OperationState.ConsumerAttached);
      bunny.ReceivedWithAnyArgs(1).Channel();
      bunny.ReceivedWithAnyArgs(1).Channel().BasicQos(default, default, default);
      bunny.ReceivedWithAnyArgs(1).Channel().BasicConsume(default, default, default, default, default, default, default);
    }
    
    [Fact]
    public async Task AsyncConsumer_StartConsuming_WithDefaults_TakesDefaultCallbacks() {
      var bunny = Substitute.For<IBunny>();
      var consumer = new AsyncConsumer<object>(bunny, string.Empty);

      var result = await consumer.StartConsuming();

      result.State.Should().HaveFlag(OperationState.ConsumerAttached);
      bunny.ReceivedWithAnyArgs(1).Channel();
      bunny.ReceivedWithAnyArgs(1).Channel().BasicQos(default, default, default);
      bunny.ReceivedWithAnyArgs(1).Channel().BasicConsume(default, default, default, default, default, default, default);
    }
    
    // TODO: The rest is pretty much an integration test.
    // Migrate once remaining methods are covered

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

    public class ConsumeMessage {
      public string MyText { get; set; } = nameof(ConsumeMessage);
    }
  }
}