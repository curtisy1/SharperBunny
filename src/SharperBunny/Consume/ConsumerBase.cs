namespace SharperBunny.Consume {
  using System.Collections.Generic;
  using SharperBunny.Connection;
  using SharperBunny.Interfaces;
  using SharperBunny.Serializer;

  public class ConsumerBase<TMsg> : Serializable<TMsg>, IConsumerBase {
    protected readonly Dictionary<string, object> arguments = new Dictionary<string, object>();
    protected readonly IBunny bunny;
    protected readonly PermanentChannel thisChannel;

    protected bool autoAck;
    protected string consumeFromQueue;
    protected bool disposedValue;
    protected ushort prefetchCount = 50;
    protected bool useUniqueChannel;

    protected ConsumerBase(IBunny bunny, string fromQueue) {
      this.bunny = bunny;
      this.consumeFromQueue = fromQueue;
      this.thisChannel = new PermanentChannel(bunny);
    }

    public IConsumerBase AsAutoAck(bool autoAck = true) {
      this.autoAck = autoAck;
      return this;
    }

    public IConsumerBase AddTag(string tag, object value) {
      if (this.arguments.ContainsKey(tag)) {
        this.arguments[tag] = value;
      } else {
        this.arguments.Add(tag, value);
      }

      return this;
    }

    public IConsumerBase UseUniqueChannel(bool useUnique = true) {
      this.useUniqueChannel = useUnique;
      return this;
    }

    public IConsumerBase Prefetch(ushort prefetchCount = 50) {
      this.prefetchCount = prefetchCount;
      return this;
    }

    public void Dispose() => this.Dispose(true);

    protected virtual void Dispose(bool disposing) {
      if (this.disposedValue) {
        return;
      }

      if (disposing) {
        this.thisChannel.Dispose();
      }

      this.disposedValue = true;
    }
  }
}