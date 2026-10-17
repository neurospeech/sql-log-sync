using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlLogSync.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlLogSync;

internal class Worker : BackgroundService
{
    private readonly IServiceProvider services;

    public Worker(IServiceProvider services)
    {
        this.services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = services.CreateScope();
                var task = DatabaseTask.Create(scope.ServiceProvider);
                await task.RunAsync();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            await Task.Delay(TimeSpan.FromMinutes(15));
        }
    }
}
