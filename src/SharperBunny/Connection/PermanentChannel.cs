namespace SharperBunny.Connection {
  using System;
  using RabbitMQ.Client;
  using SharperBunny.Interfaces;

  /// <summary>
  ///   Encapsulates reconnect
  /// </summary>
  public class PermanentChannel : IPermanentChannel {
    private readonly IBunny bunny;
    private bool disposedValue;
    private IModel model;

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
      if (this.disposedValue) {
        return;
      }

      if (disposing) {
        this.model?.Close();
      }

      this.disposedValue = true;
    }
  }
}