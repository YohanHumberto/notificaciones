using CommonStructures.Contracts.Common;

namespace Domain.Common.Querying
{
	/// <summary>
	///     Represents the parameters used to paginate, filter, and sort a collection.
	/// </summary>
	public class PageRequest
	{
		/// <summary>
		///     Gets or sets the page number to retrieve.
		///     Defaults to 1.
		/// </summary>
		public int PageNumber { get; set; } = 1;

		/// <summary>
		///     Gets or sets the maximum number of items to retrieve per page.
		///     Defaults to 15.
		/// </summary>
		public int PageSize { get; set; } = 5000000;

		/// <summary>
		///     Gets or sets the filters to apply to the query.
		///     Multiple filters can be combined using their specified logical operator.
		/// </summary>
		public Filter[] Filters { get; set; } = [];

		/// <summary>
		///     Gets or sets the sorting criteria to apply to the query.
		///     Multiple sorting criteria are applied in the specified order.
		/// </summary>
		public SortRequest[] SortOrder { get; set; } = [];
	}
}
