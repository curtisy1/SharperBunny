using System;
using System.Threading.Tasks;
using SharperBunny;
using SharperBunny.Declare;
using SharperBunny.Exceptions;
using Xunit;

namespace SharperBunny.Tests.IntegrationTests.Declaration {
    public class DeclareTests {
        [Fact]
        public void QueueCallThrowsIfNotDeclareBase () {
            var fail = new PurposeIsFail ();
            Assert.Throws<DeclarationException> (() => fail.Queue ("name"));
        }
        private class PurposeIsFail : IDeclare {
            public IBunny Bunny =>
                throw new NotImplementedException ();

            public Task DeclareAsync () {
                throw new System.NotImplementedException ();
            }
        }

        [Fact]
        public void QueueCallReturnsTypeQueueDeclareIfIsBase () {
            var @base = new DeclareBase ();
            var queue = @base.Queue ("my-queue");
            Assert.Equal (typeof (DeclareQueue), queue.GetType ());
        }

        [Theory]
        [InlineData ("")]
        [InlineData ("   ")]
        [InlineData (null)]
        [InlineData ("255")]
        public void ThrowsOnNameNullOrToLong (string name) {
            if (name == "255") {
                name = name.PadRight (500, '-');
            }
            var @base = new DeclareBase ();
            Assert.Throws<DeclarationException> (() => @base.Queue (name));
        }

        [Theory]
        [InlineData ("")]
        [InlineData ("   ")]
        [InlineData (null)]
        [InlineData ("255")]
        public void CheckBindingDeclare (string name) {
            if (name == "255") {
                name = name.PadRight (500, '-');
            }
            var @base = new DeclareQueue (ConnectSimple.Connect (), "queue");
            Assert.Throws<DeclarationException> (() => @base.Bind (name, ""));
        }

        [Fact]
        public void BindAsSetsBindingKeyOn () {
            var @base = new DeclareQueue (ConnectSimple.Connect (), "queue");
            @base.Bind ("ex", "bind-key");
            Assert.Equal ("bind-key", @base.BindingKey.HasValue ? @base.BindingKey.Value.rKey : "null");
        }

        [Fact]
        public async Task DeclareAndBindDefaultAmqDirectSucceeds () {
            IBunny bunny = Bunny.ConnectSingle (ConnectSimple.BasicAmqp);
            var declare = bunny.Setup ()
                .Queue ("bind-test")
                .Bind ("amq.direct", "bind-test-key")
                .AsDurable ()
                .QueueExpiry (1500)
                .WithTTL (500)
                .MaxLength (10);

            await declare.DeclareAsync ();

            Assert.Equal (1, 1);

        }
    }
}