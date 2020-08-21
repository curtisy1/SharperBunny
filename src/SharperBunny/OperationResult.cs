using System;

namespace SharperBunny {
    ///<summary>
    /// Indicates the Result of a Queue / Broker interaction / operation. If IsSuccess the Message is non empty, else the Error property is non empty
    ///</summary>
    public class OperationResult<T> {
        public bool IsSuccess { get; internal set; }
        public T Message { get; internal set; }
        public Exception Error { get; internal set; }

        public OperationState State { get; internal set; }
    }

    public enum OperationState {
        Failed = 1,
        ConsumerAttached = 2,
        Published = 3,
        Get = 4,
        Acked = 5,
        Nacked = 6,
        GetFailed = 7,
        RequestFailed = 8,
        ResponseFailed = 9,
        RpcSucceeded = 10,
        Response = 11,
        RpcReplyFailed = 12,
        RpcPublished = 13,
    }
}