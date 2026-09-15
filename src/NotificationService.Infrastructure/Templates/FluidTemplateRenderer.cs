using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;
using Fluid;
using NotificationService.Core.Interfaces;

namespace NotificationService.Infrastructure.Templates;

public class FluidTemplateRenderer : ITemplateRenderer
{
    private static readonly FluidParser Parser = new FluidParser();

    public async Task<string> RenderAsync(string templateText, object model)
    {
        if (string.IsNullOrWhiteSpace(templateText))
            return string.Empty;

        if (!Parser.TryParse(templateText, out var template, out var error))
        {
            throw new InvalidOperationException($"Error parseando plantilla Fluid: {error}");
        }

        var options = new TemplateOptions();
        options.MemberAccessStrategy.Register<ExpandoObject>();
        options.MemberAccessStrategy.Register<Dictionary<string, object?>>();
        options.MemberAccessStrategy.Register<JsonElement>();

        var context = new TemplateContext(options);

        if (model is IDictionary<string, object?> dict)
        {
            foreach (var kvp in dict)
            {
                context.SetValue(kvp.Key, ConvertToJsonObject(kvp.Value));
            }
        }
        else if (model is JsonElement jsonElem)
        {
            if (jsonElem.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in jsonElem.EnumerateObject())
                {
                    context.SetValue(prop.Name, ConvertToJsonObject(prop.Value));
                }
            }
        }

        context.SetValue("now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        return await template.RenderAsync(context);
    }

    private static object? ConvertToJsonObject(object? val)
    {
        if (val is JsonElement elem)
        {
            switch (elem.ValueKind)
            {
                case JsonValueKind.String: return elem.GetString();
                case JsonValueKind.Number:
                    if (elem.TryGetInt64(out long l)) return l;
                    if (elem.TryGetDouble(out double dVal)) return dVal;
                    return elem.GetRawText();
                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;
                case JsonValueKind.Null: return null;
                case JsonValueKind.Array:
                    var list = new List<object?>();
                    foreach (var item in elem.EnumerateArray())
                    {
                        list.Add(ConvertToJsonObject(item));
                    }
                    return list;
                case JsonValueKind.Object:
                    var objDict = new Dictionary<string, object?>();
                    foreach (var prop in elem.EnumerateObject())
                    {
                        objDict[prop.Name] = ConvertToJsonObject(prop.Value);
                    }
                    return objDict;
                default: return elem.GetRawText();
            }
        }
        return val;
    }
}
