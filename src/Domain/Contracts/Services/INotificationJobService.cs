using Persistence.Entities;

namespace Domain.Contracts.Services
{
	/// <summary>
	/// Defines the contract for application services that manage
	/// <see cref="NotificationJob"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the standard CRUD, query, and paging operations from
	/// <see cref="IBaseService"/> and can be extended with
	/// audit-specific business operations as needed.
	/// </remarks>
	public interface INotificationJobService : IBaseService
	{
		Task<ExecutionLog> TriggerNow(int id);
		Task<NotificationJob> ToggleActive(int id);
	}
}