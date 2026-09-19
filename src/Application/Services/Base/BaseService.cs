using AutoMapper;
using CommonStructures.Contracts.Response;
using Domain.Contracts.Repository;
using Domain.Contracts.Services;
using Domain.Exceptions;
using System.Linq.Expressions;

namespace Application.Services.Base
{
	/// <summary>
	/// Provides a generic implementation of common CRUD operations for application services.
	/// </summary>
	/// <typeparam name="TModel">
	/// The entity type managed by the service.
	/// </typeparam>
	/// <param name="repository">
	/// Repository responsible for data access operations.
	/// </param>
	/// <param name="mapper">
	/// AutoMapper instance used to map between request/response DTOs and entities.
	/// </param>
	public abstract class BaseService<TModel>(IRepositoryBase<TModel> repository, IMapper mapper) : IBaseService
	{
		/// <inheritdoc/>
		public virtual async Task<AppResponse<TCreateResponse>> Create<TCreateRequest, TCreateResponse>(TCreateRequest request)
		{
			var model = mapper.Map<TModel>(request);
			var createResponse = await repository.AddAsync(model);

			TCreateResponse created = default!;

			if (createResponse.Succeeded)
				created = mapper.Map<TCreateResponse>(model);

			return createResponse.Succeeded switch
			{
				true => new AppSuccessResponse<TCreateResponse>(created, createResponse.Message),
				_ => new InternalErrorResponse<TCreateResponse>(created, createResponse.Message)
			};
		}

		/// <inheritdoc/>
		public virtual async Task<AppResponse<IReadOnlyList<TCreateResponse>>> CreateRange<TCreateRequest, TCreateResponse>(IReadOnlyList<TCreateRequest> request)
		{
			var model = mapper.Map<IReadOnlyList<TModel>>(request);
			var createResponse = await repository.AddRangeAsync(model);

			IReadOnlyList<TCreateResponse> created = default!;

			if (createResponse.Succeeded)
				created = mapper.Map<IReadOnlyList<TModel>, IReadOnlyList<TCreateResponse>>(model);

			return createResponse.Succeeded switch
			{
				true => new AppSuccessResponse<IReadOnlyList<TCreateResponse>>(created, createResponse.Message),
				_ => new InternalErrorResponse<IReadOnlyList<TCreateResponse>>(created, createResponse.Message)
			};
		}

		/// <inheritdoc/>
		public virtual async Task<AppResponse> Delete<TIdType>(TIdType id)
		{
			if (id is null) return new NullInfoResponse("Id");

			var model = await repository.GetById(id);
			if (model is null) return new IdNotFoundResponse<TIdType>(id);

			var deleteResponse = await repository.RemoveAsync(model);

			return deleteResponse;
		}

		/// <inheritdoc/>
		public virtual async Task<AppResponse> GetById<TIdType, TGetResponse>(TIdType id)
		{
			if (id is null) return new NullInfoResponse("Id");

			var model = await repository.GetById(id);
			if (model is null) return new IdNotFoundResponse<TIdType>(id);

			var response = mapper.Map<TModel, TGetResponse>(model);

			return new AppSuccessResponse<TGetResponse>(response);
		}

		/// <inheritdoc/>
		public virtual async Task<AppResponse<PagedResult<TGetResponse>>> FindByCondition<TGetResponse, T>(Expression<Func<T, bool>> expression, Domain.Common.Querying.PageRequest request)
		{
			try
			{
				var data = await repository.FindByCondition(expression as Expression<Func<TModel, bool>>, request);
				var response = mapper.Map<PagedResult<TModel>, PagedResult<TGetResponse>>(data);
				return new AppSuccessResponse<PagedResult<TGetResponse>>(response);
			}
			catch (InvalidFilterException e)
			{
				return BadRequest<PagedResult<TGetResponse>>(null, e.Message);
			}
			catch (NotSupportedException e)
			{
				return BadRequest<PagedResult<TGetResponse>>(null, e.Message);
			}
			catch (Exception e)
			{
				return new InternalErrorResponse<PagedResult<TGetResponse>>(null, e.Message);
			}
		}

		/// <inheritdoc/>
		public virtual async Task<AppResponse<IReadOnlyList<TGetResponse>>> FindByCondition<TGetResponse, T>(Expression<Func<T, bool>> expression)
		{
			try
			{
				var data = await repository.FindByCondition(expression as Expression<Func<TModel, bool>>);
				var response = mapper.Map<IReadOnlyList<TModel>, IReadOnlyList<TGetResponse>>(data);
				return new AppSuccessResponse<IReadOnlyList<TGetResponse>>(response);
			}
			catch (InvalidFilterException e)
			{
				return BadRequest<IReadOnlyList<TGetResponse>>(null, e.Message);
			}
			catch (NotSupportedException e)
			{
				return BadRequest<IReadOnlyList<TGetResponse>>(null, e.Message);
			}
			catch (Exception e)
			{
				return new InternalErrorResponse<IReadOnlyList<TGetResponse>>(null, e.Message);
			}
		}

		/// <inheritdoc/>
		public virtual async Task<AppResponse> Update<TUpdateRequest>(TUpdateRequest request)
		{
			if (request is null) return new NullInfoResponse("Update Request");

			var model = mapper.Map<TUpdateRequest, TModel>(request);
			var updateResponse = await repository.UpdateAsync(model);

			return updateResponse;
		}

		/// <inheritdoc/>
		public async Task<AppResponse> SetStatus<TIdType>(TIdType id, sbyte status)
		{
			if (id is null) return new NullInfoResponse("Id");

			var model = await repository.GetById(id);
			if (model is null) return new IdNotFoundResponse<TIdType>(id);

			var statusField = typeof(TModel).GetProperty("Status");
			if (statusField is null) return new NullInfoResponse("Status");

			statusField.SetValue(model, status);

			await repository.UpdateAsync(model);
			return new AppSuccessResponse<string>($"[{id}] status UPDATED to [{status}]");
		}

		/// <inheritdoc/>
		public virtual async Task<AppResponse<PagedResult<TGetResponse>>> ListAll<TGetResponse>(Domain.Common.Querying.PageRequest request)
		{
			try
			{
				var data = await repository.GetAll(request);
				var response = mapper.Map<PagedResult<TModel>, PagedResult<TGetResponse>>(data);
				response.MetaData = data.MetaData;
				return new AppSuccessResponse<PagedResult<TGetResponse>>(response);
			}
			catch (InvalidFilterException e)
			{
				return BadRequest<PagedResult<TGetResponse>>(null, e.Message);
			}
			catch (NotSupportedException e)
			{
				return BadRequest<PagedResult<TGetResponse>>(null, e.Message);
			}
			catch (Exception e)
			{
				return new InternalErrorResponse<PagedResult<TGetResponse>>(null, e.Message);
			}
		}

		internal BadRequestResponse<T> BadRequest<T>(T? value, string message)
		{
			return new BadRequestResponse<T>(value, string.Empty)
			{
				Message = "The Request is invalid",
				Errors = [new(System.Net.HttpStatusCode.BadRequest, message)]
			};
		}

	}
}
