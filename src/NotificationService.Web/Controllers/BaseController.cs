using CommonStructures.Contracts.Response;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvmSystemApi.Controllers
{
	/// <summary>
	///     Base Controller Class
	/// </summary>
	public class BaseController : Controller
	{
		/// <summary>
		///     Process responses from AppResponse type methods
		/// </summary>
		/// <param name="baseResponse"></param>
		/// <typeparam name="TIdType"></typeparam>
		/// <returns></returns>
		public IActionResult ProcessResponse<TIdType>(AppResponse baseResponse)
		{
			if (baseResponse.Succeeded) return Ok(baseResponse);

			var code = (int)baseResponse.Errors[0].Code;
			return StatusCode(code, baseResponse);
		}

		/// <summary>
		///     Sets the pagination metadata in the response headers.
		/// </summary>
		/// <typeparam name="T">The type of the paginated items.</typeparam>
		/// <param name="response">The application response containing pagination metadata.</param>
		protected void SetPaginationHeader<T>(AppResponse response)
		{
			if (!response.Succeeded)
				return;

			var pagedResponse = (AppSuccessResponse<PagedResult<T>>)response;
			Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(pagedResponse.Value.MetaData));
		}
	}

}
