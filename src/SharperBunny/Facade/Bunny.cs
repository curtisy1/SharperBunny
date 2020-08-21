using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace SharperBunny.Facade {
    ///<summary>
    /// Encapsulates the single broker connect
    ///</summary>
    public class Bunny : IBunny {
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private readonly List<IModel> _model = new List<IModel> ();
        public Bunny (ConnectionFactory fac) {
            _connection = fac.CreateConnection ();;
            _factory = fac;
        }

        public IModel Channel (bool newOne = false) {
            var open = _model.Where (x => x.IsOpen).ToList ();
            _model.Clear ();
            _model.AddRange (open);
            if (_model.Any () == false || newOne) {
                if (_connection.IsOpen == false) {
                    _connection = _factory.CreateConnection ();
                }
                var model = _connection.CreateModel ();
                _model.Add (model);
                return model;
            } else {
                return _model.Last ();
            }
        }

        public IConnection Connection {
            get {
                if (_connection.IsOpen)
                    return _connection;
                else
                    return _factory.CreateConnection ();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose (bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    if (_connection.IsOpen) {
                        _model.ForEach (x => x.Dispose ());
                        _connection.Close ();
                    }
                }
                disposedValue = true;
            }
        }
        public void Dispose () {
            Dispose (true);
        }
        #endregion
    }
}