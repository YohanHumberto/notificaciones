using Application.Services.Base;
using AutoMapper;
using Domain.Contracts.Repository;
using Domain.Contracts.Services;
using Persistence.Entities;

namespace Application.Services
{
	/// <summary>
	/// Provides the service implementation for
	/// <see cref="NotificationChannelType"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common service functionality from
	/// <see cref="BaseService{T}"/> and implements
	/// <see cref="INotificationChannelTypeService"/>.
	/// </remarks>
	/// <param name="repository">
	/// The repository used to accessNotificationChannelType persistence data.
	/// </param>
	/// <param name="mapper">
	/// The AutoMapper instance used to map between domain and persistence models.
	/// </param>
	public class NotificationChannelTypeService(INotificationChannelTypeRepository repository, IMapper mapper)
		: BaseService<NotificationChannelType>(repository, mapper), INotificationChannelTypeService
	{
	}
}
