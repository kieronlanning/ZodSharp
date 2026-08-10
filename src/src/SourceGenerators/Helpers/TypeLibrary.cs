using System.Text;
using Microsoft.CodeAnalysis;

namespace ZodSharp.SourceGenerators.Helpers;

static partial class TypeLibrary
{
	public const string ZodSharpNamespace = "ZodSharp";

	public const string ZodSharpCoreNamespace = ZodSharpNamespace + ".Core";

	// This matches the name of the class, just so we can use the `nameof` for later...
	public static readonly TypeValueObject ZodSchemaAttribute = new(
		nameof(ZodSchemaAttribute),
		ZodSharpNamespace
	);

	// Other ZodSharp types...
	public static readonly TypeValueObject ValidationResult = new(
		nameof(ValidationResult),
		ZodSharpCoreNamespace
	);
	public static readonly TypeValueObject ValidationResultMetadataName = new(
		nameof(ValidationResultMetadataName),
		ZodSharpCoreNamespace
	);
	public static readonly TypeValueObject ValidationError = new(
		nameof(ValidationError),
		ZodSharpCoreNamespace
	);

	// Default custom async validation method name when none is explicitly configured.
	public const string DefaultCustomValidationMethodName = "CustomValidationAsync";

	// System Types

	public static LengthAccessKind GetLengthAccessKind(ITypeSymbol propertyType) =>
		propertyType.SpecialType == SpecialType.System_String || propertyType is IArrayTypeSymbol
			? LengthAccessKind.Length
		: HasAccessibleCountProperty(propertyType) ? LengthAccessKind.Count
		: LengthAccessKind.Enumerable;

	public static bool HasAccessibleCountProperty(ITypeSymbol propertyType) =>
		propertyType
			.GetMembers("Count")
			.OfType<IPropertySymbol>()
			.Any(static p =>
				!p.IsStatic
				&& p.DeclaredAccessibility == Accessibility.Public
				&& p.Parameters.Length == 0
				&& p.Type.SpecialType == SpecialType.System_Int32
			);

	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Style",
		"IDE0072:Add missing cases",
		Justification = "Internal is the valid default"
	)]
	public static string GetLimitedAccessibilityKeyword(INamedTypeSymbol symbol)
	{
		return symbol.DeclaredAccessibility switch
		{
			Accessibility.Public => "public",
			Accessibility.Private => "private",
			_ => "internal",
		};
	}

	public static string GetFullSchemaTypeName(INamedTypeSymbol typeSymbol)
	{
		var sb = new StringBuilder();
		sb.Append("global::");

		var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
		if (!string.IsNullOrEmpty(namespaceName))
		{
			sb.Append(namespaceName);
			sb.Append('.');
		}

		var containingTypes = new List<INamedTypeSymbol>();
		var current = typeSymbol.ContainingType;
		while (current is not null)
		{
			containingTypes.Add(current);
			current = current.ContainingType;
		}

		containingTypes.Reverse();

		foreach (var containingType in containingTypes)
		{
			sb.Append(containingType.Name);
			sb.Append('.');
		}

		sb.Append(typeSymbol.Name);
		sb.Append("Schema");
		return sb.ToString();
	}

	public static bool HasZodSchemaAttribute(
		ITypeSymbol typeSymbol,
		INamedTypeSymbol? zodSchemaAttributeSymbol
	)
	{
		var unwrapped = UnwrapNullableType(typeSymbol);
		if (unwrapped is not INamedTypeSymbol namedType || zodSchemaAttributeSymbol is null)
			return false;

		var comparer = SymbolEqualityComparer.Default;
		foreach (var attr in namedType.GetAttributes())
		{
			if (comparer.Equals(attr.AttributeClass, zodSchemaAttributeSymbol))
				return true;
		}

		foreach (var attr in namedType.GetAttributes())
		{
			if (attr.AttributeClass?.ToDisplayString() == ZodSchemaAttribute)
				return true;
		}

		return false;
	}

	public static bool CanBeNull(ITypeSymbol typeSymbol) =>
		typeSymbol.IsReferenceType
		|| typeSymbol.NullableAnnotation == NullableAnnotation.Annotated
		|| typeSymbol
			is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };

	public static bool IsNumericType(ITypeSymbol type) =>
		type.SpecialType
			is SpecialType.System_Byte
				or SpecialType.System_SByte
				or SpecialType.System_Int16
				or SpecialType.System_UInt16
				or SpecialType.System_Int32
				or SpecialType.System_UInt32
				or SpecialType.System_Int64
				or SpecialType.System_UInt64
				or SpecialType.System_Single
				or SpecialType.System_Double
				or SpecialType.System_Decimal;

#pragma warning disable format
	public static ITypeSymbol UnwrapNullableType(ITypeSymbol type) =>
		type
			is INamedTypeSymbol
			{
				OriginalDefinition.SpecialType: SpecialType.System_Nullable_T
			} nullableType
			? nullableType.TypeArguments[0]
			: type;
#pragma warning restore format

	public static bool IsSameType(
		ITypeSymbol left,
		ITypeSymbol? right,
		SymbolEqualityComparer? comparer = null
	)
	{
		if (right is null)
			return false;

		comparer ??= SymbolEqualityComparer.Default;
		return comparer.Equals(UnwrapNullableType(left), UnwrapNullableType(right));
	}

	public static bool IsNamedType(ITypeSymbol type, string fullyQualifiedMetadataName)
	{
		type = UnwrapNullableType(type);
		return type is INamedTypeSymbol namedType
			&& string.Equals(
				namedType.ToDisplayString(),
				fullyQualifiedMetadataName,
				StringComparison.Ordinal
			);
	}
}
