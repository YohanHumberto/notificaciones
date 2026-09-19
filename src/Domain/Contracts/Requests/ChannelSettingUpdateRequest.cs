namespace Domain.Contracts.Requests
{
	public class ChannelSettingUpdateRequest
	{
		public object? Value { get; set; }
		public bool Delete { get; set; }
	}
}
