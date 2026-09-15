using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Enums;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly NotificationDbContext _dbContext;

    public LogsController(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 30)
    {
        var total = await _dbContext.ExecutionLogs.CountAsync();
        var logs = await _dbContext.ExecutionLogs
            .OrderByDescending(l => l.TriggeredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { total, page, pageSize, logs });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLogById(int id)
    {
        var log = await _dbContext.ExecutionLogs.FindAsync(id);
        if (log == null) return NotFound();
        return Ok(log);
    }
}

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly NotificationDbContext _dbContext;

    public DashboardController(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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
