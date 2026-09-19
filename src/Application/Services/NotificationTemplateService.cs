using Application.Services.Base;
using AutoMapper;
using CommonStructures.Contracts.Response;
using Domain.Contracts.Repository;
using Domain.Contracts.Requests;
using Domain.Contracts.Services;
using Domain.Interfaces;
using Persistence.Entities;
using System.Text.Json;

namespace Application.Services
{
	/// <summary>
	/// Provides the service implementation for
	/// <see cref="NotificationTemplate"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common service functionality from
	/// <see cref="BaseService{T}"/> and implements
	/// <see cref="INotificationTemplateService"/>.
	/// </remarks>
	/// <param name="repository">
	/// The repository used to accessNotificationTemplate persistence data.
	/// </param>
	/// <param name="mapper">
	/// The AutoMapper instance used to map between domain and persistence models.
	/// </param>
	public class NotificationTemplateService(INotificationTemplateRepository repository, ITemplateRenderer templateRenderer, IMapper mapper)
		: BaseService<NotificationTemplate>(repository, mapper), INotificationTemplateService
	{
		public async Task<AppResponse<object>> PreviewTemplate(PreviewTemplateRequest req)
		{
			try
			{
				object model = new { };
				if (!string.IsNullOrWhiteSpace(req.SampleJsonData))
				{
					using var doc = JsonDocument.Parse(req.SampleJsonData);
					model = doc.RootElement.Clone();
				}

				string renderedSubject = await templateRenderer.RenderAsync(req.SubjectTemplate ?? "", model);
				string renderedBody = await templateRenderer.RenderAsync(req.BodyTemplate ?? "", model);

				return new AppResponse<object>(new { success = true, subject = renderedSubject, body = renderedBody }, string.Empty);
			}
			catch (Exception ex)
			{
				return new InternalErrorResponse<object>(new { success = false, error = ex.Message }, string.Empty);
			}
		}
	}
}
