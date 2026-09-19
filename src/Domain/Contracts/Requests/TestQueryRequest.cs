using Persistence.Entities;

namespace Domain.Contracts.Requests
{
	public class TestQueryRequest
	{
		public int? DataSourceId { get; set; }
		public DataSourceConfig? TempDataSource { get; set; }
		public string Query { get; set; } = string.Empty;
		public string? ParametersJson { get; set; }
	}
}
