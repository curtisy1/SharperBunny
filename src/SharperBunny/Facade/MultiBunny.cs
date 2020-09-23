namespace SharperBunny.Facade {
  using System.Collections.Generic;
  using System.Linq;
  using RabbitMQ.Client;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   Encapsulates the cluster connect
  /// </summary>
  public class MultiBunny : IBunny {
    private readonly IConnectionFactory factory;
    private readonly List<IModel> models = new List<IModel>();

    private IConnection connection;
    private bool disposedValue;

    public MultiBunny(IConnectionFactory factory) {
      this.factory = factory;
      this.connection = factory.CreateConnection();
    }

    public IModel Channel(bool newOne = false) {
      var open = this.models.Where(x => x.IsOpen).ToList();
      this.models.Clear();
      this.models.AddRange(open);
      if (this.models.Any() && !newOne) {
        return this.models.Last();
      }

      this.SetConnected();

      var model = this.connection.CreateModel();
      this.models.Add(model);
      return model;
    }

    public IConnection Connection {
      get {
        this.SetConnected();
        return this.connection;
      }
    }

    public void Dispose() => this.Dispose(true);

    private void SetConnected() {
      if (!this.connection.IsOpen) {
        this.connection = this.factory.CreateConnection();
      }
    }

    protected virtual void Dispose(bool disposing) {
      if (this.disposedValue) {
        return;
      }

      if (disposing) {
        if (this.connection.IsOpen) {
          this.models.ForEach(x => x.Dispose());
          this.connection.Dispose();
        }
      }

      this.disposedValue = true;
    }
  }
}