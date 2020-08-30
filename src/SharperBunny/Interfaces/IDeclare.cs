namespace SharperBunny.Interfaces {
  /// <summary>
  ///   Entry point to setup RabbitMQ Entities.
  /// </summary>
  public interface IDeclare {
    IBunny Bunny { get; }
    
    string Name { get; set; }

    /// <summary>
    ///   Exchange defined as autodelete
    /// </summary>
    IDeclare AsAutoDelete();

    /// <summary>
    ///   Durable Exchanges will survive a broker restart
    /// </summary>
    IDeclare AsDurable();

    /// <summary>
    ///   Execute the Declaration
    /// </summary>
    void Declare();
  }
}