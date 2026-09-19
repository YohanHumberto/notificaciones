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
	/// <see cref="ConditionalRule"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common repository functionality from
	/// <see cref="RepositoryBase{T}"/> and serves as the extension point
	/// for ConditionalRule-specific data access operations.
	/// </remarks>
	/// <param name="dbContext">
	/// The AVM database context used to access persistence data.
	/// </param>
	public class ConditionalRuleRepository(NotificationContext dbContext)
		: RepositoryBase<ConditionalRule>(dbContext), IConditionalRuleRepository
	{
		/// <inheritdoc/>
		public override async Task<PagedResult<ConditionalRule>> GetAll(Domain.Common.Querying.PageRequest request)
		{
			var data = dbContext.Set<ConditionalRule>().Include(x => x.DataSource).AsNoTracking()
				.ApplyFilters(request?.Filters?? [])
				.ApplySort(request?.SortOrder?? []);
			return await PagedResult<ConditionalRule>.ToPagedResult(data, request.PageNumber, request.PageSize);
		}

		/// <inheritdoc/>
		public override async Task<ConditionalRule?> GetById<TIdType>(TIdType id)
		{
			return await dbContext.Set<ConditionalRule>()
				.Include(x => x.DataSource)
				.FirstOrDefaultAsync(x => x.Id!.Equals(id));
		}
	}
}
