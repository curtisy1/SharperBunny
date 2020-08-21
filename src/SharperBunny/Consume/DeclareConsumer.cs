using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using SharperBunny.Connect;

namespace SharperBunny.Consume {
    public class DeclareConsumer<TMsg> : IConsume<TMsg> {
        private readonly IBunny _bunny;
        private readonly PermanentChannel _thisChannel;
        private readonly Dictionary<string, object> _arguments = new Dictionary<string, object> ();

        #region mutable fields
        private string _consumeFromQueue;
        private EventingBasicConsumer _consumer;
        private bool _useUniqueChannel;
        private Func<ICarrot<TMsg>, Task> _receive;
        private Func<ICarrot<TMsg>, Task> _ackBehaviour;
        private Func<ICarrot<TMsg>, Task> _nackBehaviour;
        private Func<ReadOnlyMemory<byte>, TMsg> _deserialize;
        private bool _autoAck = false;

        private uint _prefetchCount = 50;
        #endregion

        public DeclareConsumer (IBunny bunny, string fromQueue) {
            _bunny = bunny;
            _deserialize = Config.Deserialize<TMsg>;
            _consumeFromQueue = fromQueue;
            _thisChannel = new PermanentChannel (bunny);
            _receive = async carrot => await carrot.SendAckAsync ();
            _ackBehaviour = async carrot => await carrot.SendAckAsync ();
            _nackBehaviour = async carrot => await carrot.SendNackAsync (withRequeue: true);
        }

        public IConsume<TMsg> AsAutoAck (bool autoAck = true) {
            _autoAck = autoAck;
            return this;
        }

        public IConsume<TMsg> AckBehaviour (Func<ICarrot<TMsg>, Task> ackBehaviour) {
            _autoAck = false;
            _ackBehaviour = ackBehaviour;
            return this;
        }

        public IConsume<TMsg> NackBehaviour (Func<ICarrot<TMsg>, Task> nackBehaviour) {
            _nackBehaviour = nackBehaviour;
            return this;
        }

        public IConsume<TMsg> AddTag (string tag, object value) {
            if (_arguments.ContainsKey (tag)) {
                _arguments[tag] = value;
            } else {
                _arguments.Add (tag, value);
            }
            return this;
        }

        public IConsume<TMsg> Callback (Func<ICarrot<TMsg>, Task> callback) {
            _receive = callback;
            return this;
        }

        public async Task<OperationResult<TMsg>> GetAsync (Func<ICarrot<TMsg>, Task> handle) {
            var operationResult = new OperationResult<TMsg> ();

            try {
                await Task.Run (async () => {
                    var result = _thisChannel.Channel.BasicGet (_consumeFromQueue, _autoAck);
                    if (result != null) {
                        var msg = _deserialize (result.Body);
                        var carrot = new Carrot<TMsg> (msg, result.DeliveryTag, _thisChannel);
                        await handle (carrot);
                        operationResult.IsSuccess = true;
                        operationResult.State = OperationState.Get;
                        operationResult.Message = msg;
                    } else {
                        operationResult.IsSuccess = false;
                        operationResult.State = OperationState.GetFailed;
                    }
                });
            } catch (System.Exception ex) {
                operationResult.IsSuccess = false;
                operationResult.Error = ex;
            }
            return operationResult;
        }

        public IConsume<TMsg> Prefetch (uint prefetchCount = 50) {
            _prefetchCount = prefetchCount;
            return this;
        }

        public async Task<OperationResult<TMsg>> StartConsumingAsync (IQueue force = null) {
            var result = new OperationResult<TMsg> ();
            if (_consumer == null) {
                try {
                    var channel = _thisChannel.Channel;
                    if (force != null) {
                        await force.DeclareAsync ();
                        _consumeFromQueue = force.Name;
                    }

                    int prefetchSize = 0; // means --> no specific limit
                    bool applyToConnection = false;
                    channel.BasicQos ((uint) prefetchSize, (ushort) _prefetchCount, global : applyToConnection);

                    _consumer = new EventingBasicConsumer (channel);
                    var consumerTag = Guid.NewGuid ().ToString ();
                    _consumer.Received += HandleReceived;

                    channel.BasicConsume (_consumeFromQueue,
                        _autoAck,
                        consumerTag,
                        noLocal : false,
                        exclusive : false,
                        arguments : _arguments,
                        consumer : _consumer);

                    result.State = OperationState.ConsumerAttached;
                    result.IsSuccess = true;
                    result.Message = default (TMsg);
                    return result;
                } catch (System.Exception ex) {
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

        private async void HandleReceived (object channel, BasicDeliverEventArgs deliverd) {
            Carrot<TMsg> carrot = null;
            try {
                TMsg message = _deserialize (deliverd.Body);
                carrot = new Carrot<TMsg> (message, deliverd.DeliveryTag, _thisChannel) {
                    MessageProperties = deliverd.BasicProperties
                };

                await _receive (carrot);
                if (_autoAck == false)
                    await _ackBehaviour (carrot);
            } catch (System.Exception ex) {
                if (carrot != null) {
                    await _nackBehaviour (carrot);
                }
            }
        }

        public IConsume<TMsg> UseUniqueChannel (bool useUnique = true) {
            _useUniqueChannel = useUnique;
            return this;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose (bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    if (_consumer != null) {
                        foreach (var consumerTag in _consumer.ConsumerTags) {
                            _thisChannel.Channel.BasicCancel (consumerTag);
                        }
                        _consumer.Received -= HandleReceived;
                    }
                    _thisChannel.Dispose ();
                }
                disposedValue = true;
            }
        }
        public void Dispose () {
            Dispose (true);
        }

        public IConsume<TMsg> DeserializeMessage (Func<ReadOnlyMemory<byte>, TMsg> deserialize) {
            _deserialize = deserialize;
            return this;
        }

        public Task CancelAsync () {
            Dispose (true);
            return Task.CompletedTask;
        }
        #endregion
    }
}