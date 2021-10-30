namespace Bet.Extensions.Resilience.WebApp.Sample
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseResilienceOnStartup();

                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
