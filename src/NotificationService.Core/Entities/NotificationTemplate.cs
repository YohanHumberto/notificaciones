using System;
using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities;

public class NotificationTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ChannelType ChannelType { get; set; }
    public string SubjectTemplate { get; set; } = string.Empty; // Fluid template
    public string BodyTemplate { get; set; } = string.Empty;    // Fluid template
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
