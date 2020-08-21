using System;
using SharperBunny;

namespace SharperBunny.Tests.IntegrationTests {
    class Program {
        static void Main (string[] args) {
            var pipe = Bunny.ConnectSingleWith ();
            IBunny bunny = pipe.Connect ();

            Console.ReadLine ();
        }
    }
}