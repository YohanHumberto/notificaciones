using CommonStructures.Contracts.Response;
using Domain.Contracts.Repository;
using Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repository.Base
{
	/// <summary>
	/// Provides common repository operations for entities that support soft deletion.
	/// </summary>
	/// <typeparam name="T">The entity type.</typeparam>
	/// <param name="dbContext">The database context used for data access.</param>
	public abstract class RepositoryBase<T>(DbContext dbContext) : IRepositoryBase<T> where T : class
	{
		/// <inheritdoc/>
		public async Task<AppResponse> AddAsync(T entity)
		{
			dbContext.Set<T>().Add(entity);
			_ = await dbContext.SaveChangesAsync();
			return new AppSuccessResponse<string>($"{entity.GetType().Name} Created Ok!");
		}

		/// <inheritdoc/>
		public async Task<AppResponse> AddRangeAsync(IReadOnlyList<T> entities)
		{
			dbContext.AddRange(entities);
			_ = await dbContext.SaveChangesAsync();
			return new AppSuccessResponse<string>($"{entities.GetType().Name} Created Ok!");
		}

		/// <inheritdoc/>
		public virtual async Task<AppResponse> RemoveAsync(T entity)
		{
			dbContext.Set<T>().Remove(entity);
			_ = await dbContext.SaveChangesAsync();
			return new AppSuccessResponse<string>($"{entity.GetType().Name} Deleted Ok!");
		}

		/// <inheritdoc/>
		public async Task<AppResponse> UpdateAsync(T entity)
		{
			dbContext.Set<T>().Update(entity);
			_ = await dbContext.SaveChangesAsync();
			return new AppSuccessResponse<string>($"{entity.GetType().Name} Updated Ok!");
		}

		/// <inheritdoc/>
		public async Task<PagedResult<T>> FindByCondition(Expression<Func<T, bool>> expression, Domain.Common.Querying.PageRequest request)
		{
			var data = RepositoryBase<T>.QueryByCondition(expression, dbContext);
			return await PagedResult<T>.ToPagedResult(data, request.PageNumber, request.PageSize);
		}

		/// <inheritdoc/>
		public async Task<IReadOnlyList<T>> FindByCondition(Expression<Func<T, bool>> expression)
		{
			var data = RepositoryBase<T>.QueryByCondition(expression, dbContext);
			return await data.ToListAsync();
		}

		/// <inheritdoc/>
		public virtual async Task<PagedResult<T>> GetAll(Domain.Common.Querying.PageRequest request)
		{
			var data = dbContext.Set<T>().AsNoTracking()
				.ApplyFilters(request?.Filters?? [])
				.ApplySort(request?.SortOrder?? []);
			return await PagedResult<T>.ToPagedResult(data, request.PageNumber, request.PageSize);
		}

		/// <inheritdoc/>
		public virtual async Task<T?> GetById<TIdType>(TIdType id)
		{
			var result = await dbContext.Set<T>().FindAsync(id);
			return result is null ? null : result;
		}

		/// <inheritdoc/>
		private static IQueryable<T> QueryByCondition(Expression<Func<T, bool>> expression, DbContext context)
		{
			return context.Set<T>()
				.Where(expression)
				.AsNoTracking();
		}
	}
}
