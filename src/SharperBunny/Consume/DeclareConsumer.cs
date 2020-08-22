namespace SharperBunny.Consume {
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using RabbitMQ.Client.Events;
  using SharperBunny.Configuration;
  using SharperBunny.Connect;
  using SharperBunny.Interfaces;

  public class DeclareConsumer<TMsg> : IConsume<TMsg> {
    private readonly Dictionary<string, object> _arguments = new Dictionary<string, object>();
    private readonly IBunny _bunny;
    private readonly PermanentChannel _thisChannel;

    public DeclareConsumer(IBunny bunny, string fromQueue) {
      this._bunny = bunny;
      this._deserialize = Config.Deserialize<TMsg>;
      this._consumeFromQueue = fromQueue;
      this._thisChannel = new PermanentChannel(bunny);
      this._receive = async carrot => await carrot.SendAckAsync();
      this._ackBehaviour = async carrot => await carrot.SendAckAsync();
      this._nackBehaviour = async carrot => await carrot.SendNackAsync(withRequeue: true);
    }

    public IConsume<TMsg> AsAutoAck(bool autoAck = true) {
      this._autoAck = autoAck;
      return this;
    }

    public IConsume<TMsg> AckBehaviour(Func<ICarrot<TMsg>, Task> ackBehaviour) {
      this._autoAck = false;
      this._ackBehaviour = ackBehaviour;
      return this;
    }

    public IConsume<TMsg> NackBehaviour(Func<ICarrot<TMsg>, Task> nackBehaviour) {
      this._nackBehaviour = nackBehaviour;
      return this;
    }

    public IConsume<TMsg> Callback(Func<ICarrot<TMsg>, Task> callback) {
      this._receive = callback;
      return this;
    }

    public async Task<OperationResult<TMsg>> GetAsync(Func<ICarrot<TMsg>, Task> handle) {
      var operationResult = new OperationResult<TMsg>();

      try {
        await Task.Run(async () => {
          var result = this._thisChannel.Channel.BasicGet(this._consumeFromQueue, this._autoAck);
          if (result != null) {
            var msg = this._deserialize(result.Body);
            var carrot = new Carrot<TMsg>(msg, result.DeliveryTag, this._thisChannel);
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
      this._prefetchCount = prefetchCount;
      return this;
    }

    public async Task<OperationResult<TMsg>> StartConsumingAsync(IQueue force = null) {
      var result = new OperationResult<TMsg>();
      if (this._consumer == null) {
        try {
          var channel = this._thisChannel.Channel;
          if (force != null) {
            await force.DeclareAsync();
            this._consumeFromQueue = force.Name;
          }

          var prefetchSize = 0; // means --> no specific limit
          var applyToConnection = false;
          channel.BasicQos((uint)prefetchSize, (ushort)this._prefetchCount, applyToConnection);

          this._consumer = new EventingBasicConsumer(channel);
          var consumerTag = Guid.NewGuid().ToString();
          this._consumer.Received += this.HandleReceived;

          channel.BasicConsume(this._consumeFromQueue, this._autoAck,
                               consumerTag,
                               false,
                               false,
                               this._arguments,
                               this._consumer);

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
      this._useUniqueChannel = useUnique;
      return this;
    }

    public IConsume<TMsg> AddTag(string tag, object value) {
      if (this._arguments.ContainsKey(tag)) {
        this._arguments[tag] = value;
      } else {
        this._arguments.Add(tag, value);
      }

      return this;
    }

    private async void HandleReceived(object channel, BasicDeliverEventArgs deliverd) {
      Carrot<TMsg> carrot = null;
      try {
        var message = this._deserialize(deliverd.Body);
        carrot = new Carrot<TMsg>(message, deliverd.DeliveryTag, this._thisChannel) { MessageProperties = deliverd.BasicProperties };

        await this._receive(carrot);
        if (this._autoAck == false) {
          await this._ackBehaviour(carrot);
        }
      } catch (Exception ex) {
        if (carrot != null) {
          await this._nackBehaviour(carrot);
        }
      }
    }

    #region mutable fields

    private string _consumeFromQueue;
    private EventingBasicConsumer _consumer;
    private bool _useUniqueChannel;
    private Func<ICarrot<TMsg>, Task> _receive;
    private Func<ICarrot<TMsg>, Task> _ackBehaviour;
    private Func<ICarrot<TMsg>, Task> _nackBehaviour;
    private Func<ReadOnlyMemory<byte>, TMsg> _deserialize;
    private bool _autoAck;

    private uint _prefetchCount = 50;

    #endregion

    #region IDisposable Support

    private bool disposedValue;

    protected virtual void Dispose(bool disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          if (this._consumer != null) {
            foreach (var consumerTag in this._consumer.ConsumerTags) {
              this._thisChannel.Channel.BasicCancel(consumerTag);
            }

            this._consumer.Received -= this.HandleReceived;
          }

          this._thisChannel.Dispose();
        }

        this.disposedValue = true;
      }
    }

    public void Dispose() {
      this.Dispose(true);
    }

    public IConsume<TMsg> DeserializeMessage(Func<ReadOnlyMemory<byte>, TMsg> deserialize) {
      this._deserialize = deserialize;
      return this;
    }

    public Task CancelAsync() {
      this.Dispose(true);
      return Task.CompletedTask;
    }

    #endregion
  }
}