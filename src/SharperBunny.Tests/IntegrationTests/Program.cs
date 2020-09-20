namespace SharperBunny.Tests.IntegrationTests {
  using System.Threading.Tasks;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;

  public static class Program {
    public static Task Main(string[] args) {
      return CreateHostBuilder(args).Build().RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) {
      return Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) => {
          services.AddSingleton(hostContext.Configuration);
          services.AddHostedService<BunnyWorker>();
          services.AddLogging();
        });
    }
  }
}