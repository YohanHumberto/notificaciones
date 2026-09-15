using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly NotificationDbContext _dbContext;
    private readonly ITemplateRenderer _templateRenderer;

    public TemplatesController(NotificationDbContext dbContext, ITemplateRenderer templateRenderer)
    {
        _dbContext = dbContext;
        _templateRenderer = templateRenderer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _dbContext.Templates.ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var t = await _dbContext.Templates.FindAsync(id);
        if (t == null) return NotFound();
        return Ok(t);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NotificationTemplate template)
    {
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = template.Id }, template);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] NotificationTemplate updated)
    {
        var t = await _dbContext.Templates.FindAsync(id);
        if (t == null) return NotFound();

        t.Name = updated.Name;
        t.Description = updated.Description;
        t.ChannelType = updated.ChannelType;
        t.SubjectTemplate = updated.SubjectTemplate;
        t.BodyTemplate = updated.BodyTemplate;

        await _dbContext.SaveChangesAsync();
        return Ok(t);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _dbContext.Templates.FindAsync(id);
        if (t == null) return NotFound();

        _dbContext.Templates.Remove(t);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("preview")]
    public async Task<IActionResult> PreviewTemplate([FromBody] PreviewTemplateRequest req)
    {
        try
        {
            object model = new { };
            if (!string.IsNullOrWhiteSpace(req.SampleJsonData))
            {
                using var doc = JsonDocument.Parse(req.SampleJsonData);
                model = doc.RootElement.Clone();
            }

            string renderedSubject = await _templateRenderer.RenderAsync(req.SubjectTemplate ?? "", model);
            string renderedBody = await _templateRenderer.RenderAsync(req.BodyTemplate ?? "", model);

            return Ok(new { success = true, subject = renderedSubject, body = renderedBody });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, error = ex.Message });
        }
    }
}

public class PreviewTemplateRequest
{
    public string? SubjectTemplate { get; set; }
    public string? BodyTemplate { get; set; }
    public string? SampleJsonData { get; set; }
}
