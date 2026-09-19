//using AvmPersistenceModel.Contracts.Common;
//using CommonStructures.Contracts.Response;
//using Infrastructure.Authorization.Extensions;
//using Microsoft.EntityFrameworkCore;

//namespace Repository.Base
//{
//	/// <summary>
//	/// Provides repository operations for entities that support soft deletion.
//	/// </summary>
//	/// <typeparam name="T">The entity type.</typeparam>
//	/// <param name="dbContext">The database context used for data access.</param>
//	public abstract class SoftDeleteRepositoryBase<T>(DbContext dbContext)
//		: RepositoryBase<T>(dbContext: dbContext)
//		where T : class, ISoftDeletable
//	{
//		/// <inheritdoc/>
//		public override async Task<AppResponse> RemoveAsync(T entity)
//		{
//			entity.Deleted = true;

//			dbContext.Set<T>().Update(entity);
//			_ = await dbContext.SaveChangesAsync();

//			return new AppSuccessResponse<string>(
//				$"{entity.GetType().Name} Deleted Ok!");
//		}

//		/// <inheritdoc/>
//		public override async Task<T?> GetById<TIdType>(TIdType id)
//		{
//			var result = await dbContext.Set<T>()
//				.FindAsync(id);

//			return result is null || result.Deleted
//				? null
//				: result;
//		}

//		/// <inheritdoc/>
//		public override async Task<PagedResult<T>> GetAll(Domain.Common.Querying.PageRequest request)
//		{
//			var data = dbContext.Set<T>()
//				.Where(x => !x.Deleted)
//				.AsNoTracking()
//				.ApplyFilters(request.Filters)
//				.ApplySort(request.SortOrder);

//			return await PagedResult<T>.ToPagedResult(
//				data,
//				request.PageNumber,
//				request.PageSize);
//		}
//	}
//}