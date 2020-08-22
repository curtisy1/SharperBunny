namespace SharperBunny.Connect {
  using System;
  using SharperBunny.Interfaces;

  internal class ConnectionPipe : IConnectPipe {
    // private uint _retries;
    // private uint _retryPause;
    private string host;
    private string password;
    private uint port;
    private string user;
    private string vHost;

    public IConnectPipe AuthenticatePlain(string user = "guest", string password = "guest") {
      this.user = user;
      this.password = password;
      return this;
    }

    public IBunny Connect() {
      if (string.IsNullOrWhiteSpace(this.user)) {
        this.AuthenticatePlain();
      }

      if (string.IsNullOrWhiteSpace(this.host)) {
        this.host = "localhost";
      }

      if (string.IsNullOrWhiteSpace(this.vHost)) {
        this.ToVirtualHost();
      }

      if (this.port == 0) {
        this.ToPort();
      }

      return Bunny.ConnectSingle(this.ToString("amqp", null));
    }

    public IConnectPipe ToHost(string hostName = "localhost") {
      this.host = hostName;
      return this;
    }

    public IConnectPipe ToPort(uint port = 5672) {
      this.port = port;
      return this;
    }

    public IConnectPipe ToVirtualHost(string vHost = "/") {
      if (vHost == "/") {
        this.vHost = "%2F";
      } else {
        this.vHost = vHost;
      }

      return this;
    }

    public string ToString(string format, IFormatProvider formatProvider) {
      var result = $"amqp://{this.user}:{this.password}@{this.host}:{this.port}/{this.vHost}";
      return result;
    }
  }
}