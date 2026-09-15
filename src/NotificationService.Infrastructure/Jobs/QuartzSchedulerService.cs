using System;
using System.Threading.Tasks;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Core.Interfaces;
using Quartz;

namespace NotificationService.Infrastructure.Jobs;

public class QuartzSchedulerService : ISchedulerService
{
    private readonly ISchedulerFactory _schedulerFactory;

    public QuartzSchedulerService(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public async Task SyncJobScheduleAsync(NotificationJob job)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobKey = new JobKey($"Job_{job.Id}", "NotificationJobs");

        var existingJob = await scheduler.GetJobDetail(jobKey);
        if (existingJob != null)
        {
            await scheduler.DeleteJob(jobKey);
        }

        if (!job.IsActive)
            return;

        var jobDetail = JobBuilder.Create<NotificationQuartzJob>()
            .WithIdentity(jobKey)
            .WithDescription(job.Name)
            .UsingJobData("NotificationJobId", job.Id)
            .Build();

        ITrigger trigger;

        if (job.ScheduleType == ScheduleType.CronRecurring && !string.IsNullOrWhiteSpace(job.CronExpression))
        {
            trigger = TriggerBuilder.Create()
                .WithIdentity($"Trigger_{job.Id}", "NotificationTriggers")
                .WithCronSchedule(job.CronExpression)
                .Build();
        }
        else if (job.ScheduleType == ScheduleType.OneTime && job.ScheduledAt.HasValue)
        {
            trigger = TriggerBuilder.Create()
                .WithIdentity($"Trigger_{job.Id}", "NotificationTriggers")
                .StartAt(new DateTimeOffset(job.ScheduledAt.Value))
                .Build();
        }
        else
        {
            // If active but no schedule configured, schedule for immediate trigger once
            trigger = TriggerBuilder.Create()
                .WithIdentity($"Trigger_{job.Id}", "NotificationTriggers")
                .StartNow()
                .Build();
        }

        await scheduler.ScheduleJob(jobDetail, trigger);
    }

    public async Task RemoveJobScheduleAsync(int jobId)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobKey = new JobKey($"Job_{jobId}", "NotificationJobs");
        var existingJob = await scheduler.GetJobDetail(jobKey);
        if (existingJob != null)
        {
            await scheduler.DeleteJob(jobKey);
        }
    }

    public async Task TriggerJobImmediatelyAsync(int jobId)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobKey = new JobKey($"Job_{jobId}", "NotificationJobs");
        var existingJob = await scheduler.GetJobDetail(jobKey);
        if (existingJob != null)
        {
            await scheduler.TriggerJob(jobKey);
        }
    }
}
