using Domain.Contracts.Requests;
using Domain.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Persistence.Entities;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConditionsController(IConditionalRuleService service)
	: BaseCrudController<ConditionalRule, ConditionalRule, ConditionalRule>(service)
{

	[HttpPost("test")]
	public async Task<IActionResult> TestCondition([FromBody] TestConditionRequest req)
	{
		return Ok(await service.TestCondition(req));
	}
}
