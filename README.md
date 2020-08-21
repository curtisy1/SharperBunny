# SharperBunny
Not the RabbitMQ Library we deserve, but the one that we need 

This library assumes you have at least some knowledge about how RabbitMQ works. 

As opposed to other frameworks it DOES NOT try to abstract away the details of using RabbitMQ properly.
But on the other hand focuses on a more fluent and readable API for the different message flows and patterns by using standard C# APIs.

If you need to learn more about RabbitMQ in general please take a look at my udemy course:
https://www.udemy.com/course/rabbitmq-from-start-to-finish/?referralCode=AC4E4E5264FFDC3B1A9C


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
IQueue queue = bunny.Setup().Queue();
IExchange exchange = bunny.Setup().Exchange();
```
Those are Builder interfaces and allow to specify all properties that those Entities have in place.

You can also remove some of those entities or check if they exist
```csharp
bool deleted = await bunny.Setup().DeleteQueueAsync("queue-name");
bool exists = await bunny.Setup().QueueExistsAsync("queue-name");
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
 OperationResult<TestMessage> result = await publisher.WithQueueDeclare()
                                                      .AsMandatory() // make sure the message is routed
                                                      .AsPersistent() // deliveryMode=2
                                                      .SendAsync(new TestMessage());
 Console.WriteLine(result.IsSuccess);
 // remove the Queue again
 bool success = await bunny.Setup().DeleteQueueAsync(typeof(TestMessage).FullName, force: true);

 bunny.Dispose();
```
The publisher can be used to publish several messages. With AsUniqueChannel you can also utilize a new Channel each time.

### Declare a simple Queue bound to the amq.direct exchange
```csharp
 IBunny bunny = Bunny.ConnectSingle("amqp://guest:guest@localhost/%2F");
 IQueue declare = bunny.Setup()
                    .Queue("bind-test")
                    .Bind("amq.direct", "bind-test-key")
                    .AsDurable()
                    .QueueExpiry(1500)
                    .WithTTL(500)
                    .MaxLength(10);

 await declare.DeclareAsync();
```
The IQueue result can be supplied to other builder interfaces. It represents the declared Queue. After the DeclareAsync, the Queue Properties cannot be changed anymore.


Enjoy distributing your application
