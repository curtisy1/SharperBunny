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

    IQueue SetExclusive(bool exclusive = false);

    /// <summary>
    ///   Republish Messages to this Exchange if the are either expired, or not requeued on BasicReject/BasicNack.
    /// </summary>
    public IQueue DeadLetterExchange(string deadLetterExchange);

    /// <summary>
    ///   If send to the DeadLetter Exchange, use this routing-key instead of the original.
    /// </summary>
    public IQueue DeadLetterRoutingKey(string routingKey);

    /// <summary>
    ///   Set a time when the Queue will expire if no action is taken on the queue
    /// </summary>
    public IQueue QueueExpiry(int expiry);

    /// <summary>
    ///   Set max length of Messages on the Queue
    /// </summary>
    public IQueue MaxLength(int length);

    /// <summary>
    ///   Set max bytes on this Queue.
    /// </summary>
    public IQueue MaxLengthBytes(int lengthBytes);

    /// <summary>
    ///   Define this Queue as Lazy. Writes all messages automatically to Disk.
    /// </summary>
    public IQueue AsLazy();

    /// <summary>
    ///   Define all incoming messages to this queue with a Time to live (Message expiry)
    /// </summary>
    public IQueue WithTtl(uint ttl);
    /// <summary>
    ///   only takes effect if MaxLength/MaxLengthBytes is set and is overflown
    /// </summary>
    public IQueue OverflowReject();
  }
}