using Persistence.Enums;

namespace Domain.Contracts.Requests
{
	public class ChannelResponse
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public int ChannelTypeId { get; set; }
		public ChannelType Type { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}

	public class ChannelTypeResponse
	{
		public int Id { get; set; }
		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
	}

}
