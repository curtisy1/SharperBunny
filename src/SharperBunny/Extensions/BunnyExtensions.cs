namespace SharperBunny.Extensions {
  using System;
  using RabbitMQ.Client;
  using RabbitMQ.Client.Exceptions;
  using SharperBunny.Connection;
  using SharperBunny.Consume;
  using SharperBunny.Declare;
  using SharperBunny.Exceptions;
  using SharperBunny.Interfaces;
  using SharperBunny.Publish;

  public static class BunnyExtensions {
    /// <summary>
    ///   Create a Publisher Builder interface. Can Also be used to publish messages.
    /// </summary>
    public static IPublish<TMsg> Publisher<TMsg>(this IBunny bunny, string publishToExchange)
      where TMsg : class {
      return new DeclarePublisher<TMsg>(bunny, publishToExchange);
    }

    /// <summary>
    ///   Create a Consumer to subscribe to a Queue. If no queue is specified the Queue Name will be AssemblyName.TypeName
    /// </summary>
    public static IConsumer<TMsg> Consumer<TMsg>(this IBunny bunny, string fromQueue = null) {
      fromQueue ??= SerializeTypeName<TMsg>();

      return new Consumer<TMsg>(bunny, fromQueue);
    }

    /// <summary>
    ///   Create a AsyncConsumer to subscribe to a Queue. If no queue is specified the Queue Name will be AssemblyName.TypeName
    /// </summary>
    public static IAsyncConsumer<TMsg> AsyncConsumer<TMsg>(this IBunny bunny, string fromQueue = null) {
      fromQueue ??= SerializeTypeName<TMsg>();

      return new AsyncConsumer<TMsg>(bunny, fromQueue);
    }

    /// <summary>
    ///   Create a Requester to send Rpc Requests. If no routingKey is specified the routingKey will be AssemblyName.TypeName
    /// </summary>
    public static IRequest<TRequest, TResponse> Request<TRequest, TResponse>(this IBunny bunny, string rpcExchange, string routingKey = null)
      where TRequest : class
      where TResponse : class {
      routingKey ??= SerializeTypeName<TRequest>();

      return new DeclareRequest<TRequest, TResponse>(bunny, rpcExchange, routingKey);
    }

    /// <summary>
    ///   Other side of the Rpc Call. Consumes fromQueue. If not Specified does consume from AssemblyName.TypeName
    /// </summary>
    public static IRespond<TRequest, TResponse> Respond<TRequest, TResponse>(this IBunny bunny, string rpcExchange, Func<TRequest, TResponse> respond, string fromQueue = null)
      where TRequest : class
      where TResponse : class {
      fromQueue ??= SerializeTypeName<TRequest>();

      return new DeclareResponder<TRequest, TResponse>(bunny, rpcExchange, fromQueue, respond);
    }

    /// <summary>
    ///   Interface for building Queues, Exchanges, Bindings and so on
    /// </summary>
    public static IDeclare Setup(this IBunny bunny) {
      return new DeclareBase { Bunny = bunny };
    }

    internal static bool QueueExists(this IBunny bunny, string name) {
      IModel channel = null;
      try {
        channel = bunny.Channel(true);
        channel.QueueDeclarePassive(name);

        return true;
      } catch (OperationInterruptedException) {
        return false;
      } catch (Exception ex) {
        throw DeclarationException.DeclareFailed(ex);
      } finally {
        channel?.Close();
      }
    }

    internal static bool ExchangeExists(this IBunny bunny, string name) {
      IModel channel = null;
      try {
        channel = bunny.Channel(true);
        channel.ExchangeDeclarePassive(name);

        return true;
      } catch (OperationInterruptedException) {
        return false;
      } catch (Exception ex) {
        throw DeclarationException.DeclareFailed(ex);
      } finally {
        channel?.Close();
      }
    }

    internal static PermanentChannel ToPermanentChannel(this IBunny bunny) {
      return new PermanentChannel(bunny);
    }

    private static string SerializeTypeName<T>() {
      return SerializeTypeName(typeof(T));
    }

    private static string SerializeTypeName(Type t) {
      return $"{t.Assembly.GetName().Name}.{t.Name}";
    }
  }
}