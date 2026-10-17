using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlLogSync.Tasks;

namespace SqlLogSync;

public class Program
{
    public static void Main(string[] args)
    {
        DotNetEnv.Env.Load();
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseWindowsService().ConfigureServices((hostContext, services) => {
                services.AddHostedService<Worker>();
                services.AddScoped<DatabaseBackupTask>();
                services.AddScoped<DatabaseRestoreTask>();
            });

    }
}
