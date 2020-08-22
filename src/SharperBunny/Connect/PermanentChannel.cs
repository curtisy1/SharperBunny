namespace SharperBunny.Connect {
  using System;
  using RabbitMQ.Client;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   Encapsulates reconnect
  /// </summary>
  public class PermanentChannel : IDisposable {
    private readonly IBunny _bunny;
    private IModel _model;
    private bool disposedValue;

    public PermanentChannel(IBunny bunny) {
      this._bunny = bunny;
    }

    public IModel Channel {
      get {
        var create = this._model == null || this._model.IsClosed;
        if (create) {
          this._model = this._bunny.Channel(true);
        }

        return this._model;
      }
    }

    public void Dispose() {
      this.Dispose(true);
    }

    public void StartConfirmMode() {
      this.Channel.ConfirmSelect();
    }

    protected virtual void Dispose(bool disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          this._model?.Close();
        }

        this.disposedValue = true;
      }
    }
  }
}