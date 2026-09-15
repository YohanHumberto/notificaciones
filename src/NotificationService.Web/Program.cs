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
using NotificationService.Infrastructure.Templates;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core DbContext with SQLite (with easy path to SQL Server)
builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    options.UseSqlite("Data Source=notification_service.db");
});

builder.Services.AddHttpClient();

// Register Core Domain & Infrastructure Services
builder.Services.AddSingleton<ITemplateRenderer, FluidTemplateRenderer>();
builder.Services.AddScoped<IDataSourceService, DataSourceService>();
builder.Services.AddScoped<IConditionEvaluator, ConditionEvaluator>();
builder.Services.AddScoped<IPostExecutionProcessor, PostExecutionProcessor>();
builder.Services.AddScoped<INotificationJobProcessor, NotificationJobProcessor>();
builder.Services.AddScoped<ISchedulerService, QuartzSchedulerService>();

// Register Channel Providers
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
    await db.Database.EnsureCreatedAsync();

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
