using Domain.Interfaces;
using Persistence.Entities;
using Persistence.Enums;

namespace Application.Evaluators;

public class ConditionEvaluator : IConditionEvaluator
{
	private readonly IDataSourceService _dataSourceService;
	private readonly ITemplateRenderer _templateRenderer;

	public ConditionEvaluator(IDataSourceService dataSourceService, ITemplateRenderer templateRenderer)
	{
		_dataSourceService = dataSourceService;
		_templateRenderer = templateRenderer;
	}

	public async Task<(bool ShouldProceed, string Details)> EvaluateAsync(ConditionalRule rule, List<Dictionary<string, object?>> contextData)
	{
		if (rule == null)
			return (true, "No hay regla condicional configurada. Se prosigue por defecto.");

		if (rule.Type == ConditionType.SqlRowCount)
		{
			int rowCount = contextData?.Count ?? 0;
			if (rule.DataSourceId.HasValue && !string.IsNullOrWhiteSpace(rule.SqlQuery) && rule.DataSource != null)
			{
				var customData = await _dataSourceService.FetchDataAsync(rule.DataSource, rule.SqlQuery);
				rowCount = customData.Count;
			}

			bool passed = rowCount >= rule.ExpectedMinCount;
			return (passed, $"SqlRowCount evaluado: {rowCount} filas obtenidas vs mínimo esperado {rule.ExpectedMinCount}. Resultado: {passed}");
		}

		if (rule.Type == ConditionType.LiquidExpression)
		{
			if (string.IsNullOrWhiteSpace(rule.Expression))
				return (true, "Expresión vacía, prosigue por defecto.");

			var model = new Dictionary<string, object?>
			{
				{ "items", contextData },
				{ "total_items", contextData?.Count ?? 0 },
				{ "total_pendientes", contextData?.Count ?? 0 }
			};

			if (contextData != null && contextData.Count > 0)
			{
				model["item"] = contextData.First();
				foreach (var kvp in contextData.First())
				{
					model[kvp.Key] = kvp.Value;
				}
			}

			// Template rendering logic for condition evaluation
			string templateWrapper = "{% if " + rule.Expression + " %}true{% else %}false{% endif %}";
			string renderedResult = await _templateRenderer.RenderAsync(templateWrapper, model);
			renderedResult = renderedResult.Trim().ToLowerInvariant();

			bool passed = renderedResult == "true" || renderedResult == "1" || renderedResult == "yes";
			return (passed, $"Expresión Liquid '{rule.Expression}' evaluada como: {renderedResult} ({passed})");
		}

		return (true, "Tipo de condición no reconocido, se prosigue por defecto.");
	}
}
