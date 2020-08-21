using System;
using System.Linq;
using System.Threading.Tasks;
using BunnyTests;
using SharperBunny;
using Xunit;

namespace SharperBunny.Tests.IntegrationTests.Declaration {
    public class DeleteTests {
        [Fact]
        public async Task DeclareAndDeleteQueueNotExistsAfterWards () {
            IBunny bunny = Bunny.ConnectSingle (ConnectSimple.BasicAmqp);
            IQueue declare = bunny.Setup ()
                .Queue ("to-delete");

            await declare.DeclareAsync ();
            bool exists = await declare.QueueExistsAsync (declare.Name);
            Assert.True (exists);
            await declare.DeleteQueueAsync (declare.Name);
            exists = await declare.QueueExistsAsync (declare.Name);
            Assert.False (exists);
        }

        [Fact]
        public async Task DeclareAndDeleteExchangeNotExistsAfterWards () {
            IBunny bunny = Bunny.ConnectSingle (ConnectSimple.BasicAmqp);
            IExchange declare = bunny.Setup ()
                .Exchange ("to-delete-ex", "fanout");

            await declare.DeclareAsync ();
            bool exists = await declare.ExchangeExistsAsync (declare.Name);
            Assert.True (exists);
            await declare.DeleteExchangeAsync (declare.Name);
            exists = await declare.ExchangeExistsAsync (declare.Name);
            Assert.False (exists);
        }
    }
}