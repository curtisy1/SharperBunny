namespace SharperBunny.Utils {
  using System;
  using RabbitMQ.Client;

  public static class AmqpUtils {
    public static AmqpTcpEndpoint ParseEndpoint(this string amqp_uri) {
      return new AmqpTcpEndpoint(new Uri(amqp_uri));
    }
  }
}