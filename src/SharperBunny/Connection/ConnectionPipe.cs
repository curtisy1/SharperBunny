namespace SharperBunny.Connection {
  using System;
  using SharperBunny.Interfaces;

  internal class ConnectionPipe : IConnectionPipe {
    private string host;
    private string password;
    private uint port;
    private string user;
    private string vHost;

    public IConnectionPipe AuthenticatePlain(string user = "guest", string password = "guest") {
      this.user = user;
      this.password = password;
      return this;
    }

    public IBunny Connect() {
      if (string.IsNullOrWhiteSpace(this.user)) {
        this.AuthenticatePlain();
      }

      if (string.IsNullOrWhiteSpace(this.host)) {
        this.ToHost();
      }

      if (string.IsNullOrWhiteSpace(this.vHost)) {
        this.ToVirtualHost();
      }

      if (this.port == 0) {
        this.ToPort();
      }

      return Bunny.ConnectSingle(this.ToString("amqp", null));
    }

    public IConnectionPipe ToHost(string hostName = "localhost") {
      this.host = hostName;
      return this;
    }

    public IConnectionPipe ToPort(uint port = 5672) {
      this.port = port;
      return this;
    }

    public IConnectionPipe ToVirtualHost(string vHost = "/") {
      this.vHost = vHost == "/" ? "%2F" : vHost;

      return this;
    }

    public IConnectionPipe WithRetries(int retries, int timeout) {
      Bunny.RetryCount = retries;
      Bunny.RetryPauseInMs = timeout;
      return this; 
    }

    public IConnectionPipe UseAsyncEvents(bool useAsync = true) {
      Bunny.UseAsyncEvents = useAsync;
      return this;
    }

    public string ToString(string format, IFormatProvider formatProvider) {
      var result = $"amqp://{this.user}:{this.password}@{this.host}:{this.port}/{this.vHost}";
      return result;
    }
  }
}