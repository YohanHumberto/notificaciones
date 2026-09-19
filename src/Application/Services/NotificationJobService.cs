using Application.Services.Base;
using AutoMapper;
using CommonStructures.Contracts.Response;
using Domain.Contracts.Repository;
using Domain.Contracts.Services;
using Domain.Interfaces;
using Persistence.Entities;

namespace Application.Services
{
	/// <summary>
	/// Provides the service implementation for
	/// <see cref="NotificationJob"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common service functionality from
	/// <see cref="BaseService{T}"/> and implements
	/// <see cref="INotificationJobService"/>.
	/// </remarks>
	/// <param name="repository">
	/// The repository used to accessNotificationJob persistence data.
	/// </param>
	/// <param name="mapper">
	/// The AutoMapper instance used to map between domain and persistence models.
	/// </param>
	public class NotificationJobService(INotificationJobRepository repository, ISchedulerService schedulerService,
		 INotificationJobProcessor jobProcessor, IMapper mapper)
		: BaseService<NotificationJob>(repository, mapper), INotificationJobService
	{

		/// <inheritdoc/>
		public override async Task<AppResponse<TCreateResponse>> Create<TCreateRequest, TCreateResponse>(TCreateRequest request)
		{
			var response = await this.Create<TCreateRequest, TCreateResponse>(request);

			// Sync Quartz schedule
			await schedulerService.SyncJobScheduleAsync((NotificationJob)(object)request);

			return response;
		}

		/// <inheritdoc/>
		public override async Task<AppResponse<IReadOnlyList<TCreateResponse>>> CreateRange<TCreateRequest, TCreateResponse>(IReadOnlyList<TCreateRequest> request)
		{
			var response = await this.CreateRange<TCreateRequest, TCreateResponse>(request);
			foreach (var item in request)
			{
				// Sync Quartz schedule
				await schedulerService.SyncJobScheduleAsync((NotificationJob)(object)item);
			}
			return response;
		}

		/// <inheritdoc/>
		public override async Task<AppResponse> Update<TUpdateRequest>(TUpdateRequest request)
		{
			var response = await this.Update(request);

			// Sync Quartz schedule
			await schedulerService.SyncJobScheduleAsync((NotificationJob)(object)request);

			return response;
		}

		/// <inheritdoc/>
		public override async Task<AppResponse> Delete<TIdType>(TIdType id)
		{
			var response = await this.Delete(id);

			// Remove Quartz schedule
			await schedulerService.RemoveJobScheduleAsync((int)(object)id);

			return response;
		}

		/// <inheritdoc/>
		public async Task<NotificationJob> ToggleActive(int id)
		{
			var job = await repository.GetById(id);
			if (job == null) return null;

			job.IsActive = !job.IsActive;
			await repository.UpdateAsync(job);

			await schedulerService.SyncJobScheduleAsync(job);
			return job;
		}

		/// <inheritdoc/>
		public async Task<ExecutionLog> TriggerNow(int id)
		{
			var job = await repository.GetById(id);
			if (job == null) return null;

			var log = await jobProcessor.ProcessJobAsync(id);
			return log;
		}
	}
}
