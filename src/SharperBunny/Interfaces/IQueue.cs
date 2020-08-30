namespace SharperBunny.Interfaces {
  public interface IQueue : IDeclare {
    /// <summary>
    ///   Defaults to Queue.Name. Is set with the Bind Method.
    /// </summary>
    string RoutingKey { get; }

    /// <summary>
    ///   Bind a Queue to [exchangeName] with [routingKey]
    /// </summary>
    IQueue Bind(string exchangeName, string routingKey);

    /// <summary>
    ///   Add any tag and its value. Better use the extension methods for this.
    /// </summary>
    IQueue AddTag(string tag, object value);
  }
}