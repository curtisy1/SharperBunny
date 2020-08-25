namespace SharperBunny.Interfaces {
  /// <summary>
  ///   Entry point to setup RabbitMQ Entities.
  /// </summary>
  public interface IDeclare {
    IBunny Bunny { get; }

    /// <summary>
    ///   Execute the Declaration
    /// </summary>
    void Declare();
  }
}