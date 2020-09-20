namespace SharperBunny.Consume {
  using System;
  using RabbitMQ.Client.Events;
  using SharperBunny.Interfaces;

  public class Consumer<TMsg> : ConsumerBase<TMsg>, IConsumer<TMsg> {
    private Action<ICarrot<TMsg>> ackBehaviour;
    private EventingBasicConsumer consumer;
    private Func<ReadOnlyMemory<byte>, TMsg> deserialize;
    private Action<ICarrot<TMsg>> nackBehaviour;
    private Action<ICarrot<TMsg>> receive;

    public Consumer(IBunny bunny, string fromQueue) : base(bunny, fromQueue) {
      this.deserialize = this.InternalDeserialize;
      this.receive = carrot => carrot.SendAck();
      this.ackBehaviour = carrot => carrot.SendAck();
      this.nackBehaviour = carrot => carrot.SendNack(withRequeue: true);
    }

    public IConsumer<TMsg> Callback(Action<ICarrot<TMsg>> callback) {
      this.receive = callback;
      return this;
    }

    public IConsumer<TMsg> AckBehaviour(Action<ICarrot<TMsg>> ackBehaviour) {
      this.autoAck = false;
      this.ackBehaviour = ackBehaviour;
      return this;
    }

    public IConsumer<TMsg> NackBehaviour(Action<ICarrot<TMsg>> nackBehaviour) {
      this.nackBehaviour = nackBehaviour;
      return this;
    }

    public OperationResult<TMsg> StartConsuming(IQueue force = null) {
      var result = new OperationResult<TMsg>();
      if (this.consumer == null) {
        try {
          var channel = this.thisChannel.Channel;
          if (force != null) {
            force.Declare();
            this.consumeFromQueue = force.Name;
          }

          const int prefetchSize = 0; // means --> no specific limit
          const bool applyToConnection = false;
          channel.BasicQos(prefetchSize, this.prefetchCount, applyToConnection);

          this.consumer = new EventingBasicConsumer(channel);
          var consumerTag = Guid.NewGuid().ToString();
          this.consumer.Received += this.HandleReceived;

          channel.BasicConsume(this.consumeFromQueue, this.autoAck,
                               consumerTag,
                               false,
                               false,
                               this.arguments,
                               this.consumer);

          result.State = OperationState.ConsumerAttached;
          result.IsSuccess = true;
          result.Message = default;
        } catch (Exception ex) {
          result.IsSuccess = false;
          result.Error = ex;
          result.State = OperationState.Failed;
        }
      } else {
        result.IsSuccess = true;
        result.State = OperationState.ConsumerAttached;
      }

      return result;
    }

    public IConsumer<TMsg> DeserializeMessage(Func<ReadOnlyMemory<byte>, TMsg> deserialize) {
      this.deserialize = deserialize;
      return this;
    }

    public void Cancel() {
      this.Dispose(true);
    }

    private void HandleReceived(object channel, BasicDeliverEventArgs args) {
      Carrot<TMsg> carrot = null;
      try {
        var message = this.deserialize(args.Body);
        carrot = new Carrot<TMsg>(message, args.DeliveryTag, this.thisChannel) { MessageProperties = args.BasicProperties };

        this.receive(carrot);
        if (!this.autoAck) {
          this.ackBehaviour(carrot);
        }
      } catch (Exception ex) {
        if (carrot != null) {
          this.nackBehaviour(carrot);
        }
      }
    }

    protected override void Dispose(bool disposing) {
      if (this.disposedValue) {
        return;
      }

      if (disposing) {
        if (this.consumer != null) {
          foreach (var consumerTag in this.consumer.ConsumerTags) {
            this.thisChannel.Channel.BasicCancel(consumerTag);
          }

          this.consumer.Received -= this.HandleReceived;
        }
      }

      base.Dispose(disposing);
    }
  }
}