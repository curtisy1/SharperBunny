namespace SharperBunny.Facade {
  using System.Collections.Generic;
  using System.Linq;
  using RabbitMQ.Client;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   Encapsulates the cluster connect
  /// </summary>
  public class MultiBunny : IBunny {
    private readonly IList<AmqpTcpEndpoint> amqps;
    private readonly IConnectionFactory factory;
    private readonly List<IModel> models = new List<IModel>();
    private IConnection connection;

    public MultiBunny(IConnectionFactory factory, IList<AmqpTcpEndpoint> endpoints) {
      this.factory = factory;
      this.amqps = endpoints;

      this.connection = factory.CreateConnection(endpoints);
    }

    public IModel Channel(bool newOne = false) {
      var open = this.models.Where(x => x.IsOpen).ToList();
      this.models.Clear();
      this.models.AddRange(open);
      if (this.models.Any() == false || newOne) {
        if (this.connection.IsOpen == false) {
          this.SetConnected();
        }

        var model = this.connection.CreateModel();
        this.models.Add(model);
        return model;
      }

      return this.models.Last();
    }

    public IConnection Connection {
      get {
        this.SetConnected();
        return this.connection;
      }
    }

    private void SetConnected() {
      if (this.connection.IsOpen == false) {
        this.connection = this.factory.CreateConnection(this.amqps);
      }
    }

    #region IDisposable Support

    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          if (this.connection.IsOpen) {
            this.connection.Dispose();
          }
        }

        this.disposedValue = true;
      }
    }

    public void Dispose() {
      this.Dispose(true);
    }

    #endregion
  }
}