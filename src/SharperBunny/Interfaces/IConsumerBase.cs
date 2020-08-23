namespace SharperBunny.Interfaces {
  using System;
  using System.Threading.Tasks;
  using RabbitMQ.Client;

  public interface IConsumerWrapper<TConsumer> {
    
  }

  /// <summary>
  ///   Define a Consumer and its traits. Use StartConsume for Push API or Get for Pull API.
  /// </summary>
  public interface IConsumerBase<TMsg> : IDisposable {

    /// <summary>
    ///   define basic quality of service
    /// </summary>
    IConsumerBase<TMsg> Prefetch(ushort prefetchCount = 50);

    /// <summary>
    ///   Use a new Channel being established for each Basic.Get. Has no effect for the BasicConsume.
    /// </summary>
    IConsumerBase<TMsg> UseUniqueChannel(bool useUnique = true);

    /// <summary>
    ///   Define the consumer as auto acknowledgement for best possible performance, yet least message reliability.
    /// </summary>
    IConsumerBase<TMsg> AsAutoAck(bool autoAck = true);

    IConsumerBase<TMsg> AddTag(string tag, object value);
  }
}