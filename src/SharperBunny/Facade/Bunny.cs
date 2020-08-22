namespace SharperBunny.Facade {
  using System.Collections.Generic;
  using System.Linq;
  using RabbitMQ.Client;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   Encapsulates the single broker connect
  /// </summary>
  public class Bunny : IBunny {
    private readonly ConnectionFactory _factory;
    private readonly List<IModel> _model = new List<IModel>();
    private IConnection _connection;

    public Bunny(ConnectionFactory fac) {
      this._connection = fac.CreateConnection();
      this._factory = fac;
    }

    public IModel Channel(bool newOne = false) {
      var open = this._model.Where(x => x.IsOpen).ToList();
      this._model.Clear();
      this._model.AddRange(open);
      if (this._model.Any() == false || newOne) {
        if (this._connection.IsOpen == false) {
          this._connection = this._factory.CreateConnection();
        }

        var model = this._connection.CreateModel();
        this._model.Add(model);
        return model;
      }

      return this._model.Last();
    }

    public IConnection Connection {
      get {
        if (this._connection.IsOpen) {
          return this._connection;
        }

        return this._factory.CreateConnection();
      }
    }

    #region IDisposable Support

    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          if (this._connection.IsOpen) {
            this._model.ForEach(x => x.Dispose());
            this._connection.Close();
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