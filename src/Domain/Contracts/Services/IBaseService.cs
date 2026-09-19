using CommonStructures.Contracts.Response;
using System.Linq.Expressions;

namespace Domain.Contracts.Services
{
	/// <summary>
	/// Defines the contract for application services that provide common CRUD operations,
	/// entity retrieval, filtering, paging, and status management.
	/// </summary>
	/// <remarks>
	/// Implementations are responsible for coordinating repository operations,
	/// mapping between domain models and DTOs, and returning standardized
	/// <see cref="AppResponse"/> results.
	/// </remarks>
	public interface IBaseService
	{

		/// <summary>
		/// Creates a new entity from the specified request model.
		/// </summary>
		/// <typeparam name="TCreateRequest">The request DTO type.</typeparam>
		/// <typeparam name="TCreateResponse">The response DTO type.</typeparam>
		/// <param name="request">The data used to create the entity.</param>
		/// <returns>
		/// A successful response containing the created entity, or an error response if the operation fails.
		/// </returns>
		Task<AppResponse<TCreateResponse>> Create<TCreateRequest, TCreateResponse>(TCreateRequest request);

		/// <summary>
		/// Creates multiple entities from the specified request models.
		/// </summary>
		/// <typeparam name="TCreateRequest">The request DTO type.</typeparam>
		/// <typeparam name="TCreateResponse">The response DTO type.</typeparam>
		/// <param name="request">The collection of request models.</param>
		/// <returns>
		/// A successful response containing the created entities, or an error response if the operation fails.
		/// </returns>
		Task<AppResponse<IReadOnlyList<TCreateResponse>>> CreateRange<TCreateRequest, TCreateResponse>(IReadOnlyList<TCreateRequest> request);

		/// <summary>
		/// Deletes an entity identified by its identifier.
		/// </summary>
		/// <typeparam name="TIdType">The identifier type.</typeparam>
		/// <param name="id">The entity identifier.</param>
		/// <returns>
		/// A response indicating whether the entity was successfully deleted.
		/// </returns>
		Task<AppResponse> Delete<TIdType>(TIdType id);

		/// <summary>
		/// Retrieves all entities matching the specified condition.
		/// </summary>
		/// <typeparam name="TGetResponse">The response DTO type.</typeparam>
		/// <typeparam name="T">The entity type used in the filter expression.</typeparam>
		/// <param name="expression">The filter expression.</param>
		/// <returns>
		/// A collection of matching entities or a not-found response.
		/// </returns>
		Task<AppResponse<IReadOnlyList<TGetResponse>>> FindByCondition<TGetResponse, T>(Expression<Func<T, bool>> expression);

		/// <summary>
		/// Retrieves a paged collection of entities matching the specified condition.
		/// </summary>
		/// <typeparam name="TGetResponse">The response DTO type.</typeparam>
		/// <typeparam name="T">The entity type used in the filter expression.</typeparam>
		/// <param name="expression">The filter expression.</param>
		/// <param name="request">The paging request.</param>
		/// <returns>
		/// A paged collection of matching entities or a not-found response.
		/// </returns>
		Task<AppResponse<PagedResult<TGetResponse>>> FindByCondition<TGetResponse, T>(Expression<Func<T, bool>> expression, Domain.Common.Querying.PageRequest request);

		/// <summary>
		/// Retrieves an entity by its identifier.
		/// </summary>
		/// <typeparam name="TIdType">The identifier type.</typeparam>
		/// <typeparam name="TGetResponse">The response DTO type.</typeparam>
		/// <param name="id">The entity identifier.</param>
		/// <returns>
		/// A response containing the requested entity if found; otherwise, an appropriate error response.
		/// </returns>
		Task<AppResponse> GetById<TIdType, TGetResponse>(TIdType id);

		/// <summary>
		/// Retrieves a paged list of all entities.
		/// </summary>
		/// <typeparam name="TGetResponse">The response DTO type.</typeparam>
		/// <param name="request">The paging request.</param>
		/// <returns>
		/// A paged collection of entities or a not-found response when no records exist.
		/// </returns>
		Task<AppResponse<PagedResult<TGetResponse>>> ListAll<TGetResponse>(Domain.Common.Querying.PageRequest request);


		/// <summary>
		/// Updates the status value of an entity.
		/// </summary>
		/// <typeparam name="TIdType">The identifier type.</typeparam>
		/// <param name="id">The entity identifier.</param>
		/// <param name="status">The new status value.</param>
		/// <returns>
		/// A response indicating whether the status was successfully updated.
		/// </returns>
		Task<AppResponse> SetStatus<TIdType>(TIdType id, sbyte status);

		/// <summary>
		/// Updates an existing entity.
		/// </summary>
		/// <typeparam name="TUpdateRequest">The update request DTO type.</typeparam>
		/// <param name="request">The data used to update the entity.</param>
		/// <returns>
		/// A response indicating whether the update operation succeeded.
		/// </returns>
		Task<AppResponse> Update<TUpdateRequest>(TUpdateRequest request);
	}
}