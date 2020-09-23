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
    IDeclare SetAutoDelete(bool autoDelete = false);

    /// <summary>
    ///   Durable Exchanges will survive a broker restart
    /// </summary>
    IDeclare SetDurable(bool durable = true);

    /// <summary>
    ///   Execute the Declaration
    /// </summary>
    void Declare();

    public bool PurgeQueue(string name);

    public bool DeleteQueue(string queue, bool force = false);

    public bool QueueExists(string queue);

    public bool DeleteExchange(string exchangeName, bool force = false);

    public bool ExchangeExists(string exchangeName);
  }
}