using Application.Services.Base;
using AutoMapper;
using CommonStructures.Contracts.Response;
using Domain.Contracts.Repository;
using Domain.Contracts.Requests;
using Domain.Contracts.Services;
using Domain.Interfaces;
using Persistence.Entities;

namespace Application.Services
{
	/// <summary>
	/// Provides the service implementation for
	/// <see cref="ConditionalRule"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common service functionality from
	/// <see cref="BaseService{T}"/> and implements
	/// <see cref="IConditionalRuleService"/>.
	/// </remarks>
	/// <param name="repository">
	/// The repository used to accessConditionalRule persistence data.
	/// </param>
	/// <param name="mapper">
	/// The AutoMapper instance used to map between domain and persistence models.
	/// </param>
	public class ConditionalRuleService(IConditionalRuleRepository repository,
		IDataSourceConfigRepository dataSourceConfigRepository,
		IDataSourceService dataSourceService,
		IConditionEvaluator conditionEvaluator,
		IMapper mapper)
		: BaseService<ConditionalRule>(repository, mapper), IConditionalRuleService
	{
		public async Task<AppResponse<object>> TestCondition(TestConditionRequest req)
		{
			try
			{
				List<Dictionary<string, object?>> context = [];
				if (req.Rule.DataSourceId.HasValue && !string.IsNullOrWhiteSpace(req.Rule.SqlQuery))
				{
					var ds = await dataSourceConfigRepository.GetById(req.Rule.DataSourceId.Value);
					if (ds != null)
					{
						req.Rule.DataSource = ds;
						context = await dataSourceService.FetchDataAsync(ds, req.Rule.SqlQuery);
					}
				}

				var (shouldProceed, details) = await conditionEvaluator.EvaluateAsync(req.Rule, context);
				return new AppResponse<object>(new { success = true, shouldProceed, details }, string.Empty);
			}
			catch (Exception ex)
			{
				return new InternalErrorResponse<object>(new { success = false, error = ex.Message }, string.Empty);
			}
		}
	}
}
