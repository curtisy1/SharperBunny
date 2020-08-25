namespace SharperBunny.Tests.IntegrationTests {
  using System;

  internal static class Program {
    private static void Main(string[] args) {
      var pipe = Bunny.ConnectSingleWith();
      var bunny = pipe.Connect();

      Console.ReadLine();
    }
  }
}