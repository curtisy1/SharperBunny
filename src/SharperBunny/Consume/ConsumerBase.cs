namespace SharperBunny.Consume {
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Text.Json;
  using SharperBunny.Connection;
  using SharperBunny.Interfaces;

  public class ConsumerBase<TMsg> : IConsumerBase, IDisposable {
    protected internal readonly Dictionary<string, object> arguments = new Dictionary<string, object>();
    protected readonly IBunny bunny;
    protected readonly PermanentChannel thisChannel;

    protected internal bool autoAck;
    protected string consumeFromQueue;
    protected bool disposedValue;
    protected internal ushort prefetchCount = 50;
    protected internal bool useUniqueChannel;

    protected ConsumerBase(IBunny bunny, string fromQueue) {
      this.bunny = bunny;
      this.consumeFromQueue = fromQueue;
      this.thisChannel = new PermanentChannel(bunny);
    }

    public void Dispose() {
      this.Dispose(true);
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
    
    protected virtual TMsg InternalDeserialize(ReadOnlyMemory<byte> message) {
      return JsonSerializer.Deserialize<TMsg>(Encoding.UTF8.GetString(message.Span));
    }
  }
}