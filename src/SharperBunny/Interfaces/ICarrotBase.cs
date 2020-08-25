namespace SharperBunny.Interfaces {
  using RabbitMQ.Client;

  public interface ICarrotBase<out TMsg> {
    ulong DeliveryTag { get; }
    TMsg Message { get; }
    IBasicProperties MessageProperties { get; }
  }
}