using CommonStructures.Contracts.Response;
using Domain.Contracts.Repository;
using Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Entities;
using Repository.Base;
namespace Repository.Repositories
{
	/// <summary>
	/// Provides the repository implementation for
	/// <see cref="NotificationJob"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common repository functionality from
	/// <see cref="RepositoryBase{T}"/> and serves as the extension point
	/// for NotificationJob-specific data access operations.
	/// </remarks>
	/// <param name="dbContext">
	/// The AVM database context used to access persistence data.
	/// </param>
	public class NotificationJobRepository(NotificationContext dbContext)
		: RepositoryBase<NotificationJob>(dbContext), INotificationJobRepository
	{
		/// <inheritdoc/>
		public override async Task<PagedResult<NotificationJob>> GetAll(Domain.Common.Querying.PageRequest request)
		{
			var data = dbContext.Set<NotificationJob>()
				.Include(j => j.Channel)
				.Include(j => j.Template)
				.Include(j => j.DataSource)
				.Include(j => j.ConditionalRule)
				.Include(j => j.PostExecutionAction)
				.AsNoTracking()
				.ApplyFilters(request?.Filters?? [])
				.ApplySort(request?.SortOrder?? []);
			return await PagedResult<NotificationJob>.ToPagedResult(data, request.PageNumber, request.PageSize);
		}

		/// <inheritdoc/>
		public override async Task<NotificationJob?> GetById<TIdType>(TIdType id)
		{
			return await dbContext.Set<NotificationJob>()
				.Include(j => j.Channel)
				.Include(j => j.Template)
				.Include(j => j.DataSource)
				.Include(j => j.ConditionalRule)
				.Include(j => j.PostExecutionAction)
				.FirstOrDefaultAsync(x => x.Id!.Equals(id));
		}
	}
}
