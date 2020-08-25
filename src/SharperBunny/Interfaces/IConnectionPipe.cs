namespace SharperBunny.Interfaces {
  using System;

  //  IBunny bunny = Bunny.Connect(uri / Parameters / Fluent);
  //                 || Bunny.Connect().ToHost().ToPort().ToVirtualHost()
  //                    .WithPlain(guest, guest)
  /// <summary>
  ///   Use to configure the Connection parameters in a fluent manner. Nothing configured evaluates to
  ///   default values. Supports only plain auth.
  /// </summary>
  public interface IConnectionPipe : IFormattable {
    IConnectionPipe ToHost(string hostName = "localhost");

    IConnectionPipe ToPort(uint port = 5672);

    IConnectionPipe ToVirtualHost(string vHost = "/");

    IConnectionPipe AuthenticatePlain(string user = "guest", string password = "guest");

    IConnectionPipe WithRetries(int retries, int timeout);

    IConnectionPipe UseAsyncEvents(bool useAsync = true);

    IBunny Connect();
  }
}