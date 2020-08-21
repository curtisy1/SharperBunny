using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace SharperBunny {
    ///<summary>
    /// Define a Consumer and its traits. Use StartConsume for Push API or Get for Pull API.
    ///</summary>
    public interface IConsume<TMsg> : IDisposable {
        ///<summary>
        /// Define what your consumer does with the message. Carrot helps to ack/nack messages
        ///</summary>
        IConsume<TMsg> Callback (Func<ICarrot<TMsg>, Task> callback);
        ///<summary>
        /// define basic quality of service
        ///</summary>
        IConsume<TMsg> Prefetch (uint prefetchCount = 50);
        ///<summary>
        /// Use a new Channel being established for each Basic.Get. Has no effect for the BasicConsume.
        ///</summary>
        IConsume<TMsg> UseUniqueChannel (bool useUnique = true);
        ///<summary>
        /// Define the consumer as auto acknowledgement for best possible performance, yet least message reliability.
        ///</summary>
        IConsume<TMsg> AsAutoAck (bool autoAck = true);

        ///<summary>
        /// Define custom behaviour for acknowledging messages. Will automatically turn off auto acknowledgement.
        ///</summary>
        IConsume<TMsg> AckBehaviour (Func<ICarrot<TMsg>, Task> ackBehaviour);

        ///<summary>
        /// Define custom behaviour for rejecting messages when acknowledging failed.
        ///</summary>
        IConsume<TMsg> NackBehaviour (Func<ICarrot<TMsg>, Task> nackBehaviour);

        ///<summary>
        /// Specify your own Desiarilze function for deserializing the message. Default is Json.
        ///</summary>
        IConsume<TMsg> DeserializeMessage (Func<ReadOnlyMemory<byte>, TMsg> deserialize);

        ///<summary>
        /// Starts consuming from the specified or forceDeclared Queue
        /// force not null --> creates the queue to be consumed from
        ///</summary>
        Task<OperationResult<TMsg>> StartConsumingAsync (IQueue force = null);
        ///<summary>
        /// Basic.Get Functionality. Aka Pull API
        /// Leave out the carrot.SendAckAsync if you use AutoAck!
        ///</summary>
        Task<OperationResult<TMsg>> GetAsync (Func<ICarrot<TMsg>, Task> carrot);

        ///<summary>
        /// BasicCancel send for this Consumer. Ends subscription to the broker.
        ///</summary>
        Task CancelAsync ();
    }

    ///<summary>
    /// Send BasicAck and BasicNack on the consumer side. 
    ///</summary>
    public interface ICarrot<TMsg> {
        ulong DeliveryTag { get; }
        TMsg Message { get; }
        IBasicProperties MessageProperties { get; }
        Task<OperationResult<TMsg>> SendAckAsync (bool multiple = false);
        Task<OperationResult<TMsg>> SendNackAsync (bool multiple = false, bool withRequeue = true);
    }
}