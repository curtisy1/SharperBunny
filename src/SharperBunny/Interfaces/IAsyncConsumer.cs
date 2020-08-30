namespace SharperBunny.Interfaces {
  using System;
  using System.Threading.Tasks;

  public interface IAsyncConsumer<TMsg> : IConsumerBase {
    /// <summary>
    ///   Starts consuming from the specified or forceDeclared Queue
    ///   force not null --> creates the queue to be consumed from
    /// </summary>
    Task<OperationResult<TMsg>> StartConsuming(IQueue force = null);

    /// <summary>
    ///   Define what your consumer does with the message. Carrot helps to ack/nack messages
    /// </summary>
    IAsyncConsumer<TMsg> Callback(Func<IAsyncCarrot<TMsg>, Task> callback);

    /// <summary>
    ///   Define custom behaviour for acknowledging messages. Will automatically turn off auto acknowledgement.
    /// </summary>
    IAsyncConsumer<TMsg> AckBehaviour(Func<IAsyncCarrot<TMsg>, Task> ackBehaviour);

    /// <summary>
    ///   Define custom behaviour for rejecting messages when acknowledging failed.
    /// </summary>
    IAsyncConsumer<TMsg> NackBehaviour(Func<IAsyncCarrot<TMsg>, Task> nackBehaviour);

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