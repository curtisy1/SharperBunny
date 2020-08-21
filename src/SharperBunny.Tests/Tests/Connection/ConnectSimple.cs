using SharperBunny;
using Xunit;

namespace SharperBunny.Tests.IntegrationTests.Connection {
    public class ConnectSimple {
        private static readonly string _virtualHost = "unittests";

        internal static IBunny Connect () => Bunny.ConnectSingle (BasicAmqp);
        internal static string BasicAmqp => $"amqp://guest:guest@localhost:5672/{_virtualHost}";

        [Fact]
        public void ConnectToSingleNodeViaAmqp () {
            IBunny bunny = Bunny.ConnectSingle (BasicAmqp);
            Assert.NotNull (bunny);
        }

        [Fact]
        public void ConnectToSingleNodeWithConnectionPipe () {
            var pipe = Bunny.ConnectSingleWith ();
            // not configuring anything uses default
            IBunny bunny = pipe.Connect ();

            Assert.NotNull (bunny);
        }

        [Fact]
        public void ConfigurePipeWorks () {
            var pipe = Bunny.ConnectSingleWith ();
            pipe.ToHost ("localhost")
                .ToVirtualHost ("unittests")
                .ToPort (5672)
                .AuthenticatePlain ("guest", "guest");

            IBunny bunny = pipe.Connect ();

            Assert.NotNull (bunny);
        }

        [Fact]
        public void ConnectMultipleFailsFirstConnectsSecond () {
            string node1 = $"amqp://guest:guest@localhost:5673/{_virtualHost}";
            string node2 = $"amqp://guest:guest@localhost:5672/{_virtualHost}";

            var multi = Bunny.ClusterConnect ();
            multi.AddNode (node1);
            multi.AddNode (node2);

            IBunny bunny = multi.Connect ();

            Assert.NotNull (bunny);
        }

        [Fact]
        public void ReconnectWithCluster () {
            // TODO
            // start cluster
            // connect with all nodes
            // remove one node
            // still connected (reconnected)
            //  --> to other node --> check channel, connection etc.
        }
    }
}