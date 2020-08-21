using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharperBunny.Connect;
using SharperBunny.Declare;
using SharperBunny.Exceptions;

namespace SharperBunny.Publish {
    public class DeclarePublisher<T> : IPublish<T>
        where T : class {
            #region immutable fields
            private readonly IBunny _bunny;
            private readonly PermanentChannel _thisChannel;
            private readonly string _publishTo;
            #endregion

            #region mutable fields
            private Func<T, byte[]> _serialize;
            private bool Mandatory { get; set; }
            private bool ConfirmActivated { get; set; }
            private bool Persistent { get; set; }
            private int? Expires { get; set; }
            private string RoutingKey {
                get {
                    if (_routingKey != null) {
                        return _routingKey;
                    }
                    if (_queueDeclare != null) {
                        return _queueDeclare.RoutingKey;
                    }

                    return typeof (T).FullName;
                }
            }
            private string _routingKey;
            private bool _uniqueChannel;
            private IQueue _queueDeclare;
            private bool _useConfirm;
            private Func<BasicReturnEventArgs, Task> _returnCallback = context => Task.CompletedTask;
            private Func<BasicAckEventArgs, Task> _ackCallback = context => Task.CompletedTask;
            private Func<BasicNackEventArgs, Task> _nackCallback = context => Task.CompletedTask;
            #endregion
            internal DeclarePublisher (IBunny bunny, string publishTo) {
                _bunny = bunny;
                _publishTo = publishTo;
                _serialize = Config.Serialize;
                _thisChannel = new PermanentChannel (bunny);
            }

            #region Send
            public virtual async Task<OperationResult<T>> SendAsync (T msg, bool force = false) {
                var operationResult = new OperationResult<T> ();
                operationResult.Message = msg;
                IModel channel = null;
                try {
                    channel = _thisChannel.Channel;

                    var properties = ConstructProperties (channel.CreateBasicProperties (), Persistent, this.Expires);
                    Handlers (channel);

                    if (_queueDeclare != null) {
                        await _queueDeclare.DeclareAsync ();
                    }
                    if (force) {
                        await _bunny.Setup ()
                            .Exchange (_publishTo)
                            .AsDurable ()
                            .DeclareAsync ();
                    }

                    await Task.Run (() => {
                        if (_useConfirm) {
                            channel.ConfirmSelect ();
                        }
                        channel.BasicPublish (_publishTo, RoutingKey, mandatory : Mandatory, properties, _serialize (msg));
                        if (_useConfirm) {
                            channel.WaitForConfirmsOrDie ();
                        }
                    });

                    operationResult.IsSuccess = true;
                    operationResult.State = OperationState.Published;
                } catch (System.Exception ex) {
                    operationResult.IsSuccess = false;
                    operationResult.Error = ex;
                    operationResult.State = OperationState.Failed;
                } finally {
                    if (_uniqueChannel) {
                        Handlers (channel, dismantle : true);
                        channel?.Close ();
                    }
                }

                return operationResult;
            }
            #endregion

            #region PublisherConfirm
            private void Handlers (IModel channel, bool dismantle = false) {
                if (Mandatory) {
                    if (dismantle) {
                        channel.BasicReturn -= HandleReturn;
                    } else {
                        channel.BasicReturn += HandleReturn;
                    }
                }
                if (_useConfirm) {
                    if (dismantle) {
                        channel.BasicNacks -= HandleNack;
                        channel.BasicAcks -= HandleAck;
                    } else {
                        channel.BasicNacks += HandleNack;
                        channel.BasicAcks += HandleAck;
                    }
                }
            }

            private async void HandleReturn (object sender, BasicReturnEventArgs eventArgs) {
                await _returnCallback (eventArgs);
            }

            private async void HandleAck (object sender, BasicAckEventArgs eventArgs) {
                await _ackCallback (eventArgs);
            }

            private async void HandleNack (object sender, BasicNackEventArgs eventArgs) {
                await _nackCallback (eventArgs);
            }

            public static IBasicProperties ConstructProperties (IBasicProperties basicProperties, bool persistent, int? expires) {
                basicProperties.Persistent = persistent;
                basicProperties.Timestamp = new AmqpTimestamp (DateTimeOffset.UtcNow.ToUnixTimeSeconds ());
                basicProperties.Type = typeof (T).FullName;
                if (expires.HasValue) {
                    basicProperties.Expiration = expires.Value.ToString ();
                }
                basicProperties.CorrelationId = Guid.NewGuid ().ToString ();
                basicProperties.ContentType = Config.ContentType;
                basicProperties.ContentEncoding = Config.ContentEncoding;

                return basicProperties;
            }
            #endregion

            #region Declarations
            public IPublish<T> AsMandatory (Func<BasicReturnEventArgs, Task> onReturn) {
                _returnCallback = onReturn;
                Mandatory = true;
                return this;
            }

            public IPublish<T> AsPersistent () {
                Persistent = true;
                return this;
            }

            public IPublish<T> WithConfirm (Func<BasicAckEventArgs, Task> onAck, Func<BasicNackEventArgs, Task> onNack) {
                if (onAck == null ||  onNack == null) {
                    throw DeclarationException.Argument (new ArgumentException ("handlers for ack and nack must not be null"));
                }
                _useConfirm = true;
                _ackCallback = onAck;
                _nackCallback = onNack;
                return this;
            }

            public IPublish<T> WithExpire (uint expire) {
                Expires = (int) expire;
                return this;
            }

            public IPublish<T> WithSerialize (Func<T, byte[]> serialize) {
                _serialize = serialize;
                return this;
            }

            public IPublish<T> WithRoutingKey (string routingKey) {
                _routingKey = routingKey;
                return this;
            }

            public IPublish<T> UseUniqueChannel (bool uniqueChannel = true) {
                _uniqueChannel = uniqueChannel;
                return this;
            }

            public IPublish<T> WithQueueDeclare (string queueName = null, string routingKey = null, string exchangeName = "amq.direct") {
                string name = queueName ?? typeof (T).FullName;
                string rKey = routingKey ?? typeof (T).FullName;
                _queueDeclare = _bunny.Setup ().Queue (name).Bind (exchangeName, rKey).AsDurable ();
                return this;
            }

            public IPublish<T> WithQueueDeclare (IQueue queueDeclare) {
                _queueDeclare = queueDeclare;
                return this;
            }
            #endregion

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose (bool disposing) {
                if (!disposedValue) {
                    if (disposing) {
                        Handlers (_thisChannel.Channel, dismantle : true);
                        _thisChannel.Dispose ();
                    }
                    disposedValue = true;
                }
            }
            public void Dispose () {
                Dispose (true);
            }
            #endregion
        }
}