using System;
using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities;

public class ConditionalRule
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public ConditionType Type { get; set; } // SqlRowCount or LiquidExpression
    public int? DataSourceId { get; set; }
    public DataSourceConfig? DataSource { get; set; }

    public string? SqlQuery { get; set; }            // For SqlRowCount or auxiliary query
    public string? Expression { get; set; }          // Fluid/Liquid expression, e.g. "count > 0" or "items.size > 0" or "status == 'PENDING'"
    public int ExpectedMinCount { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
