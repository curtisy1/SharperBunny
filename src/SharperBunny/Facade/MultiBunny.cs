namespace SharperBunny.Facade {
  using System.Collections.Generic;
  using System.Linq;
  using RabbitMQ.Client;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   Encapsulates the cluster connect
  /// </summary>
  public class MultiBunny : IBunny {
    private readonly IList<AmqpTcpEndpoint> endpoints;
    private readonly IConnectionFactory factory;
    private readonly List<IModel> models = new List<IModel>();
    private IConnection connection;

    public MultiBunny(IConnectionFactory factory, IList<AmqpTcpEndpoint> endpoints) {
      this.factory = factory;
      this.endpoints = endpoints;

      this.connection = factory.CreateConnection(endpoints);
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

    private void SetConnected() {
      if (!this.connection.IsOpen) {
        this.connection = this.factory.CreateConnection(this.endpoints);
      }
    }

    #region IDisposable Support

    private bool disposedValue; // To detect redundant calls

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

    public void Dispose() {
      this.Dispose(true);
    }

    #endregion
  }
}