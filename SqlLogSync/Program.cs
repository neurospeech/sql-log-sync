using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlLogSync.Tasks;
using System;
using System.IO;

namespace SqlLogSync;

public class Program
{
    public static void Main(string[] args)
    {
        string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectory);
        string logPath = Path.Combine(logDirectory, "console.log");

        // 2. Open an un-buffered stream writer (Append mode = true)
        var streamWriter = new StreamWriter(logPath, append: true) { AutoFlush = true };

        // 3. Wrap it using the built-in .NET thread-safe wrapper
        var threadSafeWriter = TextWriter.Synchronized(streamWriter);

        // 4. Redirect
        Console.SetOut(threadSafeWriter);
        Console.SetError(threadSafeWriter);

        DotNetEnv.Env.Load();
        DotNetEnv.Env.TraversePath().Load();
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
