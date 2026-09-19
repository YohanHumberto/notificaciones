using Domain.Contracts.Repository;
using Persistence;
using Persistence.Entities;
using Repository.Base;

namespace Repository.Repositories
{
	/// <summary>
	/// Provides the repository implementation for
	/// <see cref="PostExecutionAction"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the common repository functionality from
	/// <see cref="RepositoryBase{T}"/> and serves as the extension point
	/// for PostExecutionAction-specific data access operations.
	/// </remarks>
	/// <param name="avmContext">
	/// The AVM database context used to access persistence data.
	/// </param>
	public class PostExecutionActionRepository(NotificationContext notificationContext)
		: RepositoryBase<PostExecutionAction>(notificationContext), IPostExecutionActionRepository
	{
	}
}
