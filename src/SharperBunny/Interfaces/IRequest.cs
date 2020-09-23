namespace SharperBunny.Interfaces {
  using System;

  /// <summary>
  ///   RPC client side.
  /// </summary>
  public interface IRequest<in TRequest, TResponse> : IDisposable
    where TRequest : class
    where TResponse : class {
    /// <summary>
    ///   Force a Queue declare before sending
    /// </summary>
    IRequest<TRequest, TResponse> WithQueueDeclare(IQueue queue);

    /// <summary>
    ///   Send the Request
    /// </summary>
    OperationResult<TResponse> Request(TRequest request, bool force = false);

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