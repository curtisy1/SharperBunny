namespace SharperBunny.Consume {
  using System;
  using System.Threading.Tasks;
  using RabbitMQ.Client.Events;
  using SharperBunny.Interfaces;

  public class AsyncConsumer<TMsg> : ConsumerBase, IAsyncConsumer<TMsg> {
    private AsyncEventingBasicConsumer consumer;
    private Func<IAsyncCarrot<TMsg>, Task> receive;
    private Func<IAsyncCarrot<TMsg>, Task> ackBehaviour;
    private Func<IAsyncCarrot<TMsg>, Task> nackBehaviour;
    private Func<ReadOnlyMemory<byte>, TMsg> deserialize;

    public AsyncConsumer(IBunny bunny, string fromQueue) : base(bunny, fromQueue) {
      this.receive = async carrot => await carrot.SendAck();
      this.ackBehaviour = async carrot => await carrot.SendAck();
      this.nackBehaviour = async carrot => await carrot.SendNack(withRequeue: true);
    }

    public Task<OperationResult<TMsg>> StartConsuming(IQueue force = null) {
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

          this.consumer = new AsyncEventingBasicConsumer(channel);
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

      return Task.FromResult(result);
    }

    public async Task<OperationResult<TMsg>> Get(Func<IAsyncCarrot<TMsg>, Task> handle) {
      var operationResult = new OperationResult<TMsg>();

      try {
        var result = this.thisChannel.Channel.BasicGet(this.consumeFromQueue, this.autoAck);
        if (result != null) {
          var msg = this.deserialize(result.Body);
          var carrot = new AsyncCarrot<TMsg>(msg, result.DeliveryTag, this.thisChannel);
          await handle(carrot);
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

    public IAsyncConsumer<TMsg> Callback(Func<IAsyncCarrot<TMsg>, Task> callback) {
      this.receive = callback;
      return this;
    }

    public IAsyncConsumer<TMsg> AckBehaviour(Func<IAsyncCarrot<TMsg>, Task> ackBehaviour) {
      this.autoAck = false;
      this.ackBehaviour = ackBehaviour;
      return this;
    }

    public IAsyncConsumer<TMsg> NackBehaviour(Func<IAsyncCarrot<TMsg>, Task> nackBehaviour) {
      this.nackBehaviour = nackBehaviour;
      return this;
    }
    
    public IAsyncConsumer<TMsg> DeserializeMessage(Func<ReadOnlyMemory<byte>, TMsg> deserialize) {
      this.deserialize = deserialize;
      return this;
    }

    public Task Cancel() {
      this.Dispose(true);
      return Task.CompletedTask;
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

    private async Task HandleReceived(object channel, BasicDeliverEventArgs args) {
      AsyncCarrot<TMsg> carrot = null;
      try {
        var message = this.deserialize(args.Body);
        carrot = new AsyncCarrot<TMsg>(message, args.DeliveryTag, this.thisChannel) { MessageProperties = args.BasicProperties };

        await this.receive(carrot);
        if (this.autoAck == false) {
          await this.ackBehaviour(carrot);
        }
      } catch (Exception ex) {
        if (carrot != null) {
          await this.nackBehaviour(carrot);
        }
      }
    }
  }
}