using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Channels;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.DataConnectors;
using NotificationService.Infrastructure.Evaluators;
using NotificationService.Infrastructure.Jobs;
using NotificationService.Infrastructure.PostActions;
using NotificationService.Infrastructure.Security;
using NotificationService.Infrastructure.Templates;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("NotificationService")
        ?? "Data Source=notification_service.db";
    options.UseSqlite(connectionString);
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<ISecretProtector, AesSecretProtector>();
builder.Services.AddScoped<INotificationChannelConfigurationService, NotificationChannelConfigurationService>();

builder.Services.AddSingleton<ITemplateRenderer, FluidTemplateRenderer>();
builder.Services.AddScoped<IDataSourceService, DataSourceService>();
builder.Services.AddScoped<IConditionEvaluator, ConditionEvaluator>();
builder.Services.AddScoped<IPostExecutionProcessor, PostExecutionProcessor>();
builder.Services.AddScoped<INotificationJobProcessor, NotificationJobProcessor>();
builder.Services.AddScoped<ISchedulerService, QuartzSchedulerService>();

builder.Services.AddScoped<INotificationChannelProvider, EmailChannelProvider>();
builder.Services.AddScoped<INotificationChannelProvider, WebhookChannelProvider>();

// Register Quartz.NET
builder.Services.AddQuartz(q =>
{
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore();
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddControllers();

var app = builder.Build();

// Ensure Database is created and Seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.MigrateAsync();

    // Sync any pre-existing active jobs with Quartz.NET scheduler
    var schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
    var activeJobs = await db.Jobs.ToListAsync();
    foreach (var job in activeJobs)
    {
        await schedulerService.SyncJobScheduleAsync(job);
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.MapControllers();

app.Run();
