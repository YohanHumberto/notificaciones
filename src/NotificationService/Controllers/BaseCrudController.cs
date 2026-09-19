using AvmSystemApi.Controllers;
using CommonStructures.Contracts.Response;
using Domain.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Web.Controllers
{
	/// <summary>
	///     Generic controller with basic CRUD operations
	/// </summary>
	/// <typeparam name="TList"></typeparam>
	/// <typeparam name="TCreate"></typeparam>
	/// <typeparam name="TUpdate"></typeparam>
	/// <remarks>
	///     Constructor with service injection
	/// </remarks>
	/// <param name="service"></param>
	public class BaseCrudController<TList, TCreate, TUpdate>(IBaseService service) : BaseController
	{
		/// <summary>
		///     Return a list of elements in a <see cref="PagedResult{T}" /> response
		/// </summary>
		/// <param name="request"></param>
		/// <see cref="Domain.Common.Querying.PageRequest" />
		/// <returns></returns>
		[HttpGet]
		//[HasPermission(PermissionAction.Read)]
		public virtual async Task<IActionResult> ListAll([FromQuery] Domain.Common.Querying.PageRequest request)
		{
			var response = await service.ListAll<TList>(request);

			if (!response.Succeeded) return ProcessResponse<int>(response);
			SetPaginationHeader<TList>(response);
			return Ok(response);
		}

		/// <summary>
		///     Get element by Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet("{id}")]
		//[HasPermission(PermissionAction.Read)]
		public virtual async Task<IActionResult> Get(Guid id)
		{
			var response = await service.GetById<Guid, TList>(id);
			return ProcessResponse<int>(response);
		}

		/// <summary>
		///     Create new element
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		//[HasPermission(PermissionAction.Create)]
		public virtual async Task<IActionResult> Create([FromBody] TCreate request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var response = await service.Create<TCreate, TList>(request);
			return !response.Succeeded ? ProcessResponse<int>(response) : Created("BaseCreate", response);
		}

		/// <summary>
		///     Create elements from a list
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost("AddRange")]
		//[HasPermission(PermissionAction.Create)]
		public virtual async Task<IActionResult> CreateRange([FromBody] IReadOnlyList<TCreate> request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var response = await service.CreateRange<TCreate, TList>(request);
			return !response.Succeeded ? ProcessResponse<int>(response) : Created("BaseCreate", response);
		}

		/// <summary>
		///     Update element
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPut]
		//[HasPermission(PermissionAction.Update)]
		public virtual async Task<IActionResult> Update(TUpdate request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var response = await service.Update(request);
			return ProcessResponse<int>(response);
		}

		/// <summary>
		///     Delete element from Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpDelete("{id}")]
		//[HasPermission(PermissionAction.Delete)]
		public virtual async Task<IActionResult> Delete(Guid id)
		{
			var response = await service.Delete(id);
			return ProcessResponse<int>(response);
		}
	}

}
