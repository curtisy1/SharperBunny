namespace SharperBunny.Connection {
  using System;

  /// <summary>
  ///   Specify ConnectionParameters by using Properties.
  ///   Virtualhost defaults to '/'
  /// </summary>
  public class ConnectionParameters : IFormattable {
    private string vHost;
    public string Host { get; set; }
    public uint Port { get; set; }
    public string User { get; set; }
    public string Password { get; set; }

    public string VirtualHost {
      get => this.vHost == "/" ? "%2F" : this.vHost;
      set => this.vHost = value;
    }

    public string ToString(string format, IFormatProvider formatProvider) {
      return $"amqp://{this.User}:{this.Password}@{this.Host}:{this.Port}/{this.VirtualHost}";
    }
  }
}