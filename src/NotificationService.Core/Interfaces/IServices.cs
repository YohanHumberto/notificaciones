using System.Collections.Generic;
using System.Threading.Tasks;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Core.Interfaces;

public interface IDataSourceService
{
    Task<List<Dictionary<string, object?>>> FetchDataAsync(DataSourceConfig dataSource, string query, string? parametersJson = null);
}

public interface IConditionEvaluator
{
    Task<(bool ShouldProceed, string Details)> EvaluateAsync(ConditionalRule rule, List<Dictionary<string, object?>> contextData);
}

public interface ITemplateRenderer
{
    Task<string> RenderAsync(string templateText, object model);
}

public interface INotificationChannelProvider
{
    ChannelType ChannelType { get; }
    Task<(bool Success, string Message)> SendNotificationAsync(NotificationChannel channelConfig, string recipient, string subject, string body);
}

public interface INotificationChannelConfigurationService
{
    Task<IReadOnlyList<NotificationChannelSettingValue>> GetSettingsAsync(int channelId);
    Task<NotificationChannelSettingValue?> GetSettingValueAsync(int channelId, string code);
    Task<string?> GetSecretAsync(int channelId, string code);
    Task<IReadOnlyList<string>> ValidateAsync(int channelId);
    Task SaveSettingAsync(int channelId, string code, object? value, bool isExplicitDelete = false);
    Task DeleteSettingAsync(int channelId, string code);
}

public interface IPostExecutionProcessor
{
    Task<string> ExecuteActionAsync(PostExecutionAction action, List<Dictionary<string, object?>> contextData);
}

public interface INotificationJobProcessor
{
    Task<ExecutionLog> ProcessJobAsync(int jobId);
}

public interface ISchedulerService
{
    Task SyncJobScheduleAsync(NotificationJob job);
    Task RemoveJobScheduleAsync(int jobId);
    Task TriggerJobImmediatelyAsync(int jobId);
}
