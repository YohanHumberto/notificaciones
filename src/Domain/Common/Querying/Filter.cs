using CommonStructures.Contracts.Common;

namespace Domain.Common.Querying
{
	/// <summary>
	///     Represents a filter criterion used to filter query results.
	/// </summary>
	public class Filter
	{
		/// <summary>
		///     Gets or sets the name of the field to filter by.
		/// </summary>
		public string Field { get; set; } = string.Empty;

		/// <summary>
		///     Gets or sets the comparison operation to apply to the field.
		/// </summary>
		public FilterOperations Operator { get; set; }

		/// <summary>
		///     Gets or sets the value to use when applying the filter.
		/// </summary>
		public string Value { get; set; } = string.Empty;

		/// <summary>
		///     Gets or sets the logical operator used to combine this filter with other filters.
		/// </summary>
		public LogicalOperators Logic { get; set; }
	}
}
