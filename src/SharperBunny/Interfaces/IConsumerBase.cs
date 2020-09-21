namespace SharperBunny.Interfaces {
  public interface IConsumerBase {
    public IConsumerBase AsAutoAck(bool autoAck = true);

    public IConsumerBase AddTag(string tag, object value);

    public IConsumerBase UseUniqueChannel(bool useUnique = true);

    public IConsumerBase Prefetch(ushort prefetchCount = 50);
  }
}