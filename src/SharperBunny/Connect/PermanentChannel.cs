namespace SharperBunny.Connect {
  using System;
  using RabbitMQ.Client;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   Encapsulates reconnect
  /// </summary>
  public class PermanentChannel : IDisposable {
    private readonly IBunny bunny;
    private IModel model;
    private bool disposedValue;

    public PermanentChannel(IBunny bunny) {
      this.bunny = bunny;
    }

    public IModel Channel {
      get {
        var create = this.model == null || this.model.IsClosed;
        if (create) {
          this.model = this.bunny.Channel(true);
        }

        return this.model;
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
          this.model?.Close();
        }

        this.disposedValue = true;
      }
    }
  }
}