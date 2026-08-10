using System.Collections.Immutable;

namespace ZodSharp.SourceGenerators.Helpers;

partial class TypeLibrary
{
	public static readonly TypeValueObject CancellationToken = new(typeof(CancellationToken));

	public static readonly TypeValueObject ValueTask = new(typeof(ValueTask));

	public static class Collections
	{
		public static readonly TypeValueObject ImmutableArray = new(typeof(ImmutableArray));

		public static readonly TypeValueObject ICollection = new(
			nameof(ICollection),
			"System.Collections"
		);

		public static readonly TypeValueObject IEnumerable = new(
			nameof(IEnumerable),
			"System.Collections"
		);

		public static readonly TypeValueObject ICollectionT = new(
			"ICollection",
			"System.Collections.Generic"
		);

		public static readonly TypeValueObject IEnumerableT = new(
			"IEnumerable",
			"System.Collections.Generic"
		);

		public static readonly TypeValueObject IReadOnlyCollectionT = new(
			"IReadOnlyCollection",
			"System.Collections.Generic"
		);
	}
}
