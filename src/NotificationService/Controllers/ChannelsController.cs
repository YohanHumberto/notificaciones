using Domain.Contracts.Requests;
using Domain.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Persistence.Entities;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChannelsController(INotificationChannelService channelService) :
	BaseCrudController<NotificationChannel, NotificationChannel, NotificationChannel>(channelService)
{

	[HttpGet("types")]
	public async Task<IActionResult> GetTypes()
	{
		var response = await channelService.GetTypes();
		return Ok(response);
	}

	[HttpGet("types/{typeId}/definitions")]
	public async Task<IActionResult> GetDefinitions(int typeId)
	{
		var response = await channelService.GetDefinitions(typeId);
		return ProcessResponse<int>(response);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(int id)
	{
		var response = await channelService.GetById(id);
		return ProcessResponse<int>(response);
	}

	[HttpGet("{id}/settings")]
	public async Task<IActionResult> GetSettings(int id)
	{
		var response = await channelService.GetSettings(id);
		return ProcessResponse<int>(response);
	}

	[HttpPost("{id}/validate")]
	public async Task<IActionResult> Validate(int id)
	{
		var response = await channelService.Validate(id);
		return ProcessResponse<int>(response);
	}


	[HttpPut("{id}/settings/{code}")]
	public async Task<IActionResult> UpsertSetting(int id, string code, [FromBody] ChannelSettingUpdateRequest request)
	{
		var response = await channelService.UpsertSetting(id, code, request);
		return ProcessResponse<int>(response);
	}

	[HttpPost("{id}/test")]
	public async Task<IActionResult> TestChannel(int id, [FromBody] TestChannelRequest req)
	{
		var response = await channelService.TestChannel(id, req);
		return ProcessResponse<int>(response);
	}
}