using System;
using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities;

public class NotificationChannel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ChannelType Type { get; set; }
    public string ConfigJson { get; set; } = "{}"; // JSON storing SMTP options, Webhook URL, API Keys
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
