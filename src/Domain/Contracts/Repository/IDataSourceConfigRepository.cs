using Persistence.Entities;

namespace Domain.Contracts.Repository
{
	/// <summary>
	/// Defines the contract for data access operations related to
	/// <see cref="DataSourceConfig"/> entities.
	/// </summary>
	/// <remarks>
	/// Inherits the standard CRUD and query operations from
	/// <see cref="IRepositoryBase{T}"/> and can be extended with
	/// DataSourceConfig-specific data access methods as needed.
	/// </remarks>
	public interface IDataSourceConfigRepository : IRepositoryBase<DataSourceConfig>
	{
	}
}
