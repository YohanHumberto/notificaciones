using Application.Channels;
using Application.Configuration;
using Application.Evaluators;
using Application.Services;
using Domain.Contracts.Repository;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Security;
using NotificationService.Infrastructure.Templates;
using Persistence;
using Quartz;
using Repository.Repositories;
using Service.DataConnectors;
using Service.Jobs;
using Service.PostActions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NotificationContext>(options =>
{
	var connectionString = builder.Configuration.GetConnectionString("NotificationService")
		?? "Data Source=notification_service.db";
	options.UseSqlite(connectionString);
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<ISecretProtector, AesSecretProtector>();


builder.Services.AddScoped<IConditionalRuleRepository, ConditionalRuleRepository>();
builder.Services.AddScoped<IDataSourceConfigRepository, DataSourceConfigRepository>();
builder.Services.AddScoped<IExecutionLogRepository, ExecutionLogRepository>();
builder.Services.AddScoped<INotificationChannelRepository, NotificationChannelRepository>();
builder.Services.AddScoped<INotificationChannelSettingRepository, NotificationChannelSettingRepository>();
builder.Services.AddScoped<INotificationChannelSettingValueRepository, NotificationChannelSettingValueRepository>();
builder.Services.AddScoped<INotificationChannelTypeRepository, NotificationChannelTypeRepository>();
builder.Services.AddScoped<INotificationJobRepository, NotificationJobRepository>();
builder.Services.AddScoped<INotificationSettingDefinitionRepository, NotificationSettingDefinitionRepository>();
builder.Services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
builder.Services.AddScoped<IPostExecutionActionRepository, PostExecutionActionRepository>();

builder.Services.AddScoped<ConditionalRuleService, ConditionalRuleService>();
builder.Services.AddScoped<DataSourceConfigService, DataSourceConfigService>();
builder.Services.AddScoped<ExecutionLogService, ExecutionLogService>();
builder.Services.AddScoped<NotificationChannelService, NotificationChannelService>();
builder.Services.AddScoped<NotificationChannelSettingService, NotificationChannelSettingService>();
builder.Services.AddScoped<NotificationChannelSettingValueService, NotificationChannelSettingValueService>();
builder.Services.AddScoped<NotificationChannelTypeService, NotificationChannelTypeService>();
builder.Services.AddScoped<NotificationJobService, NotificationJobService>();
builder.Services.AddScoped<NotificationSettingDefinitionService, NotificationSettingDefinitionService>();
builder.Services.AddScoped<NotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<PostExecutionActionService, PostExecutionActionService>();

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
// Ensure Database is created and Seeded
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<NotificationContext>();
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
