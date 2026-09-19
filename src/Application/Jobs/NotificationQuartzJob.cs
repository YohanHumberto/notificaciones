using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Domain.Interfaces;
using Quartz;

namespace NotificationService.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class NotificationQuartzJob : IJob
{
    private readonly INotificationJobProcessor _jobProcessor;
    private readonly ILogger<NotificationQuartzJob> _logger;

    public NotificationQuartzJob(INotificationJobProcessor jobProcessor, ILogger<NotificationQuartzJob> logger)
    {
        _jobProcessor = jobProcessor;
        _logger = logger;
    }

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var jobId = context.JobDetail.JobDataMap.GetInt("NotificationJobId");
        _logger.LogInformation("Ejecutando Job de Notificación #{JobId} desde Quartz.NET...", jobId);

        try
        {
            var log = await _jobProcessor.ProcessJobAsync(jobId);
            _logger.LogInformation("Job #{JobId} finalizado con estado {Status}. Duración: {Duration}ms", jobId, log.Status, log.DurationMs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción durante la ejecución del Job #{JobId} en Quartz.NET", jobId);
            throw new JobExecutionException(ex);
        }
    }
}
