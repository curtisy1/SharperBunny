namespace SharperBunny {
    ///<summary>
    /// Exchange Entity of RabbitMQ
    ///</summary>
    public interface IExchange : IDeclare {
        string Name { get; }
        ///<summary>
        /// Exchange defined as autodelete
        ///</summary>
        IExchange AsAutoDelete ();
        ///<summary>
        /// Durable Exchanges will survive a broker restart
        ///</summary>
        IExchange AsDurable ();
        ///<summary>
        /// Define an Exchange as Internal
        ///</summary>
        IExchange Internal ();
        ///<summary>
        /// Non routeable messages will be sent to this alternate exchange
        ///</summary>
        IExchange AlternateExchange (string alternate);
    }
}