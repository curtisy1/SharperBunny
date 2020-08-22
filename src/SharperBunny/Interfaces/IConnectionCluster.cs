namespace SharperBunny.Interfaces {
  using SharperBunny.Connection;

  /// <summary>
  ///   Builder Interface for Cluster connect
  /// </summary>
  public interface IConnectionCluster {
    /// <summary>
    ///   Add a node by specifying the amqp uri
    /// </summary>
    IConnectionCluster AddNode(string amqpUri);

    /// <summary>
    ///   Add a node by using the ConnectionPipe interface
    /// </summary>
    IConnectionCluster AddNode(IConnectionPipe pipe);

    /// <summary>
    ///   Add a node by using ConnectionParameters
    /// </summary>
    IConnectionCluster AddNode(ConnectionParameters parameters);

    /// <summary>
    ///   specify the Retry attempts
    /// </summary>
    IConnectionCluster WithRetries(int retry = 5, int timeout = 2);

    IBunny Connect();
  }
}