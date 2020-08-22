namespace SharperBunny {
  using System;
  using System.Threading.Tasks;
  using SharperBunny.Consume;
  using SharperBunny.Declare;
  using SharperBunny.Interfaces;
  using SharperBunny.Publish;

  public static class OutwardExtensions {
    /// <summary>
    ///   Create a Publisher Builder interface. Can Also be used to publish messages.
    /// </summary>
    public static IPublish<TMsg> Publisher<TMsg>(this IBunny bunny, string publishToExchange)
      where TMsg : class {
      return new DeclarePublisher<TMsg>(bunny, publishToExchange);
    }

    /// <summary>
    ///   Create a Consumer to subscribe to a Queue. If no queue is specified the Queue Name will be AssemblyName.TypeName
    /// </summary>
    public static IConsume<TMsg> Consumer<TMsg>(this IBunny bunny, string fromQueue = null) {
      if (fromQueue == null) {
        fromQueue = SerializeTypeName<TMsg>();
      }

      return new DeclareConsumer<TMsg>(bunny, fromQueue);
    }

    /// <summary>
    ///   Create a Requester to send Rpc Requests. If no routingKey is specified the routingKey will be AssemblyName.TypeName
    /// </summary>
    public static IRequest<TRequest, TResponse> Request<TRequest, TResponse>(this IBunny bunny, string rpcExchange, string routingKey = null)
      where TRequest : class
      where TResponse : class {
      if (routingKey == null) {
        routingKey = SerializeTypeName<TRequest>();
      }

      return new DeclareRequest<TRequest, TResponse>(bunny, rpcExchange, routingKey);
    }

    /// <summary>
    ///   Other side of the Rpc Call. Consumes fromQueue. If not Specified does consume from AssemblyName.TypeName
    /// </summary>
    public static IRespond<TRequest, TResponse> Respond<TRequest, TResponse>(this IBunny bunny, string rpcExchange, Func<TRequest, Task<TResponse>> respond, string fromQueue = null)
      where TRequest : class
      where TResponse : class {
      if (fromQueue == null) {
        fromQueue = SerializeTypeName<TRequest>();
      }

      return new DeclareResponder<TRequest, TResponse>(bunny, rpcExchange, fromQueue, respond);
    }

    /// <summary>
    ///   Interface for building Queues, Exchanges, Bindings and so on
    /// </summary>
    public static IDeclare Setup(this IBunny bunny) {
      return new DeclareBase { Bunny = bunny };
    }

    private static string SerializeTypeName<T>() {
      return SerializeTypeName(typeof(T));
    }

    private static string SerializeTypeName(Type t) {
      return $"{t.Assembly.GetName().Name}.{t.Name}";
    }

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
    public static IQueue WithTTL(this IQueue queue, uint ttl) {
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