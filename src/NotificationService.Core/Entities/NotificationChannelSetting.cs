using System;

namespace NotificationService.Core.Entities;

public class NotificationChannelSetting
{
    public int Id { get; set; }

    public int ChannelId { get; set; }
    public NotificationChannel? Channel { get; set; }

    public int SettingDefinitionId { get; set; }
    public NotificationSettingDefinition? SettingDefinition { get; set; }

    public string? StringValue { get; set; }
    public int? IntValue { get; set; }
    public bool? BoolValue { get; set; }
    public decimal? DecimalValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
