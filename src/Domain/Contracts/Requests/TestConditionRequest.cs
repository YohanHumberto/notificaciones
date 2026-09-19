using Persistence.Entities;

namespace Domain.Contracts.Requests
{
	public class TestConditionRequest
	{
		public ConditionalRule Rule { get; set; } = new();
	}
}
