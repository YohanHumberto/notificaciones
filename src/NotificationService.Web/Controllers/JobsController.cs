using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly NotificationDbContext _dbContext;
    private readonly ISchedulerService _schedulerService;
    private readonly INotificationJobProcessor _jobProcessor;

    public JobsController(
        NotificationDbContext dbContext,
        ISchedulerService schedulerService,
        INotificationJobProcessor jobProcessor)
    {
        _dbContext = dbContext;
        _schedulerService = schedulerService;
        _jobProcessor = jobProcessor;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _dbContext.Jobs
            .Include(j => j.Channel)
            .Include(j => j.Template)
            .Include(j => j.DataSource)
            .Include(j => j.ConditionalRule)
            .Include(j => j.PostExecutionAction)
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var job = await _dbContext.Jobs
            .Include(j => j.Channel)
            .Include(j => j.Template)
            .Include(j => j.DataSource)
            .Include(j => j.ConditionalRule)
            .Include(j => j.PostExecutionAction)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null) return NotFound();
        return Ok(job);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NotificationJob job)
    {
        _dbContext.Jobs.Add(job);
        await _dbContext.SaveChangesAsync();

        // Sync Quartz schedule
        await _schedulerService.SyncJobScheduleAsync(job);

        return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] NotificationJob updated)
    {
        var job = await _dbContext.Jobs.FindAsync(id);
        if (job == null) return NotFound();

        job.Name = updated.Name;
        job.Description = updated.Description;
        job.ScheduleType = updated.ScheduleType;
        job.CronExpression = updated.CronExpression;
        job.ScheduledAt = updated.ScheduledAt;
        job.ChannelId = updated.ChannelId;
        job.TemplateId = updated.TemplateId;
        job.DataSourceId = updated.DataSourceId;
        job.DataQuery = updated.DataQuery;
        job.RecipientExpression = updated.RecipientExpression;
        job.ConditionalRuleId = updated.ConditionalRuleId;
        job.PostExecutionActionId = updated.PostExecutionActionId;
        job.IsActive = updated.IsActive;

        await _dbContext.SaveChangesAsync();

        // Resync Quartz Schedule
        await _schedulerService.SyncJobScheduleAsync(job);

        return Ok(job);
    }

    [HttpPost("{id}/toggle")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var job = await _dbContext.Jobs.FindAsync(id);
        if (job == null) return NotFound();

        job.IsActive = !job.IsActive;
        await _dbContext.SaveChangesAsync();

        await _schedulerService.SyncJobScheduleAsync(job);
        return Ok(new { isActive = job.IsActive });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var job = await _dbContext.Jobs.FindAsync(id);
        if (job == null) return NotFound();

        _dbContext.Jobs.Remove(job);
        await _dbContext.SaveChangesAsync();

        await _schedulerService.RemoveJobScheduleAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/trigger")]
    public async Task<IActionResult> TriggerNow(int id)
    {
        var job = await _dbContext.Jobs.FindAsync(id);
        if (job == null) return NotFound();

        var log = await _jobProcessor.ProcessJobAsync(id);
        return Ok(log);
    }
}
