using CommonStructures.Contracts.Response;
using Domain.Contracts.Requests;

namespace Domain.Contracts.Services
{
	/// <summary>
	/// Defines the contract for application services that manage
	/// <see cref="NotificationTemplate"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the standard CRUD, query, and paging operations from
	/// <see cref="IBaseService"/> and can be extended with
	/// audit-specific business operations as needed.
	/// </remarks>
	public interface INotificationTemplateService : IBaseService
	{
		Task<AppResponse<object>> PreviewTemplate(PreviewTemplateRequest req);
	}
}