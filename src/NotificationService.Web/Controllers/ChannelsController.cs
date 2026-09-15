using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChannelsController : ControllerBase
{
    private readonly NotificationDbContext _dbContext;
    private readonly System.Collections.Generic.IEnumerable<INotificationChannelProvider> _providers;

    public ChannelsController(NotificationDbContext dbContext, System.Collections.Generic.IEnumerable<INotificationChannelProvider> providers)
    {
        _dbContext = dbContext;
        _providers = providers;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var channels = await _dbContext.Channels.ToListAsync();
        return Ok(channels);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var channel = await _dbContext.Channels.FindAsync(id);
        if (channel == null) return NotFound();
        return Ok(channel);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NotificationChannel channel)
    {
        _dbContext.Channels.Add(channel);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = channel.Id }, channel);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] NotificationChannel updated)
    {
        var channel = await _dbContext.Channels.FindAsync(id);
        if (channel == null) return NotFound();

        channel.Name = updated.Name;
        channel.Type = updated.Type;
        channel.ConfigJson = updated.ConfigJson;
        channel.IsActive = updated.IsActive;

        await _dbContext.SaveChangesAsync();
        return Ok(channel);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var channel = await _dbContext.Channels.FindAsync(id);
        if (channel == null) return NotFound();

        _dbContext.Channels.Remove(channel);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestChannel(int id, [FromBody] TestChannelRequest req)
    {
        var channel = await _dbContext.Channels.FindAsync(id);
        if (channel == null) return NotFound();

        var provider = System.Linq.Enumerable.FirstOrDefault(_providers, p => p.ChannelType == channel.Type);
        if (provider == null) return BadRequest("Proveedor no registrado para este canal.");

        var (success, message) = await provider.SendNotificationAsync(channel, req.Recipient, req.Subject ?? "Prueba de Canal", req.Body ?? "<p>Prueba de envío</p>");
        return Ok(new { success, message });
    }
}

public class TestChannelRequest
{
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Body { get; set; }
}
