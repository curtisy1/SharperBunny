namespace SharperBunny.Extensions {
  using SharperBunny.Interfaces;

  public static class QueueExtensions {
    /// <summary>
    ///   Republish Messages to this Exchange if the are either expired, or not requeued on BasicReject/BasicNack.
    /// </summary>
    public static IQueue DeadLetterExchange(this IQueue queue, string deadLetterExchange) {
      return queue.AddTag("x-dead-letter-exchange", deadLetterExchange);
    }

    /// <summary>
    ///   If send to the DeadLetter Exchange, use this routing-key instead of the original.
    /// </summary>
    public static IQueue DeadLetterRoutingKey(this IQueue queue, string routingKey) {
      return queue.AddTag("x-dead-letter-routing-key", routingKey);
    }

    /// <summary>
    ///   Set a time when the Queue will expire if no action is taken on the queue
    /// </summary>
    public static IQueue QueueExpiry(this IQueue queue, int expiry) {
      return queue.AddTag("x-expires", expiry);
    }

    /// <summary>
    ///   Set max length of Messages on the Queue
    /// </summary>
    public static IQueue MaxLength(this IQueue queue, int length) {
      return queue.AddTag("x-max-length", length);
    }

    /// <summary>
    ///   Set max bytes on this Queue.
    /// </summary>
    public static IQueue MaxLengthBytes(this IQueue queue, int lengthBytes) {
      return queue.AddTag("x-max-length-bytes", lengthBytes);
    }

    /// <summary>
    ///   Define this Queue as Lazy. Writes all messages automatically to Disk.
    /// </summary>
    public static IQueue AsLazy(this IQueue queue) {
      return queue.AddTag("x-queue-mode", "lazy");
    }

    /// <summary>
    ///   Define all incoming messages to this queue with a Time to live (Message expiry)
    /// </summary>
    public static IQueue WithTtl(this IQueue queue, uint ttl) {
      return queue.AddTag("x-message-ttl", ttl);
    }

    /// <summary>
    ///   only takes effect if MaxLength/MaxLengthBytes is set and is overflown
    /// </summary>
    public static IQueue OverflowReject(this IQueue queue) {
      return queue.AddTag("x-overflow", "reject-publish");
    }
  }
}