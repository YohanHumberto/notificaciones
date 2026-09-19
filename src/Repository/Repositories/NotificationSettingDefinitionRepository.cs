using Domain.Contracts.Repository;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Entities;
using Repository.Base;

namespace Repository.Repositories
{
	/// <summary>
	/// Provides the repository implementation for
	/// <see cref="NotificationSettingDefinition"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common repository functionality from
	/// <see cref="RepositoryBase{T}"/> and serves as the extension point
	/// for NotificationSettingDefinition-specific data access operations.
	/// </remarks>
	/// <param name="avmContext">
	/// The AVM database context used to access persistence data.
	/// </param>
	public class NotificationSettingDefinitionRepository(NotificationContext dbContext)
		: RepositoryBase<NotificationSettingDefinition>(dbContext), INotificationSettingDefinitionRepository
	{

		public async Task<List<NotificationSettingDefinition>> GetDefinitions(int typeId)
		{
			var typeExists = await dbContext.ChannelTypes.AnyAsync(type => type.Id == typeId && type.Enabled);
			if (!typeExists) return null;

			return await dbContext.SettingDefinitions
				.AsNoTracking()
				.Where(definition => definition.ChannelTypeId == typeId && definition.Enabled)
				.OrderBy(definition => definition.Id)
				//.Select(x =>
				//{
				//	x.DataType = x.DataType.ToString().ToUpperInvariant();
				//	return x;
				//})
				.ToListAsync();

			//return definitions.Select(definition => new
			//{
			//	definition.Code,
			//	definition.Name,
			//	DataType = definition.DataType.ToString().ToUpperInvariant(),
			//	definition.IsRequired,
			//	definition.IsSensitive,
			//	definition.DefaultValue
			//});
		}
	}
}
