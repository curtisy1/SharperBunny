namespace SharperBunny.Interfaces {
  using System;
  using RabbitMQ.Client;

  /// <summary>
  ///   Is a Connection Wrapper and describes Extension methods for connecting to a broker or cluster
  /// </summary>
  public interface IBunny : IDisposable {
    /// <summary>
    ///   The connection that is established with the Bunny.Connect methods.
    ///   Only one exists for every Bunny.
    /// </summary>
    IConnection Connection { get; }

    /// <summary>
    ///   Returns the current channel. Creates One if none exists.
    ///   If newOne is set to true, you get a new IModel object
    /// </summary>
    IModel Channel(bool newOne = false);
  }
}