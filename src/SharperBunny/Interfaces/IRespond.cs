namespace SharperBunny.Interfaces {
  using System;

  /// <summary>
  ///   Responder for Rpc call.
  /// </summary>
  public interface IRespond<TResponse> : IDisposable {
    /// <summary>
    ///   StartsResponding asynchronously to the specified Request. Make sure to have a matching IRequest on the other side.
    ///   Be aware of changing the defaults (with regards to queue naming etc.)--> apply on both sides.
    ///   A Queue is declared that is equal to the Request name, or the provided fromQueue name on creation.
    /// </summary>
    OperationResult<TResponse> StartResponding();
  }
}