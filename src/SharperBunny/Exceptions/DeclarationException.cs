using System;

namespace SharperBunny.Exceptions {
    ///<summary>
    /// Something with the declaration was off
    ///</summary>
    public class DeclarationException : Exception {
        internal static DeclarationException BaseNotValid () => new DeclarationException ("you need to specify any declarations at all - e.g. Declare().Queue().BindAs() etc.");

        internal static DeclarationException WrongType (Type desired, IDeclare actual) => new DeclarationException ($"required type was: {desired} got {actual?.GetType()} instead");

        internal static DeclarationException Argument (ArgumentException inner) => new DeclarationException (inner.Message, inner);

        internal static DeclarationException DeclareFailed (Exception exception, string msg = "") => new DeclarationException (msg, exception);

        private DeclarationException (string msg) : base (msg) { }

        private DeclarationException (string msg, Exception inner) : base (msg, inner) { }

    }
}