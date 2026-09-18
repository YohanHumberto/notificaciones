using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;
using NotificationService.Core.Enums;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChannelsController : ControllerBase
{
    private readonly NotificationDbContext _dbContext;
    private readonly INotificationChannelConfigurationService _configurationService;
    private readonly System.Collections.Generic.IEnumerable<INotificationChannelProvider> _providers;

    public ChannelsController(
        NotificationDbContext dbContext,
        INotificationChannelConfigurationService configurationService,
        System.Collections.Generic.IEnumerable<INotificationChannelProvider> providers)
    {
        _dbContext = dbContext;
        _configurationService = configurationService;
        _providers = providers;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var channels = await _dbContext.Channels
            .AsNoTracking()
            .Select(channel => new ChannelResponse
            {
                Id = channel.Id,
                Name = channel.Name,
                ChannelTypeId = channel.ChannelTypeId,
                Type = channel.Type,
                IsActive = channel.IsActive,
                CreatedAt = channel.CreatedAt,
                UpdatedAt = channel.UpdatedAt
            })
            .ToListAsync();
        return Ok(channels);
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetTypes()
    {
        var types = await _dbContext.ChannelTypes
            .AsNoTracking()
            .Where(type => type.Enabled)
            .OrderBy(type => type.Name)
            .Select(type => new { type.Id, type.Code, type.Name })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("types/{typeId}/definitions")]
    public async Task<IActionResult> GetDefinitions(int typeId)
    {
        var typeExists = await _dbContext.ChannelTypes.AnyAsync(type => type.Id == typeId && type.Enabled);
        if (!typeExists) return NotFound();

        var definitions = await _dbContext.SettingDefinitions
            .AsNoTracking()
            .Where(definition => definition.ChannelTypeId == typeId && definition.Enabled)
            .OrderBy(definition => definition.Id)
            .ToListAsync();

        return Ok(definitions.Select(definition => new
        {
            definition.Code,
            definition.Name,
            DataType = definition.DataType.ToString().ToUpperInvariant(),
            definition.IsRequired,
            definition.IsSensitive,
            definition.DefaultValue
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var channel = await _dbContext.Channels.FindAsync(id);
        if (channel == null) return NotFound();

        var settings = await _configurationService.GetSettingsAsync(id);
        return Ok(new
        {
            channel.Id,
            channel.Name,
            channel.Type,
            channel.ChannelTypeId,
            channel.IsActive,
            channel.CreatedAt,
            channel.UpdatedAt,
            settings
        });
    }

    [HttpGet("{id}/settings")]
    public async Task<IActionResult> GetSettings(int id)
    {
        var channel = await _dbContext.Channels.FindAsync(id);
        if (channel == null) return NotFound();

        var settings = await _configurationService.GetSettingsAsync(id);
        return Ok(settings);
    }

    [HttpPost("{id}/validate")]
    public async Task<IActionResult> Validate(int id)
    {
        var errors = await _configurationService.ValidateAsync(id);
        if (errors.Count == 1 && errors[0] == "El canal no existe.") return NotFound();
        return Ok(new { valid = errors.Count == 0, errors });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ChannelCreateRequest request)
    {
        var channelType = await _dbContext.ChannelTypes.FirstOrDefaultAsync(t => t.Id == request.ChannelTypeId && t.Enabled);
        if (channelType == null) return BadRequest("El tipo de canal no existe o está deshabilitado.");

        var channel = new NotificationChannel
        {
            Name = request.Name,
            ChannelTypeId = request.ChannelTypeId,
            Type = request.Type,
            IsActive = request.IsActive
        };
        _dbContext.Channels.Add(channel);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = channel.Id }, new ChannelResponse
        {
            Id = channel.Id,
            Name = channel.Name,
            ChannelTypeId = channel.ChannelTypeId,
            Type = channel.Type,
            IsActive = channel.IsActive,
            CreatedAt = channel.CreatedAt,
            UpdatedAt = channel.UpdatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ChannelUpdateRequest updated)
    {
        var channel = await _dbContext.Channels.FindAsync(id);
        if (channel == null) return NotFound();

        var channelType = await _dbContext.ChannelTypes.FirstOrDefaultAsync(t => t.Id == updated.ChannelTypeId && t.Enabled);
        if (channelType == null) return BadRequest("El tipo de canal no existe o está deshabilitado.");

        channel.Name = updated.Name;
        channel.Type = updated.Type;
        channel.ChannelTypeId = updated.ChannelTypeId;
        channel.IsActive = updated.IsActive;
        channel.UpdatedAt = System.DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(new ChannelResponse
        {
            Id = channel.Id,
            Name = channel.Name,
            ChannelTypeId = channel.ChannelTypeId,
            Type = channel.Type,
            IsActive = channel.IsActive,
            CreatedAt = channel.CreatedAt,
            UpdatedAt = channel.UpdatedAt
        });
    }

    [HttpPut("{id}/settings/{code}")]
    public async Task<IActionResult> UpsertSetting(int id, string code, [FromBody] ChannelSettingUpdateRequest request)
    {
        var channel = await _dbContext.Channels.FindAsync(id);
        if (channel == null) return NotFound();

        if (request.Delete)
        {
            await _configurationService.DeleteSettingAsync(id, code);
            return NoContent();
        }

        await _configurationService.SaveSettingAsync(id, code, request.Value, false);
        return Ok(new { success = true, code });
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

public class ChannelSettingUpdateRequest
{
    public object? Value { get; set; }
    public bool Delete { get; set; }
}

public class ChannelCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public int ChannelTypeId { get; set; }
    public ChannelType Type { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ChannelUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public int ChannelTypeId { get; set; }
    public ChannelType Type { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ChannelResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ChannelTypeId { get; set; }
    public ChannelType Type { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
