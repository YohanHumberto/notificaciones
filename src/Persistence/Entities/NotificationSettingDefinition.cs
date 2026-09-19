using Persistence.Enums;

namespace Persistence.Entities;

public class NotificationSettingDefinition
{
	public int Id { get; set; }

	public int ChannelTypeId { get; set; }
	public NotificationChannelType? ChannelType { get; set; }

	public string Code { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public NotificationSettingDataType DataType { get; set; }
	public bool IsRequired { get; set; }
	public bool IsSensitive { get; set; }
	public string? DefaultValue { get; set; }
	public bool Enabled { get; set; } = true;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }
}
