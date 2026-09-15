using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostActionsController : ControllerBase
{
    private readonly NotificationDbContext _dbContext;

    public PostActionsController(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _dbContext.PostExecutionActions.Include(a => a.DataSource).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _dbContext.PostExecutionActions.Include(a => a.DataSource).FirstOrDefaultAsync(a => a.Id == id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PostExecutionAction action)
    {
        _dbContext.PostExecutionActions.Add(action);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = action.Id }, action);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PostExecutionAction updated)
    {
        var item = await _dbContext.PostExecutionActions.FindAsync(id);
        if (item == null) return NotFound();

        item.Name = updated.Name;
        item.Description = updated.Description;
        item.DataSourceId = updated.DataSourceId;
        item.ActionType = updated.ActionType;
        item.SqlQuery = updated.SqlQuery;

        await _dbContext.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _dbContext.PostExecutionActions.FindAsync(id);
        if (item == null) return NotFound();

        _dbContext.PostExecutionActions.Remove(item);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
