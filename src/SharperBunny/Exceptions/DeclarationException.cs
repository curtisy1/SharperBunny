namespace SharperBunny.Exceptions {
  using System;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   Something with the declaration was off
  /// </summary>
  public class DeclarationException : Exception {
    private DeclarationException(string msg) : base(msg) { }

    private DeclarationException(string msg, Exception inner) : base(msg, inner) { }

    internal static DeclarationException BaseNotValid() {
      return new DeclarationException("you need to specify any declarations at all - e.g. Declare().Queue().BindAs() etc.");
    }

    internal static DeclarationException WrongType(Type desired, IDeclare actual) {
      return new DeclarationException($"required type was: {desired} got {actual?.GetType()} instead");
    }

    internal static DeclarationException Argument(ArgumentException inner) {
      return new DeclarationException(inner.Message, inner);
    }

    internal static DeclarationException DeclareFailed(Exception exception, string msg = "") {
      return new DeclarationException(msg, exception);
    }
  }
}