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
  }
}