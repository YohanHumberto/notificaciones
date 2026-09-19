using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Entities;
using Persistence.Enums;
using System.Diagnostics;

namespace Service.Jobs;

public class NotificationJobProcessor : INotificationJobProcessor
{
	private readonly NotificationContext _dbContext;
	private readonly IDataSourceService _dataSourceService;
	private readonly IConditionEvaluator _conditionEvaluator;
	private readonly ITemplateRenderer _templateRenderer;
	private readonly IEnumerable<INotificationChannelProvider> _channelProviders;
	private readonly IPostExecutionProcessor _postExecutionProcessor;

	public NotificationJobProcessor(
		NotificationContext dbContext,
		IDataSourceService dataSourceService,
		IConditionEvaluator conditionEvaluator,
		ITemplateRenderer templateRenderer,
		IEnumerable<INotificationChannelProvider> channelProviders,
		IPostExecutionProcessor postExecutionProcessor)
	{
		_dbContext = dbContext;
		_dataSourceService = dataSourceService;
		_conditionEvaluator = conditionEvaluator;
		_templateRenderer = templateRenderer;
		_channelProviders = channelProviders;
		_postExecutionProcessor = postExecutionProcessor;
	}

	public async Task<ExecutionLog> ProcessJobAsync(int jobId)
	{
		var stopwatch = Stopwatch.StartNew();

		var job = await _dbContext.Jobs
			.Include(j => j.Channel)
			.Include(j => j.Template)
			.Include(j => j.DataSource)
			.Include(j => j.ConditionalRule)
				.ThenInclude(r => r!.DataSource)
			.Include(j => j.PostExecutionAction)
				.ThenInclude(p => p!.DataSource)
			.FirstOrDefaultAsync(j => j.Id == jobId);

		if (job == null)
		{
			throw new InvalidOperationException($"El Job con ID {jobId} no existe.");
		}

		var log = new ExecutionLog
		{
			JobId = job.Id,
			JobName = job.Name,
			TriggeredAt = DateTime.UtcNow,
			Status = ExecutionStatus.Success
		};

		try
		{
			// 1. Fetch data context from Data Source if specified
			List<Dictionary<string, object?>> contextData = new();
			if (job.DataSourceId.HasValue && job.DataSource != null && !string.IsNullOrWhiteSpace(job.DataQuery))
			{
				contextData = await _dataSourceService.FetchDataAsync(job.DataSource, job.DataQuery, job.QueryParametersJson);
			}
			log.DataRowsFetched = contextData.Count;

			var model = new Dictionary<string, object?>
			{
				{ "items", contextData },
				{ "total_items", contextData.Count },
				{ "total_pendientes", contextData.Count }
			};

			if (contextData.Count > 0)
			{
				model["item"] = contextData.First();
				foreach (var kvp in contextData.First())
				{
					model[kvp.Key] = kvp.Value;
				}
			}

			// 2. Evaluate Conditional Rule if configured
			if (job.ConditionalRuleId.HasValue && job.ConditionalRule != null)
			{
				log.ConditionEvaluated = true;
				var (shouldProceed, conditionDetails) = await _conditionEvaluator.EvaluateAsync(job.ConditionalRule, contextData);
				log.ConditionResult = shouldProceed;

				if (!shouldProceed)
				{
					log.Status = ExecutionStatus.SkippedCondition;
					log.ErrorMessage = $"Notificación omitida por condicional. Detalle: {conditionDetails}";
					stopwatch.Stop();
					log.DurationMs = stopwatch.ElapsedMilliseconds;

					job.LastRunAt = DateTime.UtcNow;
					_dbContext.ExecutionLogs.Add(log);
					await _dbContext.SaveChangesAsync();
					return log;
				}
			}

			// 3. Render Recipient, Subject, and Body using Fluid Liquid Engine
			if (job.Template == null)
			{
				throw new InvalidOperationException("El Job no tiene asignada ninguna Plantilla (Template).");
			}

			string renderedRecipient = await _templateRenderer.RenderAsync(job.RecipientExpression, model);
			string renderedSubject = await _templateRenderer.RenderAsync(job.Template.SubjectTemplate, model);
			string renderedBody = await _templateRenderer.RenderAsync(job.Template.BodyTemplate, model);

			log.TargetRecipient = renderedRecipient;
			log.RenderedSubject = renderedSubject;
			log.RenderedBody = renderedBody;

			// 4. Send via Channel Provider
			if (job.Channel == null)
			{
				throw new InvalidOperationException("El Job no tiene asignado un Canal de Notificación.");
			}

			var provider = _channelProviders.FirstOrDefault(p => p.ChannelType == job.Channel.Type);
			if (provider == null)
			{
				throw new InvalidOperationException($"No hay un proveedor registrado para el tipo de canal {job.Channel.Type}.");
			}

			var (sendSuccess, sendMsg) = await provider.SendNotificationAsync(job.Channel, renderedRecipient, renderedSubject, renderedBody);

			if (!sendSuccess)
			{
				log.Status = ExecutionStatus.Failed;
				log.ErrorMessage = sendMsg;
			}
			else
			{
				// 5. Execute Post-Action if configured
				if (job.PostExecutionActionId.HasValue && job.PostExecutionAction != null)
				{
					string postResult = await _postExecutionProcessor.ExecuteActionAsync(job.PostExecutionAction, contextData);
					log.PostExecutionDetails = postResult;
				}
			}
		}
		catch (Exception ex)
		{
			log.Status = ExecutionStatus.Failed;
			log.ErrorMessage = $"Error inesperado en ejecución: {ex.Message}";
		}
		finally
		{
			stopwatch.Stop();
			log.DurationMs = stopwatch.ElapsedMilliseconds;

			job.LastRunAt = DateTime.UtcNow;
			_dbContext.ExecutionLogs.Add(log);
			await _dbContext.SaveChangesAsync();
		}

		return log;
	}
}
