namespace SharperBunny.Consume {
  using System;
  using System.Threading.Tasks;
  using RabbitMQ.Client;
  using SharperBunny.Connection;
  using SharperBunny.Interfaces;

  public class Carrot<TMsg> : ICarrot<TMsg> {
    private readonly PermanentChannel thisChannel;

    public Carrot(TMsg message, ulong deliveryTag, PermanentChannel thisChannel) {
      this.Message = message;
      this.DeliveryTag = deliveryTag;
      this.thisChannel = thisChannel;
    }

    public TMsg Message { get; }

    public ulong DeliveryTag { get; }

    public IBasicProperties MessageProperties { get; set; }

    public Task<OperationResult<TMsg>> SendAck(bool multiple = false) {
      var result = new OperationResult<TMsg>();
      try {
        this.thisChannel.Channel.BasicAck(this.DeliveryTag, multiple);
        result.IsSuccess = true;
        result.State = OperationState.Acked;
      } catch (Exception ex) {
        result.Error = ex;
        result.IsSuccess = false;
        result.State = OperationState.Failed;
      }

      return Task.FromResult(result);
    }

    public Task<OperationResult<TMsg>> SendNack(bool multiple = false, bool withRequeue = true) {
      var result = new OperationResult<TMsg>();
      try {
        this.thisChannel.Channel.BasicNack(this.DeliveryTag, multiple, withRequeue);
        result.IsSuccess = true;
        result.State = OperationState.Nacked;
      } catch (Exception ex) {
        result.IsSuccess = false;
        result.Error = ex;
        result.State = OperationState.Failed;
      }

      return Task.FromResult(result);
    }
  }
}