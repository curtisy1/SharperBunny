namespace SharperBunny.Interfaces {
  using System;

  public interface IConsumer<TMsg> : IConsumerBase<TMsg> {
    /// <summary>
    ///   Starts consuming from the specified or forceDeclared Queue
    ///   force not null --> creates the queue to be consumed from
    /// </summary>
    OperationResult<TMsg> StartConsuming(IQueue force = null);

    /// <summary>
    ///   Basic.Get Functionality. Aka Pull API
    ///   Leave out the carrot.SendAckAsync if you use AutoAck!
    /// </summary>
    OperationResult<TMsg> Get(Action<ICarrot<TMsg>> carrot);
    
    /// <summary>
    ///   Define what your consumer does with the message. Carrot helps to ack/nack messages
    /// </summary>
    IConsumer<TMsg> Callback(Action<ICarrot<TMsg>> callback);

    /// <summary>
    ///   Define custom behaviour for acknowledging messages. Will automatically turn off auto acknowledgement.
    /// </summary>
    IConsumer<TMsg> AckBehaviour(Action<ICarrot<TMsg>> ackBehaviour);

    /// <summary>
    ///   Define custom behaviour for rejecting messages when acknowledging failed.
    /// </summary>
    IConsumer<TMsg> NackBehaviour(Action<ICarrot<TMsg>> nackBehaviour);

    /// <summary>
    ///   Specify your own Deserialize function for deserializing the message. Default is Json.
    /// </summary>
    IConsumer<TMsg> DeserializeMessage(Func<ReadOnlyMemory<byte>, TMsg> deserialize);

    /// <summary>
    ///   BasicCancel send for this Consumer. Ends subscription to the broker.
    /// </summary>
    void Cancel();
  }
}