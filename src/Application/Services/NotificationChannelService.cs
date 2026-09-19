using Application.Services.Base;
using AutoMapper;
using CommonStructures.Contracts.Common;
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
	/// <see cref="NotificationChannel"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common service functionality from
	/// <see cref="BaseService{T}"/> and implements
	/// <see cref="INotificationChannelService"/>.
	/// </remarks>
	/// <param name="repository">
	/// The repository used to accessNotificationChannel persistence data.
	/// </param>
	/// <param name="mapper">
	/// The AutoMapper instance used to map between domain and persistence models.
	/// </param>
	public class NotificationChannelService(INotificationChannelRepository repository,
		INotificationSettingDefinitionRepository settingDefinitionRepository,
		INotificationChannelConfigurationService _configurationService,
	INotificationChannelTypeService channelTypeService,
	IEnumerable<INotificationChannelProvider> providers,
	IMapper mapper)
		: BaseService<NotificationChannel>(repository, mapper), INotificationChannelService
	{

		public async Task<List<ChannelTypeResponse>> GetTypes()
		{
			var types = (await channelTypeService.ListAll<ChannelTypeResponse>(new Domain.Common.Querying.PageRequest()
			{
				SortOrder = [new SortRequest() { Direction = SortDirection.Asc, Field = "Name" }],
				Filters = [new Domain.Common.Querying.Filter() { Field = "", Operator = FilterOperations.Equals, Value = "True" }]
			})).Value.ToList();

			return types;
		}

		public async Task<AppResponse<List<NotificationSettingDefinition>>> GetDefinitions(int typeId)
		{
			var response = await settingDefinitionRepository.GetDefinitions(typeId);
			if (response is null) return new DataNotFoundResponse<List<NotificationSettingDefinition>>(null, "Type");
			return new AppSuccessResponse<List<NotificationSettingDefinition>>(response);
		}

		public async Task<AppResponse<object>> GetById(int id)
		{
			var channel = await repository.GetById(id);
			if (channel == null) return new DataNotFoundResponse<object>(null, "Channel");

			var settings = await _configurationService.GetSettingsAsync(id);
			return new AppSuccessResponse<object>(new
			{
				channel.Id,
				channel.Name,
				channel.Type,
				channel.ChannelTypeId,
				channel.IsActive,
				channel.CreatedAt,
				channel.UpdatedAt,
				settings
			});
		}

		public async Task<AppResponse<object>> GetSettings(int id)
		{
			var channel = await repository.GetById(id);
			if (channel == null) return new DataNotFoundResponse<object>(null, "Channel");

			var settings = await _configurationService.GetSettingsAsync(id);
			return new AppSuccessResponse<object>(settings);
		}

		public async Task<AppResponse<object>> Validate(int id)
		{
			var errors = await _configurationService.ValidateAsync(id);
			if (errors.Count == 1 && errors[0] == "El canal no existe.") return new DataNotFoundResponse<object>(null, "Channel");
			return new AppSuccessResponse<object>(new { valid = errors.Count == 0, errors });
		}

		public async Task<AppResponse<object>> UpsertSetting(int id, string code, ChannelSettingUpdateRequest request)
		{
			var channel = await repository.GetById(id);
			if (channel == null) return new DataNotFoundResponse<object>(null, "Channel");

			if (request.Delete)
			{
				await _configurationService.DeleteSettingAsync(id, code);
				return new AppResponse<object>(new { success = true, code }, string.Empty);
			}

			await _configurationService.SaveSettingAsync(id, code, request.Value, false);
			return new AppResponse<object>(new { success = true, code }, string.Empty);
		}

		public async Task<AppResponse<object>> TestChannel(int id, TestChannelRequest req)
		{
			var channel = await repository.GetById(id);
			if (channel == null) return new DataNotFoundResponse<object>(null, "Channel");

			var provider = System.Linq.Enumerable.FirstOrDefault(providers, p => p.ChannelType == channel.Type);
			if (provider == null) return new BadRequestResponse<object>(new
			{
				success = false,
				error = "Proveedor no registrado para este canal."
			}, "Proveedor no registrado para este canal.");

			var (success, message) = await provider.SendNotificationAsync(channel, req.Recipient, req.Subject ?? "Prueba de Canal", req.Body ?? "<p>Prueba de envío</p>");
			return new AppResponse<object>(new { success, message }, message);
		}

	}
}
