using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NotificationService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialRelationalChannelConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChannelTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnectionStringOrUrl = table.Column<string>(type: "TEXT", nullable: false),
                    ExtraConfigJson = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: false),
                    JobName = table.Column<string>(type: "TEXT", nullable: false),
                    TriggeredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ConditionEvaluated = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConditionResult = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataRowsFetched = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetRecipient = table.Column<string>(type: "TEXT", nullable: true),
                    RenderedSubject = table.Column<string>(type: "TEXT", nullable: true),
                    RenderedBody = table.Column<string>(type: "TEXT", nullable: true),
                    PostExecutionDetails = table.Column<string>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    DurationMs = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ChannelType = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectTemplate = table.Column<string>(type: "TEXT", nullable: false),
                    BodyTemplate = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ChannelTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Channels_ChannelTypes_ChannelTypeId",
                        column: x => x.ChannelTypeId,
                        principalTable: "ChannelTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettingDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DataType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSensitive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultValue = table.Column<string>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettingDefinitions_ChannelTypes_ChannelTypeId",
                        column: x => x.ChannelTypeId,
                        principalTable: "ChannelTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConditionalRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    DataSourceId = table.Column<int>(type: "INTEGER", nullable: true),
                    SqlQuery = table.Column<string>(type: "TEXT", nullable: true),
                    Expression = table.Column<string>(type: "TEXT", nullable: true),
                    ExpectedMinCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionalRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConditionalRules_DataSources_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PostExecutionActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DataSourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionType = table.Column<int>(type: "INTEGER", nullable: false),
                    SqlQuery = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostExecutionActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostExecutionActions_DataSources_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: false),
                    SettingDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    StringValue = table.Column<string>(type: "TEXT", nullable: true),
                    IntValue = table.Column<int>(type: "INTEGER", nullable: true),
                    BoolValue = table.Column<bool>(type: "INTEGER", nullable: true),
                    DecimalValue = table.Column<decimal>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelSettings_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelSettings_SettingDefinitions_SettingDefinitionId",
                        column: x => x.SettingDefinitionId,
                        principalTable: "SettingDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ScheduleType = table.Column<int>(type: "INTEGER", nullable: false),
                    CronExpression = table.Column<string>(type: "TEXT", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: false),
                    TemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataSourceId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataQuery = table.Column<string>(type: "TEXT", nullable: true),
                    QueryParametersJson = table.Column<string>(type: "TEXT", nullable: true),
                    RecipientExpression = table.Column<string>(type: "TEXT", nullable: false),
                    ConditionalRuleId = table.Column<int>(type: "INTEGER", nullable: true),
                    PostExecutionActionId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_ConditionalRules_ConditionalRuleId",
                        column: x => x.ConditionalRuleId,
                        principalTable: "ConditionalRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Jobs_DataSources_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Jobs_PostExecutionActions_PostExecutionActionId",
                        column: x => x.PostExecutionActionId,
                        principalTable: "PostExecutionActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Jobs_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ChannelTypes",
                columns: new[] { "Id", "Code", "CreatedAt", "Enabled", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "EMAIL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Email", null },
                    { 2, "SMS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "SMS", null },
                    { 3, "WEBHOOK", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Webhook", null }
                });

            migrationBuilder.InsertData(
                table: "ConditionalRules",
                columns: new[] { "Id", "CreatedAt", "DataSourceId", "Description", "ExpectedMinCount", "Expression", "Name", "SqlQuery", "Type" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "total_pendientes > 0", "Verificar si hay filas retenidas", null, 1 });

            migrationBuilder.InsertData(
                table: "DataSources",
                columns: new[] { "Id", "ConnectionStringOrUrl", "CreatedAt", "ExtraConfigJson", "Name", "Type" },
                values: new object[] { 1, "Data Source=notification_service.db", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Base de Datos SQLite Interna / Demo", 2 });

            migrationBuilder.InsertData(
                table: "Templates",
                columns: new[] { "Id", "BodyTemplate", "ChannelType", "CreatedAt", "Description", "Name", "SubjectTemplate" },
                values: new object[,]
                {
                    { 1, "<h2>Hola {{ usuario_nombre }}</h2><p>Tienes <strong>{{ total_pendientes }}</strong> tareas pendientes por procesar.</p><ul>{% for item in items %}<li>{{ item.titulo }} - Status: {{ item.estado }}</li>{% endfor %}</ul>", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Email Alerta Pendientes", "Atención: Tienes {{ total_pendientes }} tareas pendientes" },
                    { 2, "{\"event\":\"PENDING_ITEMS_ALERT\",\"total\":{{ total_pendientes }},\"timestamp\":\"{{ now }}\"}", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Webhook Notification Payload", "ALERTA_WEBHOOK" }
                });

            migrationBuilder.InsertData(
                table: "Channels",
                columns: new[] { "Id", "ChannelTypeId", "CreatedAt", "IsActive", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Servidor SMTP Principal", 1, null },
                    { 2, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Webhook Sistema Externo", 2, null }
                });

            migrationBuilder.InsertData(
                table: "SettingDefinitions",
                columns: new[] { "Id", "ChannelTypeId", "Code", "CreatedAt", "DataType", "DefaultValue", "Enabled", "IsRequired", "IsSensitive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "SMTP_HOST", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, true, false, "SMTP Host", null },
                    { 2, 1, "SMTP_PORT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, true, true, false, "SMTP Port", null },
                    { 3, 1, "SMTP_USERNAME", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, true, false, "SMTP Username", null },
                    { 4, 1, "SMTP_PASSWORD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, true, true, "SMTP Password", null },
                    { 5, 1, "FROM_ADDRESS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, true, false, "From Address", null },
                    { 6, 1, "USE_SSL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, true, false, false, "Use SSL", null },
                    { 7, 2, "PROVIDER_URL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, true, false, "Provider URL", null },
                    { 8, 2, "API_KEY", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, true, true, "API Key", null },
                    { 9, 2, "SENDER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, true, false, "Sender", null },
                    { 10, 3, "URL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, true, false, "Webhook URL", null },
                    { 11, 3, "HTTP_METHOD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, true, false, "HTTP Method", null },
                    { 12, 3, "TIMEOUT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, true, false, false, "Timeout", null },
                    { 13, 3, "AUTH_TYPE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, false, false, "Auth Type", null },
                    { 14, 3, "AUTH_TOKEN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, true, false, true, "Auth Token", null }
                });

            migrationBuilder.InsertData(
                table: "ChannelSettings",
                columns: new[] { "Id", "BoolValue", "ChannelId", "CreatedAt", "DecimalValue", "IntValue", "SettingDefinitionId", "StringValue", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 1, "smtp.gmail.com", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, null, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 587, 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, null, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, "demo@gmail.com", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, null, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 5, "no-reply@empresa.com", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, true, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 6, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, null, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 10, "https://webhook.site/demo-notification", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, null, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 11, "POST", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Channels_ChannelTypeId",
                table: "Channels",
                column: "ChannelTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSettings_ChannelId_SettingDefinitionId",
                table: "ChannelSettings",
                columns: new[] { "ChannelId", "SettingDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSettings_SettingDefinitionId",
                table: "ChannelSettings",
                column: "SettingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelTypes_Code",
                table: "ChannelTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalRules_DataSourceId",
                table: "ConditionalRules",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ChannelId",
                table: "Jobs",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ConditionalRuleId",
                table: "Jobs",
                column: "ConditionalRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_DataSourceId",
                table: "Jobs",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PostExecutionActionId",
                table: "Jobs",
                column: "PostExecutionActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_TemplateId",
                table: "Jobs",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PostExecutionActions_DataSourceId",
                table: "PostExecutionActions",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SettingDefinitions_ChannelTypeId_Code",
                table: "SettingDefinitions",
                columns: new[] { "ChannelTypeId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelSettings");

            migrationBuilder.DropTable(
                name: "ExecutionLogs");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "SettingDefinitions");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "ConditionalRules");

            migrationBuilder.DropTable(
                name: "PostExecutionActions");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "ChannelTypes");

            migrationBuilder.DropTable(
                name: "DataSources");
        }
    }
}
