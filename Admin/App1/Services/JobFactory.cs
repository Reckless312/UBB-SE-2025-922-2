using System;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

public class JobFactory : IJobFactory
{
    private readonly IServiceProvider serviceProvider;

    public JobFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        try
        {
            IJob? job = this.serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            return job == null ? throw new Exception("Couldn't retrieve the required service") : job;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Job creation failed: {ex}");
            throw;
        }
    }

    public void ReturnJob(IJob job)
    {
        (job as IDisposable)?.Dispose();
    }
}