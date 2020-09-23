namespace SharperBunny.Consume {
  using System;
  using System.Threading.Tasks;
  using SharperBunny.Interfaces;

  public class AsyncCarrot<TMsg> : CarrotBase<TMsg>, IAsyncCarrot<TMsg> {
    public AsyncCarrot(TMsg message, ulong deliveryTag, IPermanentChannel thisChannel)
      : base(message, deliveryTag, thisChannel) { }

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