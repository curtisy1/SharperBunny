namespace SharperBunny.Tests.Declaration {
  using NSubstitute;
  using NSubstitute.ExceptionExtensions;
  using RabbitMQ.Client;
  using RabbitMQ.Client.Exceptions;
  using SharperBunny.Declare;
  using SharperBunny.Interfaces;
  using Xunit;

  public class DeclareQueueTests {
    [Fact]
    public void Declare_WhenNoBindingSpecified_DoesNotBindQueueToExchange() {
      var bunny = Substitute.For<IBunny>();
      var model = Substitute.For<IModel>();
      model.QueueDeclarePassive(default).ThrowsForAnyArgs(new OperationInterruptedException(default));
      bunny.Channel().ReturnsForAnyArgs(model);
      var queue = new DeclareQueue(bunny, string.Empty);
      
      queue.Declare();

      model.ReceivedWithAnyArgs(1).QueueDeclare();
      model.DidNotReceiveWithAnyArgs().QueueBind(default, default, default,default);
    }

    [Fact]
    public void Declare_WhenBindingSpecified_BindsQueueToExchange() {
      var bunny = Substitute.For<IBunny>();
      var model = Substitute.For<IModel>();
      model.QueueDeclarePassive(default).ThrowsForAnyArgs(new OperationInterruptedException(default));
      bunny.Channel().ReturnsForAnyArgs(model);
      var queue = new DeclareQueue(bunny, string.Empty);

      queue.Bind(string.Empty);
      queue.Declare();

      model.ReceivedWithAnyArgs(1).QueueDeclare();
      model.ReceivedWithAnyArgs(1).QueueBind(default, default, default,default);
    }
    
    [Fact]
    public void Declare_OnlyDeclaresQueueOnce() {
      var bunny = Substitute.For<IBunny>();
      var model = Substitute.For<IModel>();
      model.QueueDeclarePassive(default).ThrowsForAnyArgs(new OperationInterruptedException(default));
      bunny.Channel().ReturnsForAnyArgs(model);
      var queue = new DeclareQueue(bunny, string.Empty);

      queue.Declare();
      queue.Declare();

      model.ReceivedWithAnyArgs(1).QueueDeclare();
    }
  }
}