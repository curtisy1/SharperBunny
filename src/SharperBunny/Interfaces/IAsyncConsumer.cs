namespace SharperBunny.Interfaces {
  using System;
  using System.Threading.Tasks;

  public interface IAsyncConsumer<TMsg> : IConsumerBase<TMsg> {
    /// <summary>
    ///   Starts consuming from the specified or forceDeclared Queue
    ///   force not null --> creates the queue to be consumed from
    /// </summary>
    Task<OperationResult<TMsg>> StartConsuming(IQueue force = null);

    /// <summary>
    ///   Basic.Get Functionality. Aka Pull API
    ///   Leave out the carrot.SendAckAsync if you use AutoAck!
    /// </summary>
    Task<OperationResult<TMsg>> Get(Func<ICarrot<TMsg>, Task> carrot);
    
    /// <summary>
    ///   Define what your consumer does with the message. Carrot helps to ack/nack messages
    /// </summary>
    IAsyncConsumer<TMsg> Callback(Func<ICarrot<TMsg>, Task> callback);

    /// <summary>
    ///   Define custom behaviour for acknowledging messages. Will automatically turn off auto acknowledgement.
    /// </summary>
    IAsyncConsumer<TMsg> AckBehaviour(Func<ICarrot<TMsg>, Task> ackBehaviour);

    /// <summary>
    ///   Define custom behaviour for rejecting messages when acknowledging failed.
    /// </summary>
    IAsyncConsumer<TMsg> NackBehaviour(Func<ICarrot<TMsg>, Task> nackBehaviour);
    
    /// <summary>
    ///   Specify your own Deserialize function for deserializing the message. Default is Json.
    /// </summary>
    IAsyncConsumer<TMsg> DeserializeMessage(Func<ReadOnlyMemory<byte>, TMsg> deserialize);

    /// <summary>
    ///   BasicCancel send for this Consumer. Ends subscription to the broker.
    /// </summary>
    Task Cancel();
  }
}