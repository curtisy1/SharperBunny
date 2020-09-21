namespace SharperBunny {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using RabbitMQ.Client;
  using RabbitMQ.Client.Exceptions;
  using SharperBunny.Connection;
  using SharperBunny.Consume;
  using SharperBunny.Declare;
  using SharperBunny.Exceptions;
  using SharperBunny.Facade;
  using SharperBunny.Interfaces;
  using SharperBunny.Publish;

  public static class Bunny {
    internal static readonly IList<string> Endpoints = new List<string>();
    internal static bool UseAsyncEvents { get; set; }
    internal static int RetryCount { get; set; } = 3;
    internal static int RetryPauseInMs { get; set; } = 1500;

    /// <summary>
    ///   Create a permanent connection by using parameters.
    /// </summary>
    public static IBunny ConnectSingle(ConnectionParameters parameters, bool useAsync = false) {
      Endpoints.Clear();
      Endpoints.Add(parameters.ToString());
      UseAsyncEvents = useAsync;
      return Connect();
    }

    /// <summary>
    ///   Create a permanent connection by using an amqp_uri.
    /// </summary>
    public static IBunny ConnectSingle(string amqpUri, bool useAsync = false) {
      Endpoints.Clear();
      Endpoints.Add(amqpUri);
      UseAsyncEvents = useAsync;
      return Connect();
    }

    /// <summary>
    ///   Connect with fluent interface
    /// </summary>
    public static IConnectionPipe ConnectSingleWith() {
      return new ConnectionPipe();
    }

    /// <summary>
    ///   Connect to a cluster with a builder interface
    /// </summary>
    public static IConnectionCluster ClusterConnect() {
      return new ConnectionCluster();
    }
    
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
      return new DeclareBase(bunny);
    }

    internal static IBunny Connect() {
      var factory = new ConnectionFactory {
        DispatchConsumersAsync = UseAsyncEvents,
        Uri = new Uri(Endpoints.FirstOrDefault() ?? "amqp://guest:guest@localhost:5672"),
      };
      var count = 0;
      
      while (count <= RetryCount) {
        try {
          return new MultiBunny(factory);
        } catch {
          count++;
          Thread.Sleep(RetryPauseInMs);
        }
      }

      throw new BrokerUnreachableException(new InvalidOperationException($"Broker not reachable at {Endpoints.FirstOrDefault()}"));
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