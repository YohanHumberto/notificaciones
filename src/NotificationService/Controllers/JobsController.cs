using Domain.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Persistence.Entities;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController(INotificationJobService service) :
	BaseCrudController<NotificationJob, NotificationJob, NotificationJob>(service)
{
	[HttpPost("{id}/toggle")]
	public async Task<IActionResult> ToggleActive(int id)
	{
		var result = await service.ToggleActive(id);
		if (result == null) return NotFound();
		return Ok(new { isActive = result.IsActive });
	}

	[HttpPost("{id}/trigger")]
	public async Task<IActionResult> TriggerNow(int id)
	{
		var result = await service.TriggerNow(id);
		if (result == null) return NotFound();
		return Ok(result);
	}
}
