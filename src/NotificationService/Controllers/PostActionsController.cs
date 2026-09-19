using Domain.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Persistence.Entities;

namespace NotificationService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostActionsController(IPostExecutionActionService service) :
	BaseCrudController<PostExecutionAction, PostExecutionAction, PostExecutionAction>(service)
{
}
