using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities;

public class NotificationChannelSettingValue
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public NotificationSettingDataType DataType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsSensitive { get; set; }
    public bool IsConfigured { get; set; }
    public object? Value { get; set; }
}
