using Domain.Contracts.Requests;
using Domain.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Persistence.Entities;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController(
	INotificationTemplateService service)
	: BaseCrudController<NotificationTemplate, NotificationTemplate, NotificationTemplate>(service)
{

	[HttpPost("preview")]
	public async Task<IActionResult> PreviewTemplate([FromBody] PreviewTemplateRequest req)
	{
		return Ok(await service.PreviewTemplate(req));
	}
}

