namespace SharperBunny.Tests.Declaration {
  using FluentAssertions;
  using SharperBunny.Declare;
  using SharperBunny.Exceptions;
  using Xunit;

  public class DeclareBaseTests {
    [Fact]
    public void DeclareBase_ThrowsException() {
      var declareBase = new DeclareBase(default);

      declareBase.Invoking(b => b.Declare()).Should().Throw<DeclarationException>();
    }
  }
}