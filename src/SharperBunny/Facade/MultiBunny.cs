namespace SharperBunny.Facade {
  using System.Collections.Generic;
  using System.Linq;
  using RabbitMQ.Client;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   Encapsulates the cluster connect
  /// </summary>
  public class MultiBunny : IBunny {
    private readonly IList<AmqpTcpEndpoint> _amqps;
    private readonly IConnectionFactory _factory;
    private readonly List<IModel> _models = new List<IModel>();
    private IConnection _connection;

    public MultiBunny(IConnectionFactory factory, IList<AmqpTcpEndpoint> endpoints) {
      this._factory = factory;
      this._amqps = endpoints;

      this._connection = factory.CreateConnection(endpoints);
    }

    public IModel Channel(bool newOne = false) {
      var open = this._models.Where(x => x.IsOpen).ToList();
      this._models.Clear();
      this._models.AddRange(open);
      if (this._models.Any() == false || newOne) {
        if (this._connection.IsOpen == false) {
          this.SetConnected();
        }

        var model = this._connection.CreateModel();
        this._models.Add(model);
        return model;
      }

      return this._models.Last();
    }

    public IConnection Connection {
      get {
        this.SetConnected();
        return this._connection;
      }
    }

    private void SetConnected() {
      if (this._connection.IsOpen == false) {
        this._connection = this._factory.CreateConnection(this._amqps);
      }
    }

    #region IDisposable Support

    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          if (this._connection.IsOpen) {
            this._connection.Dispose();
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