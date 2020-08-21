using System;
using RabbitMQ.Client;

namespace SharperBunny.Connect {
    ///<summary>
    /// Encapsulates reconnect
    ///</summary>
    public class PermanentChannel : IDisposable {
        private IModel _model;
        private readonly IBunny _bunny;
        public PermanentChannel (IBunny bunny) {
            _bunny = bunny;
        }

        public IModel Channel {
            get {
                bool create = _model == null  || _model.IsClosed;
                if (create) {
                    _model = _bunny.Channel (newOne: true);
                }

                return _model;
            }
        }

        public void StartConfirmMode () => Channel.ConfirmSelect ();
        private bool disposedValue = false;

        protected virtual void Dispose (bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _model?.Close ();
                }
                disposedValue = true;
            }
        }

        public void Dispose () {
            Dispose (true);
        }
    }
}