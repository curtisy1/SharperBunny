using System;
using RabbitMQ.Client;

namespace SharperBunny {
    ///<summary>
    /// Is a Connection Wrapper and describes Extension methods for connecting to a broker or cluster
    ///</summary>
    public interface IBunny : IDisposable {
        ///<summary>
        /// Returns the current channel. Creates One if none exists. 
        /// If newOne is set to true, you get a new IModel object
        ///</summary>
        IModel Channel (bool newOne = false);
        ///<summary>
        /// The connection that is established with the Bunny.Connect methods.
        /// Only one exists for every Bunny.
        ///</summary>
        IConnection Connection { get; }
    }
}