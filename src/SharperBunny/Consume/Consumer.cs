namespace SharperBunny.Consume {
  using System;
  using RabbitMQ.Client.Events;
  using SharperBunny.Interfaces;

  public class Consumer<TMsg> : ConsumerBase<TMsg>, IConsumer<TMsg> {
    private EventingBasicConsumer consumer;
    private Action<ICarrot<TMsg>> receive;
    private Action<ICarrot<TMsg>> ackBehaviour;
    private Action<ICarrot<TMsg>> nackBehaviour;
    private Func<ReadOnlyMemory<byte>, TMsg> deserialize;

    public Consumer(IBunny bunny, string fromQueue) : base(bunny, fromQueue) {
      this.receive = async carrot => await carrot.SendAck();
      this.ackBehaviour = async carrot => await carrot.SendAck();
      this.nackBehaviour = async carrot => await carrot.SendNack(withRequeue: true);
    }

    public IConsumer<TMsg> Callback(Action<ICarrot<TMsg>> callback) {
      this.receive = callback;
      return this;
    }

    public OperationResult<TMsg> Get(Action<ICarrot<TMsg>> handle) {
      var operationResult = new OperationResult<TMsg>();

      try {
        var result = this.thisChannel.Channel.BasicGet(this.consumeFromQueue, this.autoAck);
        if (result != null) {
          var msg = this.deserialize(result.Body);
          var carrot = new Carrot<TMsg>(msg, result.DeliveryTag, this.thisChannel);
          handle(carrot);
          operationResult.IsSuccess = true;
          operationResult.State = OperationState.Get;
          operationResult.Message = msg;
        } else {
          operationResult.IsSuccess = false;
          operationResult.State = OperationState.GetFailed;
        }
      } catch (Exception ex) {
        operationResult.IsSuccess = false;
        operationResult.Error = ex;
      }

      return operationResult;
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
          return result;
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

    private void HandleReceived(object channel, BasicDeliverEventArgs args) {
      Carrot<TMsg> carrot = null;
      try {
        var message = this.deserialize(args.Body);
        carrot = new Carrot<TMsg>(message, args.DeliveryTag, this.thisChannel) { MessageProperties = args.BasicProperties };

        this.receive(carrot);
        if (this.autoAck == false) {
          this.ackBehaviour(carrot);
        }
      } catch (Exception ex) {
        if (carrot != null) {
          this.nackBehaviour(carrot);
        }
      }
    }

    public void Cancel() {
      this.Dispose(true);
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