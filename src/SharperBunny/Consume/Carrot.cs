namespace SharperBunny.Consume {
  using System;
  using System.Threading.Tasks;
  using RabbitMQ.Client;
  using SharperBunny.Connect;
  using SharperBunny.Interfaces;

  public class Carrot<TMsg> : ICarrot<TMsg> {
    private readonly PermanentChannel _thisChannel;

    public Carrot(TMsg message, ulong deliveryTag, PermanentChannel thisChannel) {
      this.Message = message;
      this.DeliveryTag = deliveryTag;
      this._thisChannel = thisChannel;
    }

    public TMsg Message { get; }

    public ulong DeliveryTag { get; }

    public IBasicProperties MessageProperties { get; set; }

    public async Task<OperationResult<TMsg>> SendAckAsync(bool multiple = false) {
      var result = new OperationResult<TMsg>();
      try {
        await Task.Run(() => this._thisChannel.Channel.BasicAck(this.DeliveryTag, multiple));
        result.IsSuccess = true;
        result.State = OperationState.Acked;
        return result;
      } catch (Exception ex) {
        result.Error = ex;
        result.IsSuccess = false;
        result.State = OperationState.Failed;
      }

      return result;
    }

    public async Task<OperationResult<TMsg>> SendNackAsync(bool multiple = false, bool withRequeue = true) {
      var result = new OperationResult<TMsg>();
      try {
        await Task.Run(() => this._thisChannel.Channel.BasicNack(this.DeliveryTag, multiple, withRequeue));
        result.IsSuccess = true;
        result.State = OperationState.Nacked;

        return result;
      } catch (Exception ex) {
        result.IsSuccess = false;
        result.Error = ex;
        result.State = OperationState.Failed;
      }

      return result;
    }
  }
}