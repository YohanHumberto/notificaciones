using Domain.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Entities;
using Persistence.Enums;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController(IExecutionLogService service) :
	BaseCrudController<ExecutionLog, ExecutionLog, ExecutionLog>(service)
{
}

[ApiController]
[Route("api/[controller]")]
public class DashboardController(NotificationContext dbContext) : ControllerBase
{
	private readonly NotificationContext _dbContext = dbContext;

	[HttpGet]
	public async Task<IActionResult> GetStats()
	{
		var totalJobs = await _dbContext.Jobs.CountAsync();
		var activeJobs = await _dbContext.Jobs.CountAsync(j => j.IsActive);
		var totalChannels = await _dbContext.Channels.CountAsync();
		var totalTemplates = await _dbContext.Templates.CountAsync();
		var totalDataSources = await _dbContext.DataSources.CountAsync();

		var totalLogs = await _dbContext.ExecutionLogs.CountAsync();
		var successLogs = await _dbContext.ExecutionLogs.CountAsync(l => l.Status == ExecutionStatus.Success);
		var skippedLogs = await _dbContext.ExecutionLogs.CountAsync(l => l.Status == ExecutionStatus.SkippedCondition);
		var failedLogs = await _dbContext.ExecutionLogs.CountAsync(l => l.Status == ExecutionStatus.Failed);

		var recentLogs = await _dbContext.ExecutionLogs
			.OrderByDescending(l => l.TriggeredAt)
			.Take(10)
			.ToListAsync();

		return Ok(new
		{
			totalJobs,
			activeJobs,
			totalChannels,
			totalTemplates,
			totalDataSources,
			totalLogs,
			successLogs,
			skippedLogs,
			failedLogs,
			recentLogs
		});
	}
}
