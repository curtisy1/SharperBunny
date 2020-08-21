using System;
using System.Threading;
using System.Threading.Tasks;
using BunnyTests;
using RabbitMQ.Client.Events;
using SharperBunny;
using Xunit;

namespace SharperBunny.Tests.IntegrationTests.Producer {
    public class PublisherTests {
        private string Exchange = ""; // default --> " "

        [Fact]
        public async Task PublisherSimplySendsWithoutQueueReturnsFailure () {
            IBunny bunny = Bunny.ConnectSingle (ConnectSimple.BasicAmqp);
            IPublish<TestMessage> publisher = bunny.Publisher<TestMessage> (Exchange);

            OperationResult<TestMessage> result = await publisher.SendAsync (new TestMessage ());

            Assert.True (result.IsSuccess);
            bunny.Dispose ();
        }

        [Fact]
        public async Task PublisherSimplySendsWitQueueReturnsSuccess () {
            IBunny bunny = Bunny.ConnectSingle (ConnectSimple.BasicAmqp);
            IPublish<TestMessage> publisher = bunny.Publisher<TestMessage> (Exchange);

            OperationResult<TestMessage> result = await publisher
                .WithQueueDeclare ()
                .SendAsync (new TestMessage ());

            bool success = await bunny.Setup ().DeleteQueueAsync (typeof (TestMessage).FullName, force : true);

            Assert.True (result.IsSuccess);
            Assert.True (success);
            bunny.Dispose ();
        }

        [Fact]
        public async Task ForceCreatesTheExchangeIfNotExists () {
            IBunny bunny = Bunny.ConnectSingle (ConnectSimple.BasicAmqp);
            IPublish<TestMessage> publisher = bunny.Publisher<TestMessage> ("test-exchange");

            OperationResult<TestMessage> result = await publisher.SendAsync (new TestMessage (), force : true);

            Assert.True (result.IsSuccess);
            bool removed_exchange = await bunny.Setup ().DeleteExchangeAsync ("test-exchange", force : true);
            Assert.True (removed_exchange);
            bunny.Dispose ();
        }

        [Fact]
        public async Task ConfirmsAndAcksWork () {
            IBunny bunny = Bunny.ConnectSingle (ConnectSimple.BasicAmqp);
            IQueue queue = bunny.Setup ()
                .Queue ("constraint")
                .MaxLength (1)
                .QueueExpiry (1500)
                .Bind ("amq.direct", "constraint-key")
                .OverflowReject ();

            bool isNacked = false;
            bool isAcked = false;
            var publisher = bunny.Publisher<TestMessage> ("amq.direct");
            Func<BasicNackEventArgs, Task> nacker = ea => { isNacked = true; return Task.CompletedTask; };
            Func<BasicAckEventArgs, Task> acker = ea => { isAcked = true; return Task.CompletedTask; };

            await publisher.WithQueueDeclare (queue)
                .WithConfirm (acker, nacker)
                .WithRoutingKey ("constraint-key")
                .SendAsync (new TestMessage () { Text = "Confirm-1st" });

            await publisher.WithQueueDeclare (queue)
                .WithConfirm (acker, nacker)
                .SendAsync (new TestMessage () { Text = "Confirm-2nd" });

            Assert.True (isAcked);
            Assert.True (isNacked);
            bunny.Dispose ();
        }

        [Fact]
        public async Task MandatoryFailsWithoutQueue () {
            IBunny bunny = Bunny.ConnectSingle (ConnectSimple.BasicAmqp);

            bool isReturned = false;
            var publisher = bunny.Publisher<TestMessage> ("amq.direct");
            Func<BasicReturnEventArgs, Task> nacker = ea => { isReturned = true; return Task.CompletedTask; };

            await publisher.AsMandatory (nacker)
                .WithRoutingKey ("not-any-bound-queue")
                .SendAsync (new TestMessage ());

            await Task.Delay (500);
            Assert.True (isReturned);
            bunny.Dispose ();
        }

        [Fact]
        public async Task MandatoryWorksWithQueue () {
            IBunny bunny = Bunny.ConnectSingle (ConnectSimple.BasicAmqp);

            bool isReturned = true;
            var publisher = bunny.Publisher<TestMessage> ("amq.direct");
            Func<BasicReturnEventArgs, Task> nacker = ea => { isReturned = false; return Task.CompletedTask; };

            await publisher.AsMandatory (nacker)
                .WithQueueDeclare ()
                .SendAsync (new TestMessage () { Text = "Mandatory-succeeds" });

            bool removed = await bunny.Setup ().DeleteQueueAsync (typeof (TestMessage).FullName, force : true);

            Assert.True (isReturned);
            bunny.Dispose ();
        }

        [Fact]
        public async Task MultiplePublishOnSinglePublisher () {
            IBunny bunny = Bunny.ConnectSingle (ConnectSimple.BasicAmqp);
            var publisher = bunny.Publisher<TestMessage> ("amq.direct");

            string queueName = "polymorph-queue";
            await publisher.WithQueueDeclare (queueName, "poly")
                .SendAsync (new OtherMessage ());
            await publisher.SendAsync (new YetAnotherMessage ());
            await Task.Delay (150);
            uint count = bunny.Channel ().MessageCount (queueName);

            Assert.Equal (2, (int) count);
            await bunny.Setup ().DeleteQueueAsync (queueName, force : true);
        }

        [Fact]
        public async Task OverWriteRoutingKeySendsToDifferentQueuesEachTime () {
            IBunny bunny = Bunny.ConnectSingle (ConnectSimple.BasicAmqp);
            var publisher = bunny.Publisher<TestMessage> ("amq.direct");

            string queueName = "polymorph-queue-other";
            string queueNameYetOther = "polymorph-queue-yet-another";
            await publisher.WithQueueDeclare (queueName, "poly")
                .SendAsync (new OtherMessage ());
            await publisher.WithQueueDeclare (queueNameYetOther, "poly-2")
                .WithRoutingKey ("poly-2")
                .SendAsync (new YetAnotherMessage ());

            uint otherCount = bunny.Channel ().MessageCount (queueName);
            uint yetOtherCount = bunny.Channel ().MessageCount (queueNameYetOther);

            Assert.Equal (1, (int) otherCount);
            Assert.Equal (1, (int) yetOtherCount);

            await bunny.Setup ().DeleteQueueAsync (queueName, force : true);
            await bunny.Setup ().DeleteQueueAsync (queueNameYetOther, force : true);
        }

        public class TestMessage {
            public string Text { get; set; } = "Test";
            public int Number { get; set; } = 42;
        }

        public class OtherMessage : TestMessage { }
        public class YetAnotherMessage : OtherMessage { }
    }
}