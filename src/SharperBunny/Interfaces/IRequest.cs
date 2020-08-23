namespace SharperBunny.Interfaces {
  using System;
  using System.Threading.Tasks;

  /// <summary>
  ///   RPC client side.
  /// </summary>
  public interface IRequest<TRequest, TResponse> : IDisposable
    where TRequest : class
    where TResponse : class {
    /// <summary>
    ///   Force a Queue declare before sending
    /// </summary>
    IRequest<TRequest, TResponse> WithQueueDeclare(IQueue queue);

    /// <summary>
    ///   Use a new channel for each request
    /// </summary>
    IRequest<TRequest, TResponse> UseUniqueChannel(bool useUnique = true);

    /// <summary>
    ///   Use an autodelete, exclusive queue for each Request. Default is Direct-Reply-to Mode
    /// </summary>
    IRequest<TRequest, TResponse> WithTemporaryQueue(bool useTempQueue = true);

    /// <summary>
    ///   Specify the Serialize method
    /// </summary>
    IRequest<TRequest, TResponse> SerializeRequest(Func<TRequest, byte[]> serialize);

    /// <summary>
    ///   Send the Request
    /// </summary>
    OperationResult<TResponse> Request(TRequest request, bool force = false);

    /// <summary>
    ///   Specify a Deserialize function
    /// </summary>
    IRequest<TRequest, TResponse> DeserializeResponse(Func<ReadOnlyMemory<byte>, TResponse> deserialize);

    /// <summary>
    ///   Force a Queue declare before sending.
    /// </summary>
    IRequest<TRequest, TResponse> WithQueueDeclare(string queue = null, string exchange = null, string routingKey = null);

    /// <summary>
    ///   Specify a timeout in ms for waiting on the Response. Default is 1500 ms
    /// </summary>
    IRequest<TRequest, TResponse> WithTimeOut(int timeOut);
  }
}