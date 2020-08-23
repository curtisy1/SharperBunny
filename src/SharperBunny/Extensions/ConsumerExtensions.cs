namespace SharperBunny.Extensions {
  using SharperBunny.Consume;

  public static class ConsumerExtensions {
    public static T AsAutoAck<T>(this T consumer, bool autoAck = true) {
      (consumer as ConsumerBase).autoAck = autoAck;
      return consumer;
    }
    
    public static T AddTag<T>(this T consumer, string tag, object value) {
      var consumerBase = consumer as ConsumerBase;
      if (consumerBase.arguments.ContainsKey(tag)) {
        consumerBase.arguments[tag] = value;
      } else {
        consumerBase.arguments.Add(tag, value);
      }
    
      return consumer;
    }
    
    public static T UseUniqueChannel<T>(this T consumer, bool useUnique = true) {
      (consumer as ConsumerBase).useUniqueChannel = useUnique;
      return consumer;
    }
    
    public static T Prefetch<T>(this T consumer, ushort prefetchCount = 50) {
      (consumer as ConsumerBase).prefetchCount = prefetchCount;
      return consumer;
    }
  }
}