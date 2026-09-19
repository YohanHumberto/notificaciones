namespace Domain.Contracts.Requests
{
	public class PreviewTemplateRequest
	{
		public string? SubjectTemplate { get; set; }
		public string? BodyTemplate { get; set; }
		public string? SampleJsonData { get; set; }
	}
}
