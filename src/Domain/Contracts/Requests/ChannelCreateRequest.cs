using Persistence.Enums;

namespace Domain.Contracts.Requests
{
	public class ChannelCreateRequest
	{
		public string Name { get; set; } = string.Empty;
		public int ChannelTypeId { get; set; }
		public ChannelType Type { get; set; }
		public bool IsActive { get; set; } = true;
	}
}
