namespace SharperBunny.Interfaces {
  using System;
  using System.Threading.Tasks;
  using RabbitMQ.Client.Events;

  /// <summary>
  ///   Create a Publisher
  /// </summary>
  public interface IPublish<T> : IDisposable
    where T : class {
    /// <summary>
    ///   Ensures Publisher confirms. Specify onAck and onNack what you want to do in those events
    ///   , Must be not null.
    /// </summary>
    IPublish<T> WithConfirm(Func<BasicAckEventArgs, Task> onAck, Func<BasicNackEventArgs, Task> onNack);

    /// <summary>
    /// </summary>
    IPublish<T> AsMandatory(Func<BasicReturnEventArgs, Task> onReturn);

    /// <summary>
    ///   Sets deliveryMode equal to 2
    /// </summary>
    IPublish<T> AsPersistent();

    /// <summary>
    ///   Adds expiry to the send message.
    /// </summary>
    IPublish<T> WithExpire(int expire);

    /// <summary>
    ///   Asynchronously sends the Message to the MessageBroker. Force flag creates the exchange if it does not exist yet.
    ///   THe OperationResult tells you if the message was routed successfully. For full message reliability activate
    ///   AsMandatory as well as WithConfirm and handle the respective events.
    /// </summary>
    OperationResult<T> Send(T message, bool force = false);

    IPublish<T> WithSerialize(Func<T, byte[]> serialize);

    /// <summary>
    ///   If not specified the TypeName is used
    /// </summary>
    IPublish<T> WithRoutingKey(string routingKey);

    /// <summary>
    ///   If QueueName == null --> use the typeof(T).FullName property as QueueName. If no routingKey is specified, use the
    ///   Type name.
    ///   Uses a durable Queue by default. If you need further Tuning, Declare a Queue by yourself and use the overload with
    ///   IQueue
    /// </summary>
    IPublish<T> WithQueueDeclare(string queueName = null, string routingKey = null, string exchangeName = "amq.direct");

    /// <summary>
    ///   Before the publish occurs use the IQueue to declare a queue asynchronously
    /// </summary>
    IPublish<T> WithQueueDeclare(IQueue queue);

    /// <summary>
    ///   each publish is done on a seperate channel
    /// </summary>
    IPublish<T> UseUniqueChannel(bool uniqueChannel = true);
  }
}