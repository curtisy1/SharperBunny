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

    public virtual IDeclare SetDurable(bool durable = true) {
      this.Durable = durable;
      return this;
    }

    public virtual IDeclare SetAutoDelete(bool autoDelete = false) {
      this.AutoDelete = autoDelete;
      return this;
    }

    public virtual void Declare() {
      throw DeclarationException.BaseNotValid();
    }
  }
}