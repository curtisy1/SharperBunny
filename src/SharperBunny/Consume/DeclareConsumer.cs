namespace SharperBunny.Consume {
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using RabbitMQ.Client.Events;
  using SharperBunny.Configuration;
  using SharperBunny.Connection;
  using SharperBunny.Interfaces;

  public class DeclareConsumer<TMsg> : IConsume<TMsg> {
    private readonly Dictionary<string, object> arguments = new Dictionary<string, object>();
    private readonly IBunny bunny;
    private readonly PermanentChannel thisChannel;

    public DeclareConsumer(IBunny bunny, string fromQueue) {
      this.bunny = bunny;
      this.deserialize = Config.Deserialize<TMsg>;
      this.consumeFromQueue = fromQueue;
      this.thisChannel = new PermanentChannel(bunny);
      this.receive = async carrot => await carrot.SendAckAsync();
      this.ackBehaviour = async carrot => await carrot.SendAckAsync();
      this.nackBehaviour = async carrot => await carrot.SendNackAsync(withRequeue: true);
    }

    public IConsume<TMsg> AsAutoAck(bool autoAck = true) {
      this.autoAck = autoAck;
      return this;
    }

    public IConsume<TMsg> AckBehaviour(Func<ICarrot<TMsg>, Task> ackBehaviour) {
      this.autoAck = false;
      this.ackBehaviour = ackBehaviour;
      return this;
    }

    public IConsume<TMsg> NackBehaviour(Func<ICarrot<TMsg>, Task> nackBehaviour) {
      this.nackBehaviour = nackBehaviour;
      return this;
    }

    public IConsume<TMsg> Callback(Func<ICarrot<TMsg>, Task> callback) {
      this.receive = callback;
      return this;
    }

    public async Task<OperationResult<TMsg>> GetAsync(Func<ICarrot<TMsg>, Task> handle) {
      var operationResult = new OperationResult<TMsg>();

      try {
        await Task.Run(async () => {
          var result = this.thisChannel.Channel.BasicGet(this.consumeFromQueue, this.autoAck);
          if (result != null) {
            var msg = this.deserialize(result.Body);
            var carrot = new Carrot<TMsg>(msg, result.DeliveryTag, this.thisChannel);
            await handle(carrot);
            operationResult.IsSuccess = true;
            operationResult.State = OperationState.Get;
            operationResult.Message = msg;
          } else {
            operationResult.IsSuccess = false;
            operationResult.State = OperationState.GetFailed;
          }
        });
      } catch (Exception ex) {
        operationResult.IsSuccess = false;
        operationResult.Error = ex;
      }

      return operationResult;
    }

    public IConsume<TMsg> Prefetch(uint prefetchCount = 50) {
      this.prefetchCount = prefetchCount;
      return this;
    }

    public async Task<OperationResult<TMsg>> StartConsumingAsync(IQueue force = null) {
      var result = new OperationResult<TMsg>();
      if (this.consumer == null) {
        try {
          var channel = this.thisChannel.Channel;
          if (force != null) {
            await force.DeclareAsync();
            this.consumeFromQueue = force.Name;
          }

          var prefetchSize = 0; // means --> no specific limit
          var applyToConnection = false;
          channel.BasicQos((uint)prefetchSize, (ushort)this.prefetchCount, applyToConnection);

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

    public IConsume<TMsg> UseUniqueChannel(bool useUnique = true) {
      this.useUniqueChannel = useUnique;
      return this;
    }

    public IConsume<TMsg> AddTag(string tag, object value) {
      if (this.arguments.ContainsKey(tag)) {
        this.arguments[tag] = value;
      } else {
        this.arguments.Add(tag, value);
      }

      return this;
    }

    private async void HandleReceived(object channel, BasicDeliverEventArgs deliverd) {
      Carrot<TMsg> carrot = null;
      try {
        var message = this.deserialize(deliverd.Body);
        carrot = new Carrot<TMsg>(message, deliverd.DeliveryTag, this.thisChannel) { MessageProperties = deliverd.BasicProperties };

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

    #region mutable fields

    private string consumeFromQueue;
    private EventingBasicConsumer consumer;
    private bool useUniqueChannel;
    private Func<ICarrot<TMsg>, Task> receive;
    private Func<ICarrot<TMsg>, Task> ackBehaviour;
    private Func<ICarrot<TMsg>, Task> nackBehaviour;
    private Func<ReadOnlyMemory<byte>, TMsg> deserialize;
    private bool autoAck;

    private uint prefetchCount = 50;

    #endregion

    #region IDisposable Support

    private bool disposedValue;

    protected virtual void Dispose(bool disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          if (this.consumer != null) {
            foreach (var consumerTag in this.consumer.ConsumerTags) {
              this.thisChannel.Channel.BasicCancel(consumerTag);
            }

            this.consumer.Received -= this.HandleReceived;
          }

          this.thisChannel.Dispose();
        }

        this.disposedValue = true;
      }
    }

    public void Dispose() {
      this.Dispose(true);
    }

    public IConsume<TMsg> DeserializeMessage(Func<ReadOnlyMemory<byte>, TMsg> deserialize) {
      this.deserialize = deserialize;
      return this;
    }

    public Task CancelAsync() {
      this.Dispose(true);
      return Task.CompletedTask;
    }

    #endregion
  }
}