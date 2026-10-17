using Microsoft.Extensions.Hosting;

namespace SqlLogSync;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseWindowsService().ConfigureServices((hostContext, services) => {
                
            });

    }
}
