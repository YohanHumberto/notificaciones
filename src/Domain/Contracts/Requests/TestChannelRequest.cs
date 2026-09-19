namespace Domain.Contracts.Requests
{
	public class TestChannelRequest
	{
		public string Recipient { get; set; } = string.Empty;
		public string? Subject { get; set; }
		public string? Body { get; set; }
	}
}
