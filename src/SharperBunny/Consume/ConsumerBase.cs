namespace SharperBunny.Consume {
  using System;
  using System.Collections.Generic;
  using SharperBunny.Connection;
  using SharperBunny.Interfaces;

  public class ConsumerBase<TMsg> : IConsumerBase<TMsg> {
    protected readonly Dictionary<string, object> arguments = new Dictionary<string, object>();
    protected readonly IBunny bunny;
    protected readonly PermanentChannel thisChannel;

    protected bool autoAck;
    protected ushort prefetchCount = 50;
    protected bool useUniqueChannel;
    protected string consumeFromQueue;
    protected bool disposedValue;

    protected ConsumerBase(IBunny bunny, string fromQueue) {
      this.bunny = bunny;
      this.consumeFromQueue = fromQueue;
      this.thisChannel = new PermanentChannel(bunny);
    }
    
    public virtual IConsumerBase<TMsg> AddTag(string tag, object value) {
      if (this.arguments.ContainsKey(tag)) {
        this.arguments[tag] = value;
      } else {
        this.arguments.Add(tag, value);
      }

      return this;
    }
    
    public virtual IConsumerBase<TMsg> UseUniqueChannel(bool useUnique = true) {
      this.useUniqueChannel = useUnique;
      return this;
    }

    public virtual IConsumerBase<TMsg> AsAutoAck(bool autoAck = true) {
      this.autoAck = autoAck;
      return this;
    }

    public virtual IConsumerBase<TMsg> Prefetch(ushort prefetchCount = 50) {
      this.prefetchCount = prefetchCount;
      return this;
    }

    protected virtual void Dispose(bool disposing) {
      if (this.disposedValue) {
        return;
      }

      if (disposing) {
        this.thisChannel.Dispose();
      }

      this.disposedValue = true;
    }

    public void Dispose() {
      this.Dispose(true);
    }
  }
}