using Application.Services.Base;
using AutoMapper;
using Domain.Contracts.Repository;
using Domain.Contracts.Services;
using Persistence.Entities;

namespace Application.Services
{
	/// <summary>
	/// Provides the service implementation for
	/// <see cref="PostExecutionAction"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common service functionality from
	/// <see cref="BaseService{T}"/> and implements
	/// <see cref="IPostExecutionActionService"/>.
	/// </remarks>
	/// <param name="repository">
	/// The repository used to accessPostExecutionAction persistence data.
	/// </param>
	/// <param name="mapper">
	/// The AutoMapper instance used to map between domain and persistence models.
	/// </param>
	public class PostExecutionActionService(IPostExecutionActionRepository repository, IMapper mapper)
		: BaseService<PostExecutionAction>(repository, mapper), IPostExecutionActionService
	{
	}
}
