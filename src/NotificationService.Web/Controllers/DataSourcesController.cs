using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataSourcesController : ControllerBase
{
    private readonly NotificationDbContext _dbContext;
    private readonly IDataSourceService _dataSourceService;

    public DataSourcesController(NotificationDbContext dbContext, IDataSourceService dataSourceService)
    {
        _dbContext = dbContext;
        _dataSourceService = dataSourceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _dbContext.DataSources.ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ds = await _dbContext.DataSources.FindAsync(id);
        if (ds == null) return NotFound();
        return Ok(ds);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DataSourceConfig ds)
    {
        _dbContext.DataSources.Add(ds);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = ds.Id }, ds);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DataSourceConfig updated)
    {
        var ds = await _dbContext.DataSources.FindAsync(id);
        if (ds == null) return NotFound();

        ds.Name = updated.Name;
        ds.Type = updated.Type;
        ds.ConnectionStringOrUrl = updated.ConnectionStringOrUrl;
        ds.ExtraConfigJson = updated.ExtraConfigJson;

        await _dbContext.SaveChangesAsync();
        return Ok(ds);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ds = await _dbContext.DataSources.FindAsync(id);
        if (ds == null) return NotFound();

        _dbContext.DataSources.Remove(ds);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestQuery([FromBody] TestQueryRequest req)
    {
        try
        {
            DataSourceConfig? ds = null;
            if (req.DataSourceId.HasValue)
            {
                ds = await _dbContext.DataSources.FindAsync(req.DataSourceId.Value);
            }
            else if (req.TempDataSource != null)
            {
                ds = req.TempDataSource;
            }

            if (ds == null) return BadRequest("Debe especificar una fuente de datos.");

            var data = await _dataSourceService.FetchDataAsync(ds, req.Query, req.ParametersJson);
            return Ok(new { success = true, rowCount = data.Count, data });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, error = ex.Message });
        }
    }
}

public class TestQueryRequest
{
    public int? DataSourceId { get; set; }
    public DataSourceConfig? TempDataSource { get; set; }
    public string Query { get; set; } = string.Empty;
    public string? ParametersJson { get; set; }
}
