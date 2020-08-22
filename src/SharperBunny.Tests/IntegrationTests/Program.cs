namespace SharperBunny.Tests.IntergrationTests {
  using System;

  internal class Program {
    private static void Main(string[] args) {
      var pipe = Bunny.ConnectSingleWith();
      IBunny bunny = pipe.Connect();

      Console.ReadLine();
    }
  }
}