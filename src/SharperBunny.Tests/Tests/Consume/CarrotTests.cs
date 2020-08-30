namespace SharperBunny.Tests.Consume {
  using System;
  using System.Threading.Tasks;
  using FluentAssertions;
  using NSubstitute;
  using SharperBunny.Consume;
  using SharperBunny.Interfaces;
  using Xunit;

  public class CarrotTests {
    [Fact]
    public async Task AsyncCarrot_SendNack_ReturnsSuccess() {
      var permanentChannelSubstitute = Substitute.For<IPermanentChannel>();
      var asyncCarrot = new AsyncCarrot<object>(new object(), 1, permanentChannelSubstitute);

      var result = await asyncCarrot.SendNack();

      result.IsSuccess.Should().BeTrue();
      result.State.Should().HaveFlag(OperationState.Nacked);
      result.Error.Should().BeNull();
    }

    [Fact]
    public async Task AsyncCarrot_SendNack_ReturnsFailed_OnThrow() {
      var permanentChannelSubstitute = Substitute.For<IPermanentChannel>();
      permanentChannelSubstitute.Channel.WhenForAnyArgs(c => c.BasicNack(default, default, default))
        .Throw(new Exception("Test success"));
      var asyncCarrot = new AsyncCarrot<object>(new object(), 1, permanentChannelSubstitute);

      var result = await asyncCarrot.SendNack();

      result.IsSuccess.Should().BeFalse();
      result.State.Should().HaveFlag(OperationState.Failed);
      result.Error.Message.Should().NotBeNull().And.Be("Test success");
    }
    
    [Fact]
    public async Task AsyncCarrot_SendAck_ReturnsSuccess() {
      var permanentChannelSubstitute = Substitute.For<IPermanentChannel>();
      var asyncCarrot = new AsyncCarrot<object>(new object(), 1, permanentChannelSubstitute);

      var result = await asyncCarrot.SendAck();

      result.IsSuccess.Should().BeTrue();
      result.State.Should().HaveFlag(OperationState.Acked);
      result.Error.Should().BeNull();
    }

    [Fact]
    public async Task AsyncCarrot_SendAck_ReturnsFailed_OnThrow() {
      var permanentChannelSubstitute = Substitute.For<IPermanentChannel>();
      permanentChannelSubstitute.Channel.WhenForAnyArgs(c => c.BasicAck(default, default))
        .Throw(new Exception("Test success"));
      var asyncCarrot = new AsyncCarrot<object>(new object(), 1, permanentChannelSubstitute);

      var result = await asyncCarrot.SendAck();

      result.IsSuccess.Should().BeFalse();
      result.State.Should().HaveFlag(OperationState.Failed);
      result.Error.Message.Should().NotBeNull().And.Be("Test success");
    }
    
    [Fact]
    public void Carrot_SendNack_ReturnsSuccess() {
      var permanentChannelSubstitute = Substitute.For<IPermanentChannel>();
      var carrot = new Carrot<object>(new object(), 1, permanentChannelSubstitute);

      var result = carrot.SendNack();

      result.IsSuccess.Should().BeTrue();
      result.State.Should().HaveFlag(OperationState.Nacked);
      result.Error.Should().BeNull();
    }

    [Fact]
    public void Carrot_SendNack_ReturnsFailed_OnThrow() {
      var permanentChannelSubstitute = Substitute.For<IPermanentChannel>();
      permanentChannelSubstitute.Channel.WhenForAnyArgs(c => c.BasicNack(default, default, default))
        .Throw(new Exception("Test success"));
      var carrot = new Carrot<object>(new object(), 1, permanentChannelSubstitute);

      var result = carrot.SendNack();

      result.IsSuccess.Should().BeFalse();
      result.State.Should().HaveFlag(OperationState.Failed);
      result.Error.Message.Should().NotBeNull().And.Be("Test success");
    }
    
    
    
    [Fact]
    public void Carrot_SendAck_ReturnsSuccess() {
      var permanentChannelSubstitute = Substitute.For<IPermanentChannel>();
      var carrot = new Carrot<object>(new object(), 1, permanentChannelSubstitute);

      var result = carrot.SendAck();

      result.IsSuccess.Should().BeTrue();
      result.State.Should().HaveFlag(OperationState.Acked);
      result.Error.Should().BeNull();
    }

    [Fact]
    public void Carrot_SendAck_ReturnsFailed_OnThrow() {
      var permanentChannelSubstitute = Substitute.For<IPermanentChannel>();
      permanentChannelSubstitute.Channel.WhenForAnyArgs(c => c.BasicAck(default, default))
        .Throw(new Exception("Test success"));
      var carrot = new Carrot<object>(new object(), 1, permanentChannelSubstitute);

      var result = carrot.SendAck();

      result.IsSuccess.Should().BeFalse();
      result.State.Should().HaveFlag(OperationState.Failed);
      result.Error.Message.Should().NotBeNull().And.Be("Test success");
    }
  }
}