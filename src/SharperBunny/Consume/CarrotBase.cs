namespace SharperBunny.Consume {
  using RabbitMQ.Client;
  using SharperBunny.Connection;
  using SharperBunny.Interfaces;

  public class CarrotBase<TMsg> : ICarrotBase<TMsg> {
    protected readonly PermanentChannel thisChannel;

    protected CarrotBase(TMsg message, ulong deliveryTag, PermanentChannel thisChannel) {
      this.Message = message;
      this.DeliveryTag = deliveryTag;
      this.thisChannel = thisChannel;
    }

    public ulong DeliveryTag { get; }
    public TMsg Message { get; }
    public IBasicProperties MessageProperties { get; set; }
  }
}