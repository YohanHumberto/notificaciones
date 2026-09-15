using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConditionsController : ControllerBase
{
    private readonly NotificationDbContext _dbContext;
    private readonly IConditionEvaluator _conditionEvaluator;
    private readonly IDataSourceService _dataSourceService;

    public ConditionsController(
        NotificationDbContext dbContext,
        IConditionEvaluator conditionEvaluator,
        IDataSourceService dataSourceService)
    {
        _dbContext = dbContext;
        _conditionEvaluator = conditionEvaluator;
        _dataSourceService = dataSourceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _dbContext.ConditionalRules.Include(r => r.DataSource).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rule = await _dbContext.ConditionalRules.Include(r => r.DataSource).FirstOrDefaultAsync(r => r.Id == id);
        if (rule == null) return NotFound();
        return Ok(rule);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ConditionalRule rule)
    {
        _dbContext.ConditionalRules.Add(rule);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ConditionalRule updated)
    {
        var rule = await _dbContext.ConditionalRules.FindAsync(id);
        if (rule == null) return NotFound();

        rule.Name = updated.Name;
        rule.Description = updated.Description;
        rule.Type = updated.Type;
        rule.DataSourceId = updated.DataSourceId;
        rule.SqlQuery = updated.SqlQuery;
        rule.Expression = updated.Expression;
        rule.ExpectedMinCount = updated.ExpectedMinCount;

        await _dbContext.SaveChangesAsync();
        return Ok(rule);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var rule = await _dbContext.ConditionalRules.FindAsync(id);
        if (rule == null) return NotFound();

        _dbContext.ConditionalRules.Remove(rule);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestCondition([FromBody] TestConditionRequest req)
    {
        try
        {
            List<Dictionary<string, object?>> context = new();
            if (req.Rule.DataSourceId.HasValue && !string.IsNullOrWhiteSpace(req.Rule.SqlQuery))
            {
                var ds = await _dbContext.DataSources.FindAsync(req.Rule.DataSourceId.Value);
                if (ds != null)
                {
                    req.Rule.DataSource = ds;
                    context = await _dataSourceService.FetchDataAsync(ds, req.Rule.SqlQuery);
                }
            }

            var (shouldProceed, details) = await _conditionEvaluator.EvaluateAsync(req.Rule, context);
            return Ok(new { success = true, shouldProceed, details });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, error = ex.Message });
        }
    }
}

public class TestConditionRequest
{
    public ConditionalRule Rule { get; set; } = new();
}
