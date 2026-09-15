using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Core.Interfaces;

namespace NotificationService.Infrastructure.PostActions;

public class PostExecutionProcessor : IPostExecutionProcessor
{
    private readonly ITemplateRenderer _templateRenderer;

    public PostExecutionProcessor(ITemplateRenderer templateRenderer)
    {
        _templateRenderer = templateRenderer;
    }

    public async Task<string> ExecuteActionAsync(PostExecutionAction action, List<Dictionary<string, object?>> contextData)
    {
        if (action == null || action.DataSource == null || string.IsNullOrWhiteSpace(action.SqlQuery))
            return "No se definió acción posterior o consulta vacía.";

        try
        {
            var model = new Dictionary<string, object?>
            {
                { "items", contextData },
                { "total_items", contextData?.Count ?? 0 }
            };

            // Render SQL query dynamically using Fluid (allows liquid parameters inside SQL post-action)
            string renderedSql = await _templateRenderer.RenderAsync(action.SqlQuery, model);

            using IDbConnection connection = CreateConnection(action.DataSource);
            connection.Open();

            int affectedRows = await connection.ExecuteAsync(renderedSql);

            return $"Acción post-ejecución procesada con éxito en Data Source '{action.DataSource.Name}'. Filas afectadas: {affectedRows}. Consulta ejecutada: {renderedSql}";
        }
        catch (Exception ex)
        {
            return $"Error en acción post-ejecución SQL: {ex.Message}";
        }
    }

    private static IDbConnection CreateConnection(DataSourceConfig ds)
    {
        return ds.Type switch
        {
            DataSourceType.Sqlite => new SqliteConnection(ds.ConnectionStringOrUrl),
            DataSourceType.SqlServer => new SqlConnection(ds.ConnectionStringOrUrl),
            _ => new SqliteConnection(ds.ConnectionStringOrUrl)
        };
    }
}
