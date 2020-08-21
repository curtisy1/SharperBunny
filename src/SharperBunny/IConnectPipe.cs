using System;

namespace SharperBunny {
    //  IBunny bunny = Bunny.Connect(uri / Parameters / Fluent);
    //                 || Bunny.Connect().ToHost().ToPort().ToVirtualHost()
    //                    .WithPlain(guest, guest)
    ///<summary>
    /// Use to configure the Connection parameters in a fluent manner. Nothing configured evaluates to 
    /// default values. Supports only plain auth.
    ///</summary>
    public interface IConnectPipe : IFormattable {
        IConnectPipe ToHost (string hostName = "localhost");
        IConnectPipe ToPort (uint port = 5672);
        IConnectPipe ToVirtualHost (string vHost = "/");
        IConnectPipe AuthenticatePlain (string user = "guest", string password = "guest");

        IBunny Connect ();

    }
}