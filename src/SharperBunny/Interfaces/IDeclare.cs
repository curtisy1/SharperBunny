namespace SharperBunny.Interfaces {
  using System.Threading.Tasks;

  /// <summary>
  ///   Entry point to setup RabbitMQ Entities.
  /// </summary>
  public interface IDeclare {
    IBunny Bunny { get; }

    /// <summary>
    ///   Execute the Declaration
    /// </summary>
    Task DeclareAsync();
  }
}