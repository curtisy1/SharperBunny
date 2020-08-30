namespace SharperBunny.Tests.Consume {
  using System.Threading.Tasks;
  using FluentAssertions;
  using NSubstitute;
  using SharperBunny.Consume;
  using SharperBunny.Interfaces;
  using Xunit;

  public class ConsumerTests {
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
  }
}