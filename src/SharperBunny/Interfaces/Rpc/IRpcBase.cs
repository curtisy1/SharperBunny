namespace SharperBunny.Interfaces.Rpc {
  using System;

  public interface IRpcBase<TRequest, TResponse>
  where TRequest : class 
  where TResponse : class {
    /// <summary>
    ///   Use an autodelete, exclusive queue for each Request. Default is Direct-Reply-to Mode
    /// </summary>
    IRpcBase<TRequest, TResponse> WithTemporaryQueue(bool useTempQueue = true);

    /// <summary>
    ///   Override serialize method
    /// </summary>
    IRpcBase<TRequest, TResponse> WithSerialize(Func<TRequest, byte[]> serialize);

    /// <summary>
    ///   Override Deserialize method
    /// </summary>
    IRpcBase<TRequest, TResponse> WithDeserialize(Func<ReadOnlyMemory<byte>, TResponse> deserialize);

    /// <summary>
    ///   If applied, uses a new channel for each Response (publish with reply_to tag)
    /// </summary>
    IRpcBase<TRequest, TResponse> WithUniqueChannel(bool useUniqueChannel = true);
  }
}