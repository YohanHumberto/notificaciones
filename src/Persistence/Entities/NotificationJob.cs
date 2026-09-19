using Persistence.Enums;

namespace Persistence.Entities;

public class NotificationJob
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }

	public ScheduleType ScheduleType { get; set; }
	public string? CronExpression { get; set; } // e.g. "0 0/5 * * * ?"
	public DateTime? ScheduledAt { get; set; }

	public int ChannelId { get; set; }
	public NotificationChannel? Channel { get; set; }

	public int TemplateId { get; set; }
	public NotificationTemplate? Template { get; set; }

	public int? DataSourceId { get; set; }
	public DataSourceConfig? DataSource { get; set; }
	public string? DataQuery { get; set; }
	public string? QueryParametersJson { get; set; }

	public string RecipientExpression { get; set; } = string.Empty; // Static email/URL or Liquid expression e.g. "{{ items[0].email }}"

	public int? ConditionalRuleId { get; set; }
	public ConditionalRule? ConditionalRule { get; set; }

	public int? PostExecutionActionId { get; set; }
	public PostExecutionAction? PostExecutionAction { get; set; }

	public bool IsActive { get; set; } = true;
	public DateTime? LastRunAt { get; set; }
	public DateTime? NextRunAt { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
