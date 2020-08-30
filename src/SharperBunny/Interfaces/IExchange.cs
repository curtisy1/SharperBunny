namespace SharperBunny.Interfaces {
  /// <summary>
  ///   Exchange Entity of RabbitMQ
  /// </summary>
  public interface IExchange : IDeclare {
    /// <summary>
    ///   Non routable messages will be sent to this alternate exchange
    /// </summary>
    IExchange AlternateExchange(string alternate);
  }
}