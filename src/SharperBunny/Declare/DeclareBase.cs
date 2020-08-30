namespace SharperBunny.Declare {
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   entry for declaration builder
  /// </summary>
  public class DeclareBase : IDeclare {
    public IBunny Bunny { get; set; }
    
    public string Name { get; set; }

    internal bool Durable { get; set; }
    
    internal bool AutoDelete { get; set; }

    public DeclareBase(IBunny bunny) {
      this.Bunny = bunny;
    }

    public DeclareBase(IBunny bunny, string name) {
      this.Bunny = bunny;
      this.Name = name;
    }

    public virtual IDeclare AsDurable() {
      this.Durable = true;
      return this;
    }

    public virtual IDeclare AsAutoDelete() {
      this.AutoDelete = true;
      return this;
    }

    public virtual void Declare() {
      throw DeclarationException.BaseNotValid();
    }
  }
}