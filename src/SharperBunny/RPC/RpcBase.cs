namespace SharperBunny.RPC {
  using System;
  using SharperBunny.Connection;
  using SharperBunny.Interfaces;
  using SharperBunny.Interfaces.Rpc;
  using SharperBunny.Serializer;

  public class RpcBase<TRequest, TResponse> : Serializable<TResponse>, IRpcBase<TRequest, TResponse>
    where TRequest : class
    where TResponse : class {
    protected const string directReplyTo = "amq.rabbitmq.reply-to";

    protected readonly IBunny bunny;
    protected readonly PermanentChannel thisChannel;
    protected Func<ReadOnlyMemory<byte>, TResponse> deserialize;
    protected bool disposedValue;
    protected Func<TRequest, byte[]> serialize;
    protected bool useUniqueChannel;
    protected bool useTempQueue;

    public RpcBase(IBunny bunny) {
      this.bunny = bunny;
      this.deserialize = this.InternalDeserialize;
      this.serialize = this.InternalSerialize;
      this.thisChannel = new PermanentChannel(this.bunny);
    }
    
    public IRpcBase<TRequest, TResponse> WithTemporaryQueue(bool useTempQueue = true) {
      this.useTempQueue = useTempQueue;
      return this;
    }
    
    public IRpcBase<TRequest, TResponse> WithSerialize(Func<TRequest, byte[]> serialize) {
      this.serialize = serialize;
      return this;
    }

    public IRpcBase<TRequest, TResponse> WithDeserialize(Func<ReadOnlyMemory<byte>, TResponse> deserialize) {
      this.deserialize = deserialize;
      return this;
    }

    public IRpcBase<TRequest, TResponse> WithUniqueChannel(bool useUniqueChannel = true) {
      this.useUniqueChannel = useUniqueChannel;
      return this;
    }
  }
}