using Persistence.Enums;

namespace Persistence.Entities;

public class ExecutionLog
{
	public int Id { get; set; }
	public int JobId { get; set; }
	public string JobName { get; set; } = string.Empty;

	public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
	public ExecutionStatus Status { get; set; }

	public bool ConditionEvaluated { get; set; }
	public bool ConditionResult { get; set; }
	public int DataRowsFetched { get; set; }

	public string? TargetRecipient { get; set; }
	public string? RenderedSubject { get; set; }
	public string? RenderedBody { get; set; }
	public string? PostExecutionDetails { get; set; }
	public string? ErrorMessage { get; set; }
	public long DurationMs { get; set; }
}
