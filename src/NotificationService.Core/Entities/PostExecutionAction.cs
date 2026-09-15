using System;
using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities;

public class PostExecutionAction
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int DataSourceId { get; set; }
    public DataSourceConfig? DataSource { get; set; }

    public PostActionType ActionType { get; set; } = PostActionType.ExecuteSql;
    public string SqlQuery { get; set; } = string.Empty; // e.g. UPDATE Notifications SET Sent = 1 WHERE Id IN (@Ids)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
