using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Core.Interfaces;

namespace NotificationService.Infrastructure.DataConnectors;

public class DataSourceService : IDataSourceService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DataSourceService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<Dictionary<string, object?>>> FetchDataAsync(DataSourceConfig dataSource, string query, string? parametersJson = null)
    {
        if (dataSource == null)
            throw new ArgumentNullException(nameof(dataSource));

        if (dataSource.Type == DataSourceType.RestApi)
        {
            return await FetchFromRestApiAsync(dataSource, query);
        }

        using IDbConnection connection = CreateConnection(dataSource);
        connection.Open();

        object? param = null;
        if (!string.IsNullOrWhiteSpace(parametersJson))
        {
            try
            {
                param = JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson);
            }
            catch
            {
                // Fallback to null if parameters parsing fails
            }
        }

        var rows = (await connection.QueryAsync(query, param)).Cast<IDictionary<string, object>>().ToList();
        var result = new List<Dictionary<string, object?>>();

        foreach (var row in rows)
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in row)
            {
                dict[kvp.Key] = kvp.Value;
            }
            result.Add(dict);
        }

        return result;
    }

    private IDbConnection CreateConnection(DataSourceConfig ds)
    {
        return ds.Type switch
        {
            DataSourceType.Sqlite => new SqliteConnection(ds.ConnectionStringOrUrl),
            DataSourceType.SqlServer => new SqlConnection(ds.ConnectionStringOrUrl),
            _ => new SqliteConnection(ds.ConnectionStringOrUrl) // Default to SQLite
        };
    }

    private async Task<List<Dictionary<string, object?>>> FetchFromRestApiAsync(DataSourceConfig ds, string endpointPath)
    {
        var client = _httpClientFactory.CreateClient();
        var url = ds.ConnectionStringOrUrl.TrimEnd('/') + "/" + endpointPath.TrimStart('/');
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var result = new List<Dictionary<string, object?>>();

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    result.Add(JsonElementToDict(element));
                }
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            result.Add(JsonElementToDict(doc.RootElement));
        }

        return result;
    }

    private static Dictionary<string, object?> JsonElementToDict(JsonElement element)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in element.EnumerateObject())
        {
            dict[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString(),
                JsonValueKind.Number => prop.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => prop.Value.GetRawText()
            };
        }
        return dict;
    }
}
