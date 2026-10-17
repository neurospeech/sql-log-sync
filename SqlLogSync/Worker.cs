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
            var task = DatabaseTask.Create(services);
            await task.RunAsync();

            await Task.Delay(TimeSpan.FromMinutes(15));
        }
    }
}
