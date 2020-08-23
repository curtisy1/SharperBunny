namespace SharperBunny.Declare {
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   entry for declaration builder
  /// </summary>
  public class DeclareBase : IDeclare {
    public IBunny Bunny { get; set; }

    public void Declare() {
      throw DeclarationException.BaseNotValid();
    }
  }
}