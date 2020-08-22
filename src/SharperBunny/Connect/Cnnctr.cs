namespace SharperBunny.Connect {
  using System.Collections.Generic;
  using RabbitMQ.Client;
  using SharperBunny.Facade;
  using SharperBunny.Interfaces;
  using SharperBunny.Utils;

  public class Cnnctr : IConnector {
    private readonly IList<AmqpTcpEndpoint> _endpoints = new List<AmqpTcpEndpoint>();

    public IConnector AddNode(string amqp_uri) {
      this._endpoints.Add(amqp_uri.ParseEndpoint());
      return this;
    }

    public IConnector AddNode(IConnectPipe pipe) {
      var amqp = pipe.ToString("amqp", null);
      this._endpoints.Add(amqp.ParseEndpoint());
      return this;
    }

    public IConnector AddNode(ConnectionParameters pipe) {
      var amqp = pipe.ToString("amqp", null);
      this._endpoints.Add(amqp.ParseEndpoint());
      return this;
    }

    public IBunny Connect() {
      var factory = new ConnectionFactory();

      return new MultiBunny(factory, this._endpoints);
    }

    public IConnector WithRetry(int retry = 5, int timeout = 2) {
      return this;
    }
  }
}