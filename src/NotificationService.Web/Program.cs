using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using NotificationService.Extensions;
using Persistence;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

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

builder.Services.ConfigureAutoMapper();
builder.Services.AddServices();
builder.Services.ConfigureDbContext(builder.Configuration);

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
