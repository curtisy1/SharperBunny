namespace SharperBunny.Interfaces {
  using System.Threading.Tasks;

  public interface IAsyncCarrot<TMsg> : ICarrotBase<TMsg> {
    Task<OperationResult<TMsg>> SendAck(bool multiple = false);
    Task<OperationResult<TMsg>> SendNack(bool multiple = false, bool withRequeue = true);
  }
}