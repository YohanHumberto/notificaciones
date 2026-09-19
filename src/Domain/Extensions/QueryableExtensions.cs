using CommonStructures.Contracts.Common;
using Domain.Exceptions;
using System.Linq.Expressions;
using System.Reflection;

namespace Domain.Extensions
{
	public static class QueryableExtensions
	{
		public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, SortRequest[]? sortOrder)
		{
			if (sortOrder == null || sortOrder.Length == 0)
				return query;

			IOrderedQueryable<T>? orderedQuery = null;

			foreach (var sort in sortOrder)
			{
				if (string.IsNullOrWhiteSpace(sort.Field) ||
					sort.Direction == SortDirection.None)
				{
					continue;
				}

				var parameter = Expression.Parameter(typeof(T), "x");

				Expression property = parameter;

				foreach (var member in sort.Field.Split('.'))
				{
					property = Expression.PropertyOrField(property, member);
				}

				var lambda = Expression.Lambda(
					property,
					parameter);

				var methodName = orderedQuery == null
					? sort.Direction == SortDirection.Asc
						? nameof(Queryable.OrderBy)
						: nameof(Queryable.OrderByDescending)
					: sort.Direction == SortDirection.Asc
						? nameof(Queryable.ThenBy)
						: nameof(Queryable.ThenByDescending);

				var method = typeof(Queryable)
					.GetMethods()
					.First(m =>
						m.Name == methodName &&
						m.IsGenericMethodDefinition &&
						m.GetGenericArguments().Length == 2 &&
						m.GetParameters().Length == 2);

				var genericMethod = method.MakeGenericMethod(
					typeof(T),
					property.Type);

				orderedQuery = (IOrderedQueryable<T>)genericMethod.Invoke(
					null,
					[orderedQuery ?? query, lambda])!;
			}

			return orderedQuery ?? query;
		}

		public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, IEnumerable<Domain.Common.Querying.Filter> filters)
		{
			var filterList = filters?.ToList();

			if (filterList == null || filterList.Count == 0)
				return query;

			var parameter = Expression.Parameter(typeof(T), "x");

			Expression? finalExpression = null;

			foreach (var filter in filterList)
			{
				var expression = BuildFilterExpression<T>(
					parameter,
					filter);

				if (finalExpression == null)
				{
					finalExpression = expression;
					continue;
				}

				finalExpression = filter.Logic switch
				{
					LogicalOperators.And =>
						Expression.AndAlso(
							finalExpression,
							expression),

					LogicalOperators.Or =>
						Expression.OrElse(
							finalExpression,
							expression),

					_ => throw new NotSupportedException(
						$"Logical operator '{filter.Logic}' is not supported.")
				};
			}

			if (finalExpression == null)
				return query;

			var lambda = Expression.Lambda<Func<T, bool>>(
				finalExpression,
				parameter);

			return query.Where(lambda);
		}

		private static Expression BuildFilterExpression<T>(ParameterExpression parameter, Domain.Common.Querying.Filter filter)
		{
			var propertyInfo = typeof(T).GetProperty(filter.Field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
				?? throw new InvalidFilterException($"The field '{filter.Field}' does not exist on type '{typeof(T).Name}'.");

			var property = Expression.Property(parameter, propertyInfo);
			var propertyType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;
			var value = ConvertFilterValue(filter.Value, propertyType);

			var constant = Expression.Constant(
				value,
				propertyType);

			// Si la propiedad es Nullable<T>
			Expression left = property;

			if (Nullable.GetUnderlyingType(property.Type) != null)
			{
				left = Expression.Property(
					property,
					nameof(Nullable<>.Value));
			}

			return filter.Operator switch
			{
				FilterOperations.Equals =>
					Expression.Equal(left, constant),

				FilterOperations.Different =>
					Expression.NotEqual(left, constant),

				FilterOperations.GreaterThan =>
					Expression.GreaterThan(left, constant),

				FilterOperations.GreaterThanOrEqual =>
					Expression.GreaterThanOrEqual(left, constant),

				FilterOperations.LowerThan =>
					Expression.LessThan(left, constant),

				FilterOperations.LowerThanOrEqual =>
					Expression.LessThanOrEqual(left, constant),

				FilterOperations.Contains =>
					Expression.Call(
						left,
						nameof(string.Contains),
						Type.EmptyTypes,
						constant),


				FilterOperations.DoesNotContain =>
					Expression.Not(
						Expression.Call(
							left,
							nameof(string.Contains),
							Type.EmptyTypes,
							constant)),

				FilterOperations.StartsWith =>
					Expression.Call(
						left,
						nameof(string.StartsWith),
						Type.EmptyTypes,
						constant),

				FilterOperations.EndsWith =>
					Expression.Call(
						left,
						nameof(string.EndsWith),
						Type.EmptyTypes,
						constant),

				_ => throw new NotSupportedException(
					$"Operator '{filter.Operator}' is not supported.")
			};
		}

		private static object? ConvertFilterValue(string value, Type targetType)
		{
			var type = Nullable.GetUnderlyingType(targetType)
					   ?? targetType;

			if (string.IsNullOrWhiteSpace(value))
				return null;

			if (type == typeof(string))
				return value;

			if (type == typeof(int))
				return int.Parse(value);

			if (type == typeof(long))
				return long.Parse(value);

			if (type == typeof(decimal))
				return decimal.Parse(value);

			if (type == typeof(double))
				return double.Parse(value);

			if (type == typeof(bool))
				return bool.Parse(value);

			if (type == typeof(Guid))
				return Guid.Parse(value);

			if (type == typeof(DateTime))
				return DateTime.Parse(value);

			if (type.IsEnum)
				return Enum.Parse(
					type,
					value,
					ignoreCase: true);

			return Convert.ChangeType(value, type);
		}

	}
}
