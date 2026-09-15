using System;
using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities;

public class DataSourceConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DataSourceType Type { get; set; }
    public string ConnectionStringOrUrl { get; set; } = string.Empty;
    public string? ExtraConfigJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
