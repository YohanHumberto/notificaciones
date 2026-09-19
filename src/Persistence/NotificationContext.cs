using Microsoft.EntityFrameworkCore;
using Persistence.Entities;
using Persistence.Enums;

namespace Persistence;

public class NotificationContext : DbContext
{
	private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public NotificationContext(DbContextOptions<NotificationContext> options) : base(options)
	{
	}

	public DbSet<NotificationChannelType> ChannelTypes => Set<NotificationChannelType>();
	public DbSet<NotificationChannel> Channels => Set<NotificationChannel>();
	public DbSet<NotificationSettingDefinition> SettingDefinitions => Set<NotificationSettingDefinition>();
	public DbSet<NotificationChannelSetting> ChannelSettings => Set<NotificationChannelSetting>();
	public DbSet<DataSourceConfig> DataSources => Set<DataSourceConfig>();
	public DbSet<NotificationTemplate> Templates => Set<NotificationTemplate>();
	public DbSet<ConditionalRule> ConditionalRules => Set<ConditionalRule>();
	public DbSet<PostExecutionAction> PostExecutionActions => Set<PostExecutionAction>();
	public DbSet<NotificationJob> Jobs => Set<NotificationJob>();
	public DbSet<ExecutionLog> ExecutionLogs => Set<ExecutionLog>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<NotificationChannelType>()
			.HasIndex(x => x.Code)
			.IsUnique();

		modelBuilder.Entity<NotificationSettingDefinition>()
			.HasIndex(x => new { x.ChannelTypeId, x.Code })
			.IsUnique();

		modelBuilder.Entity<NotificationChannelSetting>()
			.HasIndex(x => new { x.ChannelId, x.SettingDefinitionId })
			.IsUnique();

