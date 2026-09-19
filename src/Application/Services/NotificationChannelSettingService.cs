using Application.Services.Base;
using AutoMapper;
using Domain.Contracts.Repository;
using Domain.Contracts.Services;
using Persistence.Entities;

namespace Application.Services
{
	/// <summary>
	/// Provides the service implementation for
	/// <see cref="NotificationChannelSetting"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common service functionality from
	/// <see cref="BaseService{T}"/> and implements
	/// <see cref="INotificationChannelSettingService"/>.
	/// </remarks>
	/// <param name="repository">
	/// The repository used to accessNotificationChannelSetting persistence data.
	/// </param>
	/// <param name="mapper">
	/// The AutoMapper instance used to map between domain and persistence models.
	/// </param>
	public class NotificationChannelSettingService(INotificationChannelSettingRepository repository, IMapper mapper)
		: BaseService<NotificationChannelSetting>(repository, mapper), INotificationChannelSettingService
	{
	}
}
