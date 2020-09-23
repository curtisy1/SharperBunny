# SharperBunny
## Foreword
This library is heavily inspired (as in basically an evolved fork) by [sharp-bunny](https://github.com/TimoHeiten/sharp-bunny)

Since I direly needed a Nuget package that provided a basic abstraction towards the official RabbitMQ.Client that still provided access to most internals if need be and the official repo does not yet offer a Nuget package, I decided to improve it. Although not yet complete, there's a few things that were added compared to the original:

- RabbitMQ.Client has been updated to 6.2.1. This should improve performance as well as reduce the memory footprint since everything is now using ReadOnlyMemory<byte> instead of byte[] (also see [this article](https://stebet.net/real-world-example-of-reducing-allocations-using-span-t-and-memory-t/) for a more in depth explanation)
- Users can choose between using EventingBasicConsumer or AsyncEventingBasicConsumer and their resulting POCOs, Carrot and AsyncCarrot respectively. This allows for cases where you want to go truly async and prepares for an upcoming 7.0 async version of the RabbitMQ.Client
- The DeliveryTag is publicly available, this is useful for when you want to implement custom Ack/NAck behaviour which can be specified as well
- Newtonsoft.Json was replaced with System.Text.Json. This is a design choice that hopefully makes everything a bit faster and more future-proof
- All fake async methods have been stripped off their async. No need for the extra overhead

## Description
This is copied from the original library for now. Most of these haven't changed much but there's some additional features that would do well to be documented
The current focus is on further improving the library though, enabling more use cases (e.g. multiple channels per connection), increasing test coverage and writing stress tests/real world examples
I'm planning to add to the documentation while going along with that but it might come a bit short. In general, the below should be enough to get you started


Not the RabbitMQ Library we deserve, but the one that we need 

This library assumes you have at least some knowledge about how RabbitMQ works. 

As opposed to other frameworks it DOES NOT try to abstract away the details of using RabbitMQ properly.
But on the other hand focuses on a more fluent and readable API for the different message flows and patterns by using standard C# APIs.


## Features:
* Connect Single, Amqp Uri, parameters and Cluster support
* Builder interfaces for all entities
  * Queues
  * Exchanges
  * Bindings
* Support for Message Pattern
  * RPC with temproary queue or Direct-Reply-to (on same channel)
  * Pub/Sub
* Builder interfaces for Subcribe lets you handle Acks, Nacks and BasicReturn
* Connection management, but also access to it

In general you have the following options (more in the documentation)

### Connect to the Broker with
```csharp
IBunny bunny = Bunny.ConnectSingle("amqp://guest:guest@localhost:5672/%2F");
// also available with parameters instead of amqp uri
```
or connect to a cluster
```csharp
IBunny bunny = Bunny.ClusterConnect().AddNode("amqp://guest:guest@localhost:5672/%2F")
                      .AddNode("amqp://guest:guest@localhost:5673/%2F")
                      .Connect();
```

### Builder interfaces
With a bunny (representing a single connection, that does encapsulate ConnectionRetry)
you can access the Declarations to create your RabbitMQ Entities:

```csharp
IQueue queue = bunny.Queue();
IExchange exchange = bunny.Exchange();
```
Those are Builder interfaces and allow to specify all properties that those Entities have in place.

You can also remove some of those entities or check if they exist
```csharp
bool deleted = bunny.Setup().DeleteQueue("queue-name");
bool exists = bunny.Setup().QueueExists("queue-name");
```

If you still need to access the underlying Channel (IModel of the RabbitMQ.Client):
```csharp
IModel channel = bunny.Channel();
```
You can also create a new channel by passing the newOne flag set to true
```csharp
IModel freshChannel = bunny.Channel(newOne:true);
```
If you need to access the underlying Connection use
```csharp
IConnection connection = bunny.Connection;
```

# Examples:
## Setup a simple Publisher
```csharp
 string defaultExchange = "";
 IBunny bunny = Bunny.ConnectSingle("amqp://guest:guest@localhost/%2F");
 IPublish<TestMessage> publisher = bunny.Publisher<TestMessage>(defaultExchange);

 // declares a queue dependent
 // on the Message Type bound to the amq.direct exchange
 OperationResult<TestMessage> result = publisher.WithQueueDeclare()
                                                      .AsMandatory() // make sure the message is routed
                                                      .AsPersistent() // deliveryMode=2
                                                      .Send(new TestMessage());
 Console.WriteLine(result.IsSuccess);
 // remove the Queue again
 bool success = bunny.Setup().DeleteQueue(typeof(TestMessage).FullName, force: true);

 bunny.Dispose();
```
The publisher can be used to publish several messages. With AsUniqueChannel you can also utilize a new Channel each time.

### Declare a simple Queue bound to the amq.direct exchange
```csharp
 IBunny bunny = Bunny.ConnectSingle("amqp://guest:guest@localhost/%2F");
 IQueue declare = bunny.Queue("bind-test")
                    .Bind("amq.direct", "bind-test-key")
                    .SetDurable()
                    .QueueExpiry(1500)
                    .WithTTL(500)
                    .MaxLength(10);

 declare.Declare();
```
The IQueue result can be supplied to other builder interfaces. It represents the declared Queue. After the Declare, the Queue Properties cannot be changed anymore.


Enjoy distributing your application
