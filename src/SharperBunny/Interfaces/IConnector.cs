namespace SharperBunny.Interfaces {
  using SharperBunny.Connect;

  /// <summary>
  ///   Builder Interface for Cluster connect
  /// </summary>
  public interface IConnector {
    /// <summary>
    ///   Add a node by sepcifying the amqp uri
    /// </summary>
    IConnector AddNode(string amqpUri);

    /// <summary>
    ///   Add a node by using the ConnectionPipe interface
    /// </summary>
    IConnector AddNode(IConnectPipe pipe);

    /// <summary>
    ///   Add a node by using ConnectionParameters
    /// </summary>
    IConnector AddNode(ConnectionParameters parameters);

    /// <summary>
    ///   specify the Retry attempts
    /// </summary>
    IConnector WithRetry(int retry = 5, int timeout = 2);

    IBunny Connect();
  }
}