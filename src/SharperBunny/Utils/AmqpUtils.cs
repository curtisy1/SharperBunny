namespace SharperBunny.Utils {
  using System;
  using RabbitMQ.Client;

  public static class AmqpUtils {
    public static AmqpTcpEndpoint ParseEndpoint(this string amqpUri) {
      return new AmqpTcpEndpoint(new Uri(amqpUri));
    }
  }
}