		modelBuilder.Entity<NotificationChannel>()
			.HasOne(c => c.ChannelType)
			.WithMany()
			.HasForeignKey(c => c.ChannelTypeId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<NotificationSettingDefinition>()
			.HasOne(d => d.ChannelType)
			.WithMany()
			.HasForeignKey(d => d.ChannelTypeId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<NotificationChannelSetting>()
			.HasOne(s => s.Channel)
			.WithMany()
			.HasForeignKey(s => s.ChannelId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<NotificationChannelSetting>()
			.HasOne(s => s.SettingDefinition)
			.WithMany()
			.HasForeignKey(s => s.SettingDefinitionId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<NotificationJob>()
			.HasOne(j => j.Channel)
			.WithMany()
			.HasForeignKey(j => j.ChannelId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<NotificationJob>()
			.HasOne(j => j.Template)
			.WithMany()
			.HasForeignKey(j => j.TemplateId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<NotificationJob>()
			.HasOne(j => j.DataSource)
			.WithMany()
			.HasForeignKey(j => j.DataSourceId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<NotificationJob>()
			.HasOne(j => j.ConditionalRule)
			.WithMany()
			.HasForeignKey(j => j.ConditionalRuleId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<NotificationJob>()
			.HasOne(j => j.PostExecutionAction)
			.WithMany()
			.HasForeignKey(j => j.PostExecutionActionId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<NotificationChannelType>().HasData(
			new NotificationChannelType { Id = 1, Code = "EMAIL", Name = "Email", Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationChannelType { Id = 2, Code = "SMS", Name = "SMS", Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationChannelType { Id = 3, Code = "WEBHOOK", Name = "Webhook", Enabled = true, CreatedAt = SeedTimestamp }
		);

		modelBuilder.Entity<NotificationSettingDefinition>().HasData(
			new NotificationSettingDefinition { Id = 1, ChannelTypeId = 1, Code = "SMTP_HOST", Name = "SMTP Host", DataType = NotificationSettingDataType.String, IsRequired = true, IsSensitive = false, Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationSettingDefinition { Id = 2, ChannelTypeId = 1, Code = "SMTP_PORT", Name = "SMTP Port", DataType = NotificationSettingDataType.Int, IsRequired = true, IsSensitive = false, Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationSettingDefinition { Id = 3, ChannelTypeId = 1, Code = "SMTP_USERNAME", Name = "SMTP Username", DataType = NotificationSettingDataType.String, IsRequired = true, IsSensitive = false, Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationSettingDefinition { Id = 4, ChannelTypeId = 1, Code = "SMTP_PASSWORD", Name = "SMTP Password", DataType = NotificationSettingDataType.String, IsRequired = true, IsSensitive = true, Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationSettingDefinition { Id = 5, ChannelTypeId = 1, Code = "FROM_ADDRESS", Name = "From Address", DataType = NotificationSettingDataType.String, IsRequired = true, IsSensitive = false, Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationSettingDefinition { Id = 6, ChannelTypeId = 1, Code = "USE_SSL", Name = "Use SSL", DataType = NotificationSettingDataType.Bool, IsRequired = false, IsSensitive = false, Enabled = true, CreatedAt = SeedTimestamp },

			new NotificationSettingDefinition { Id = 7, ChannelTypeId = 2, Code = "PROVIDER_URL", Name = "Provider URL", DataType = NotificationSettingDataType.String, IsRequired = true, IsSensitive = false, Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationSettingDefinition { Id = 8, ChannelTypeId = 2, Code = "API_KEY", Name = "API Key", DataType = NotificationSettingDataType.String, IsRequired = true, IsSensitive = true, Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationSettingDefinition { Id = 9, ChannelTypeId = 2, Code = "SENDER", Name = "Sender", DataType = NotificationSettingDataType.String, IsRequired = true, IsSensitive = false, Enabled = true, CreatedAt = SeedTimestamp },

			new NotificationSettingDefinition { Id = 10, ChannelTypeId = 3, Code = "URL", Name = "Webhook URL", DataType = NotificationSettingDataType.String, IsRequired = true, IsSensitive = false, Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationSettingDefinition { Id = 11, ChannelTypeId = 3, Code = "HTTP_METHOD", Name = "HTTP Method", DataType = NotificationSettingDataType.String, IsRequired = true, IsSensitive = false, Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationSettingDefinition { Id = 12, ChannelTypeId = 3, Code = "TIMEOUT", Name = "Timeout", DataType = NotificationSettingDataType.Int, IsRequired = false, IsSensitive = false, Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationSettingDefinition { Id = 13, ChannelTypeId = 3, Code = "AUTH_TYPE", Name = "Auth Type", DataType = NotificationSettingDataType.String, IsRequired = false, IsSensitive = false, Enabled = true, CreatedAt = SeedTimestamp },
			new NotificationSettingDefinition { Id = 14, ChannelTypeId = 3, Code = "AUTH_TOKEN", Name = "Auth Token", DataType = NotificationSettingDataType.String, IsRequired = false, IsSensitive = true, Enabled = true, CreatedAt = SeedTimestamp }
		);

		modelBuilder.Entity<NotificationChannel>().HasData(
			new NotificationChannel { Id = 1, Name = "Servidor SMTP Principal", ChannelTypeId = 1, Type = ChannelType.Email, IsActive = true, CreatedAt = SeedTimestamp },
			new NotificationChannel { Id = 2, Name = "Webhook Sistema Externo", ChannelTypeId = 3, Type = ChannelType.Webhook, IsActive = true, CreatedAt = SeedTimestamp }
		);

		modelBuilder.Entity<NotificationChannelSetting>().HasData(
			new NotificationChannelSetting { Id = 1, ChannelId = 1, SettingDefinitionId = 1, StringValue = "smtp.gmail.com", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
			new NotificationChannelSetting { Id = 2, ChannelId = 1, SettingDefinitionId = 2, IntValue = 587, CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
			new NotificationChannelSetting { Id = 3, ChannelId = 1, SettingDefinitionId = 3, StringValue = "demo@gmail.com", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
			new NotificationChannelSetting { Id = 4, ChannelId = 1, SettingDefinitionId = 5, StringValue = "no-reply@empresa.com", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
			new NotificationChannelSetting { Id = 5, ChannelId = 1, SettingDefinitionId = 6, BoolValue = true, CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
			new NotificationChannelSetting { Id = 6, ChannelId = 2, SettingDefinitionId = 10, StringValue = "https://webhook.site/demo-notification", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp },
			new NotificationChannelSetting { Id = 7, ChannelId = 2, SettingDefinitionId = 11, StringValue = "POST", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp }
		);

		modelBuilder.Entity<DataSourceConfig>().HasData(
			new DataSourceConfig
			{
				Id = 1,
				Name = "Base de Datos SQLite Interna / Demo",
				Type = DataSourceType.Sqlite,
				ConnectionStringOrUrl = "Data Source=notification_service.db",
				CreatedAt = SeedTimestamp
			}
		);

		modelBuilder.Entity<NotificationTemplate>().HasData(
			new NotificationTemplate
			{
				Id = 1,
				Name = "Email Alerta Pendientes",
				ChannelType = ChannelType.Email,
				SubjectTemplate = "Atención: Tienes {{ total_pendientes }} tareas pendientes",
				BodyTemplate = "<h2>Hola {{ usuario_nombre }}</h2><p>Tienes <strong>{{ total_pendientes }}</strong> tareas pendientes por procesar.</p><ul>{% for item in items %}<li>{{ item.titulo }} - Status: {{ item.estado }}</li>{% endfor %}</ul>",
				CreatedAt = SeedTimestamp
			},
			new NotificationTemplate
			{
				Id = 2,
				Name = "Webhook Notification Payload",
				ChannelType = ChannelType.Webhook,
				SubjectTemplate = "ALERTA_WEBHOOK",
				BodyTemplate = "{\"event\":\"PENDING_ITEMS_ALERT\",\"total\":{{ total_pendientes }},\"timestamp\":\"{{ now }}\"}",
				CreatedAt = SeedTimestamp
			}
		);

		modelBuilder.Entity<ConditionalRule>().HasData(
			new ConditionalRule
			{
				Id = 1,
				Name = "Verificar si hay filas retenidas",
				Type = ConditionType.LiquidExpression,
				Expression = "total_pendientes > 0",
				ExpectedMinCount = 1,
				CreatedAt = SeedTimestamp
			}
		);
	}
}
