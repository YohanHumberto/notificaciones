using CommonStructures.Contracts.Response;
using System.Linq.Expressions;

namespace Domain.Contracts.Repository
{
	/// <summary>
	/// Defines the base contract for repository implementations that provide
	/// common data access operations for a specific entity type.
	/// </summary>
	/// <typeparam name="T">
	/// The entity type managed by the repository.
	/// </typeparam>
	/// <remarks>
	/// Implementations are responsible for performing CRUD operations,
	/// querying entities, and supporting paged data retrieval while
	/// returning standardized application responses.
	/// </remarks>
	public interface IRepositoryBase<T>
	{
		/// <summary>
		/// Adds a new entity to the data store.
		/// </summary>
		Task<AppResponse> AddAsync(T entity);

		/// <summary>
		/// Adds multiple entities to the data store.
		/// </summary>
		Task<AppResponse> AddRangeAsync(IReadOnlyList<T> entities);

		/// <summary>
		/// Removes an entity from the data store.
		/// </summary>
		Task<AppResponse> RemoveAsync(T entity);

		/// <summary>
		/// Updates an existing entity in the data store.
		/// </summary>
		Task<AppResponse> UpdateAsync(T entity);

		/// <summary>
		/// Retrieves all entities matching the specified condition.
		/// </summary>
		Task<IReadOnlyList<T>> FindByCondition(Expression<Func<T, bool>> expression);

		/// <summary>
		/// Retrieves a paged collection of entities matching the specified condition.
		/// </summary>
		Task<PagedResult<T>> FindByCondition(Expression<Func<T, bool>> expression, Domain.Common.Querying.PageRequest request);

		/// <summary>
		/// Retrieves a paged collection of all entities.
		/// </summary>
		Task<PagedResult<T>> GetAll(Domain.Common.Querying.PageRequest request);

		/// <summary>
		/// Retrieves an entity by its identifier.
		/// </summary>
		Task<T?> GetById<TId>(TId id);
	}
}
