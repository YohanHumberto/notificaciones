using CommonStructures.Contracts.Response;
using Domain.Contracts.Requests;
using Persistence.Entities;

namespace Domain.Contracts.Services
{
	/// <summary>
	/// Defines the contract for application services that manage
	/// <see cref="NotificationChannel"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the standard CRUD, query, and paging operations from
	/// <see cref="IBaseService"/> and can be extended with
	/// audit-specific business operations as needed.
	/// </remarks>
	public interface INotificationChannelService : IBaseService
	{
		Task<List<ChannelTypeResponse>> GetTypes();
		Task<AppResponse<List<NotificationSettingDefinition>>> GetDefinitions(int typeId);
		Task<AppResponse<object>> GetById(int id);
		Task<AppResponse<object>> GetSettings(int id);
		Task<AppResponse<object>> Validate(int id);
		Task<AppResponse<object>> UpsertSetting(int id, string code, ChannelSettingUpdateRequest request);
		Task<AppResponse<object>> TestChannel(int id, TestChannelRequest req);
	}
}