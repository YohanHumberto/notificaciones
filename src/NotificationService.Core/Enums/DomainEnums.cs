namespace NotificationService.Core.Enums;

public enum ChannelType
{
    Email = 1,
    Webhook = 2,
    Sms = 3
}

public enum ScheduleType
{
    OneTime = 1,
    CronRecurring = 2
}

public enum DataSourceType
{
    SqlServer = 1,
    Sqlite = 2,
    PostgreSql = 3,
    MySql = 4,
    RestApi = 5
}

public enum PostActionType
{
    ExecuteSql = 1
}

public enum ExecutionStatus
{
    Success = 1,
    SkippedCondition = 2,
    Failed = 3
}

public enum ConditionType
{
    LiquidExpression = 1,
    SqlRowCount = 2
}
