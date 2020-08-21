using System;

namespace SharperBunny.Connect {
    internal class ConnectionPipe : IConnectPipe {
        private string _user;
        private string _password;
        // private uint _retries;
        // private uint _retryPause;
        private string _host;
        private uint _port;
        private string _vHost;

        public IConnectPipe AuthenticatePlain (string user = "guest", string password = "guest") {
            _user = user;
            _password = password;
            return this;
        }

        public IBunny Connect () {
            if (string.IsNullOrWhiteSpace (_user)) {
                AuthenticatePlain ();
            }
            if (string.IsNullOrWhiteSpace (_host)) {
                _host = "localhost";
            }
            if (string.IsNullOrWhiteSpace (_vHost)) {
                ToVirtualHost ();
            }
            if (_port == 0) {
                ToPort ();
            }
            return Bunny.ConnectSingle (this.ToString ("amqp", null));
        }

        public IConnectPipe ToHost (string hostName = "localhost") {
            _host = hostName;
            return this;
        }

        public IConnectPipe ToPort (uint port = 5672) {
            _port = port;
            return this;
        }

        public IConnectPipe ToVirtualHost (string vHost = "/") {
            if (vHost == "/") {
                _vHost = "%2F";
            } else {
                _vHost = vHost;
            }
            return this;
        }

        public string ToString (string format, IFormatProvider formatProvider) {
            string result = $"amqp://{_user}:{_password}@{_host}:{_port}/{_vHost}";
            return result;
        }
    }
}