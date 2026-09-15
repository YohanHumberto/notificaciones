using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<NotificationChannel> Channels => Set<NotificationChannel>();
    public DbSet<DataSourceConfig> DataSources => Set<DataSourceConfig>();
    public DbSet<NotificationTemplate> Templates => Set<NotificationTemplate>();
    public DbSet<ConditionalRule> ConditionalRules => Set<ConditionalRule>();
    public DbSet<PostExecutionAction> PostExecutionActions => Set<PostExecutionAction>();
    public DbSet<NotificationJob> Jobs => Set<NotificationJob>();
    public DbSet<ExecutionLog> ExecutionLogs => Set<ExecutionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        // Seed default sample data
        modelBuilder.Entity<NotificationChannel>().HasData(
            new NotificationChannel
            {
                Id = 1,
                Name = "Servidor SMTP Principal",
                Type = ChannelType.Email,
                ConfigJson = "{\"Host\":\"smtp.gmail.com\",\"Port\":587,\"Username\":\"demo@gmail.com\",\"Password\":\"demo123\",\"FromAddress\":\"no-reply@empresa.com\",\"FromName\":\"Sistema de Notificaciones\"}",
                IsActive = true
            },
            new NotificationChannel
            {
                Id = 2,
                Name = "Webhook Sistema Externo",
                Type = ChannelType.Webhook,
                ConfigJson = "{\"Url\":\"https://webhook.site/demo-notification\",\"HttpMethod\":\"POST\",\"Headers\":{\"Authorization\":\"Bearer token-demo-123\"}}",
                IsActive = true
            }
        );

        modelBuilder.Entity<DataSourceConfig>().HasData(
            new DataSourceConfig
            {
                Id = 1,
                Name = "Base de Datos SQLite Interna / Demo",
                Type = DataSourceType.Sqlite,
                ConnectionStringOrUrl = "Data Source=notification_service.db"
            }
        );

        modelBuilder.Entity<NotificationTemplate>().HasData(
            new NotificationTemplate
            {
                Id = 1,
                Name = "Email Alerta Pendientes",
                ChannelType = ChannelType.Email,
                SubjectTemplate = "Atención: Tienes {{ total_pendientes }} tareas pendientes",
                BodyTemplate = "<h2>Hola {{ usuario_nombre }}</h2><p>Tienes <strong>{{ total_pendientes }}</strong> tareas pendientes por procesar.</p><ul>{% for item in items %}<li>{{ item.titulo }} - Status: {{ item.estado }}</li>{% endfor %}</ul>"
            },
            new NotificationTemplate
            {
                Id = 2,
                Name = "Webhook Notification Payload",
                ChannelType = ChannelType.Webhook,
                SubjectTemplate = "ALERTA_WEBHOOK",
                BodyTemplate = "{\"event\":\"PENDING_ITEMS_ALERT\",\"total\":{{ total_pendientes }},\"timestamp\":\"{{ now }}\"}"
            }
        );

        modelBuilder.Entity<ConditionalRule>().HasData(
            new ConditionalRule
            {
                Id = 1,
                Name = "Verificar si hay filas retenidas",
                Type = ConditionType.LiquidExpression,
                Expression = "total_pendientes > 0",
                ExpectedMinCount = 1
            }
        );
    }
}
