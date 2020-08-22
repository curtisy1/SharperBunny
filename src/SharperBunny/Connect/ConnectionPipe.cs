namespace SharperBunny.Connect {
  using System;
  using SharperBunny.Interfaces;

  internal class ConnectionPipe : IConnectPipe {
    // private uint _retries;
    // private uint _retryPause;
    private string _host;
    private string _password;
    private uint _port;
    private string _user;
    private string _vHost;

    public IConnectPipe AuthenticatePlain(string user = "guest", string password = "guest") {
      this._user = user;
      this._password = password;
      return this;
    }

    public IBunny Connect() {
      if (string.IsNullOrWhiteSpace(this._user)) {
        this.AuthenticatePlain();
      }

      if (string.IsNullOrWhiteSpace(this._host)) {
        this._host = "localhost";
      }

      if (string.IsNullOrWhiteSpace(this._vHost)) {
        this.ToVirtualHost();
      }

      if (this._port == 0) {
        this.ToPort();
      }

      return Bunny.ConnectSingle(this.ToString("amqp", null));
    }

    public IConnectPipe ToHost(string hostName = "localhost") {
      this._host = hostName;
      return this;
    }

    public IConnectPipe ToPort(uint port = 5672) {
      this._port = port;
      return this;
    }

    public IConnectPipe ToVirtualHost(string vHost = "/") {
      if (vHost == "/") {
        this._vHost = "%2F";
      } else {
        this._vHost = vHost;
      }

      return this;
    }

    public string ToString(string format, IFormatProvider formatProvider) {
      var result = $"amqp://{this._user}:{this._password}@{this._host}:{this._port}/{this._vHost}";
      return result;
    }
  }
}