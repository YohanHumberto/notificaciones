using Domain.Contracts.Requests;
using Domain.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Persistence.Entities;
using System.Net;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataSourcesController(IDataSourceConfigService service) :
	BaseCrudController<DataSourceConfig, DataSourceConfig, DataSourceConfig>(service)
{

	[HttpPost("test")]
	public async Task<IActionResult> TestQuery([FromBody] TestQueryRequest req)
	{
		var response = await service.TestQuery(req);
		if (!response.Succeeded && response.Errors.Any(x => x.Code == HttpStatusCode.BadRequest))
			return BadRequest("Debe especificar una fuente de datos.");
		return Ok(response);
	}
}