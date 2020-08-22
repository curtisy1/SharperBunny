namespace SharperBunny.Facade {
  using System.Collections.Generic;
  using System.Linq;
  using RabbitMQ.Client;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   Encapsulates the single broker connect
  /// </summary>
  public class Bunny : IBunny {
    private readonly ConnectionFactory factory;
    private readonly List<IModel> model = new List<IModel>();
    private IConnection connection;

    public Bunny(ConnectionFactory fac) {
      this.connection = fac.CreateConnection();
      this.factory = fac;
    }

    public IModel Channel(bool newOne = false) {
      var open = this.model.Where(x => x.IsOpen).ToList();
      this.model.Clear();
      this.model.AddRange(open);
      if (this.model.Any() == false || newOne) {
        if (this.connection.IsOpen == false) {
          this.connection = this.factory.CreateConnection();
        }

        var model = this.connection.CreateModel();
        this.model.Add(model);
        return model;
      }

      return this.model.Last();
    }

    public IConnection Connection {
      get {
        if (this.connection.IsOpen) {
          return this.connection;
        }

        return this.factory.CreateConnection();
      }
    }

    #region IDisposable Support

    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          if (this.connection.IsOpen) {
            this.model.ForEach(x => x.Dispose());
            this.connection.Close();
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