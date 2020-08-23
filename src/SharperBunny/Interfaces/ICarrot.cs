namespace SharperBunny.Interfaces {
  using System.Threading.Tasks;
  using RabbitMQ.Client;

  /// <summary>
  ///   Send BasicAck and BasicNack on the consumer side.
  /// </summary>
  public interface ICarrot<TMsg> {
    ulong DeliveryTag { get; }
    TMsg Message { get; }
    IBasicProperties MessageProperties { get; }
    Task<OperationResult<TMsg>> SendAck(bool multiple = false);
    Task<OperationResult<TMsg>> SendNack(bool multiple = false, bool withRequeue = true);
  }
}