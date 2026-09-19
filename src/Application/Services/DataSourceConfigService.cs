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
	/// <see cref="DataSourceConfig"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common service functionality from
	/// <see cref="BaseService{T}"/> and implements
	/// <see cref="IDataSourceConfigService"/>.
	/// </remarks>
	/// <param name="repository">
	/// The repository used to accessDataSourceConfig persistence data.
	/// </param>
	/// <param name="mapper">
	/// The AutoMapper instance used to map between domain and persistence models.
	/// </param>
	public class DataSourceConfigService(IDataSourceConfigRepository repository, IDataSourceService dataSourceService, IMapper mapper)
		: BaseService<DataSourceConfig>(repository, mapper), IDataSourceConfigService
	{
		public async Task<AppResponse<object>> TestQuery(TestQueryRequest req)
		{
			try
			{
				DataSourceConfig? ds = null;
				if (req.DataSourceId.HasValue)
				{
					ds = await repository.GetById(req.DataSourceId.Value);
				}
				else if (req.TempDataSource != null)
				{
					ds = req.TempDataSource;
				}

				if (ds == null)
					return new BadRequestResponse<object>(new
					{
						success = false,
						error = "Debe especificar una fuente de datos."
					}, "Debe especificar una fuente de datos.");

				var data = await dataSourceService.FetchDataAsync(ds, req.Query, req.ParametersJson);
				return new AppResponse<object>(new { success = true, rowCount = data.Count, data }, string.Empty);
			}
			catch (Exception ex)
			{
				return new InternalErrorResponse<object>(new { success = false, error = ex.Message }, string.Empty);
			}
		}
	}
}
