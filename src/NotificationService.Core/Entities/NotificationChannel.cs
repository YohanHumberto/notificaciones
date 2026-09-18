using System;
using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities;

public class NotificationChannel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int ChannelTypeId { get; set; }
    public NotificationChannelType? ChannelType { get; set; }

    public ChannelType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
