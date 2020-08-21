using System;

namespace SharperBunny.Connect {
    ///<summary>
    /// Specify Connectionparameters by using Properties.
    /// Virtualhost defaults to '/'
    ///</summary>
    public class ConnectionParameters : IFormattable {
        public string Host { get; set; }
        public uint Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        private string vHost;
        public string VirtualHost {
            get {
                return vHost == "/" ? "%2F" : vHost;
            }
            set {
                vHost = value;
            }
        }

        public string ToString (string format, IFormatProvider formatProvider) {

            return $"amqp://{User}:{Password}@{Host}:{Port}/{VirtualHost}";
        }
    }
}