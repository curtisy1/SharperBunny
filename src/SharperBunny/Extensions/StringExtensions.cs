namespace SharperBunny.Extensions {
  using System;
  using RabbitMQ.Client;

  public static class StringExtensions {
    public static AmqpTcpEndpoint ParseEndpoint(this string amqpUri) {
      return new AmqpTcpEndpoint(new Uri(amqpUri));
    }
  }
}