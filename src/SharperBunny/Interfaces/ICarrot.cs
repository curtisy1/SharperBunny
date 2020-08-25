namespace SharperBunny.Interfaces {
  /// <summary>
  ///   Send BasicAck and BasicNack on the consumer side.
  /// </summary>
  public interface ICarrot<TMsg> : ICarrotBase<TMsg> {
    OperationResult<TMsg> SendAck(bool multiple = false);
    OperationResult<TMsg> SendNack(bool multiple = false, bool withRequeue = true);
  }
}