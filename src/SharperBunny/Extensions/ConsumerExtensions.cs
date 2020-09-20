namespace SharperBunny.Extensions {
  using SharperBunny.Consume;
  using SharperBunny.Interfaces;

  public static class ConsumerExtensions {
    public static T AsAutoAck<T>(this T consumer, bool autoAck = true)
      where T : IConsumerBase {
      (consumer as ConsumerBase<T>).autoAck = autoAck;
      return consumer;
    }

    public static T AddTag<T>(this T consumer, string tag, object value)
      where T : IConsumerBase {
      var consumerBase = consumer as ConsumerBase<T>;
      if (consumerBase.arguments.ContainsKey(tag)) {
        consumerBase.arguments[tag] = value;
      } else {
        consumerBase.arguments.Add(tag, value);
      }

      return consumer;
    }

    public static T UseUniqueChannel<T>(this T consumer, bool useUnique = true)
      where T : IConsumerBase {
      (consumer as ConsumerBase<T>).useUniqueChannel = useUnique;
      return consumer;
    }

    public static T Prefetch<T>(this T consumer, ushort prefetchCount = 50)
      where T : IConsumerBase {
      (consumer as ConsumerBase<T>).prefetchCount = prefetchCount;
      return consumer;
    }
  }
}