using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ZodSharp.SourceGenerators.Models;
using ZodSharp.SourceGenerators.Models.DataAttributes;

namespace ZodSharp.SourceGenerators.Helpers;

static partial class SourceGenLibrary
{
	public static IncrementalValueProvider<SchemaGenerationModel> GetGeneratorValueProviders(
		IncrementalGeneratorInitializationContext context
	)
	{
		var generationContext = IncrementalPipeline.GenerationContextValueProvider<
			SchemaGenerationCapabilities,
			ZodSchemaGenerator
		>(
			context,
			static (compilation, _, _, _) =>
				new()
				{
					HasRequiredAttribute = TypeHelpers.HasType(
						compilation,
						TypeLibrary.DataAnnotations.RequiredAttribute
					),
				},
			PropertyLibrary.DisableZodSharpSourceGeneratorProperty
		);
		var schemaSets = IncrementalPipeline.ForAttributeWithMetadataName(
			context,
			TypeLibrary.ZodSchemaAttribute,
			predicate: static (node, _) => node is TypeDeclarationSyntax,
			transform: static (attributeContext, cancellationToken) =>
				GetSchemasForGeneration(attributeContext, cancellationToken)
		);

		return generationContext.CollectWith(
			schemaSets,
			static (outputContext, sets, _) =>
				new SchemaGenerationModel(outputContext) { ZodSchemas = Deduplicate(sets) },
			"CollectZodSchemas"
		);
	}

	static GeneratorResult<SchemaSet> GetSchemasForGeneration(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken
	)
	{
		if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode, cancellationToken) is not INamedTypeSymbol root)
			return default;

		var schemas = ImmutableArray.CreateBuilder<ZodSchemaDescriptor>();
		var seen = new HashSet<TypeIdentity>();
		var queue = new Queue<(INamedTypeSymbol Symbol, bool IsPrimary)>();
		queue.Enqueue((root, true));

		while (queue.Count > 0)
		{
			var (symbol, isPrimary) = queue.Dequeue();
			TypeIdentity target = new(symbol);
			if (!seen.Add(target))
				continue;

			var schema = target with { Name = $"{target.Name}Schema" };
			var targetCanBeNull = TypeHelpers.CanBeNull(symbol);
			var properties = GetZodProperties(symbol);
			var accessibility = symbol.ContainingType is null
				? symbol.DeclaredAccessibility == Accessibility.Public
					? TypeDeclarationAccessibility.Public
					: TypeDeclarationAccessibility.Internal
				: symbol.DeclaredAccessibility.ToTypeDeclarationAccessibility();
			var zodSchemaAttribute = ZodSchemaAttributeData.FromAttributeData(symbol, out var attribute);
			var customValidation = ResolveCustomValidationMethod(symbol, zodSchemaAttribute, attribute!);

			schemas.Add(
				new(
					target,
					schema,
					targetCanBeNull,
					GetContainingTypes(symbol),
					accessibility,
					properties,
					customValidation,
					isPrimary
				)
			);

			foreach (
				var property in symbol
					.GetMembers()
					.OfType<IPropertySymbol>()
					.Where(m =>
						m.DeclaredAccessibility == Accessibility.Public
						&& !m.IsStatic
						&& !m.IsIndexer
						&& TypeHelpers.HasDataAnnotationAttribute(m)
					)
			)
			{
				if (TryGetNestedSchemaType(property, out var nested))
					queue.Enqueue((nested, false));
			}
		}

		return GeneratorResult<SchemaSet>.Create(new SchemaSet(schemas.ToImmutable()));
	}

	static EquatableArray<TypeDeclarationOptions> GetContainingTypes(INamedTypeSymbol typeSymbol)
	{
		var chain = ImmutableArray.CreateBuilder<TypeDeclarationOptions>();
		var current = typeSymbol.ContainingType;
		while (current is not null)
		{
			// We don't own the generated container class, so don't include the
			// generated attributes on it, otherwise if that itself is also generated,
			// it will have duplicate attributes.
			chain.Add(
				TypeHelpers.CreatePartialTypeDeclarationOptions(current) with
				{
					IncludeGeneratedAttributes = false,
				}
			);
			current = current.ContainingType;
		}

		chain.Reverse();
		return chain.ToImmutable();
	}

	static EquatableArray<GeneratorResult<ZodSchemaDescriptor>> Deduplicate(
		ImmutableArray<GeneratorResult<SchemaSet>> sets
	)
	{
		var results = ImmutableArray.CreateBuilder<GeneratorResult<ZodSchemaDescriptor>>();
		var seen = new HashSet<TypeIdentity>();
		foreach (var set in sets)
		{
			if (!set.ShouldProcess)
				continue;

			foreach (var schema in set.Value.Schemas)
			{
				if (seen.Add(schema.TargetType))
					results.Add(schema);
			}
		}

		return new(results.ToImmutable());
	}

	static EquatableArray<GeneratorResult<ZodPropertyDescriptor>> GetZodProperties(INamedTypeSymbol symbol) =>
		new([
			.. symbol
				.GetMembers()
				.OfType<IPropertySymbol>()
				.Where(static property =>
					property.DeclaredAccessibility == Accessibility.Public
					&& !property.IsStatic
					&& !property.IsIndexer
					&& TypeHelpers.HasDataAnnotationAttribute(property)
				)
				.Select(static property => GetValidatablePropertyDescriptor(property)),
		]);

	static GeneratorResult<ZodPropertyDescriptor> GetValidatablePropertyDescriptor(IPropertySymbol property)
	{
		TypeIdentity propertyType = new(property.Type);
		var propertyCanBeNull = TypeHelpers.CanBeNull(property.Type);
		if (
			property.Type is INamedTypeSymbol
			{
				OriginalDefinition.SpecialType: SpecialType.System_Nullable_T
			} nullableType
		)
		{
			propertyType = new(nullableType.TypeArguments[0]);
		}

		var diagnostics = ImmutableArray.CreateBuilder<DiagnosticInfo>();
		var supportsLengthAttribute =
			propertyType.SpecialType == SpecialType.System_String
			|| property.Type is IArrayTypeSymbol
			|| TypeHelpers.IsOrImplements(property.Type, TypeLibrary.Collections.IEnumerable)
			|| TypeHelpers.IsOrImplements(property.Type, TypeLibrary.Collections.IEnumerableT);

		var requiredAttribute = RequiredAttributeData.FromAttributeData(property);
		var compareAttribute = CompareAttributeData.FromAttributeData(property);
		var displayAttribute = DisplayAttributeData.FromAttributeData(property);
		var emailAddressAttribute = EmailAddressAttributeData.FromAttributeData(property);
		var creditCardAttribute = CreditCardAttributeData.FromAttributeData(property);
		var phoneAttribute = PhoneAttribute.FromAttributeData(property);
		var urlAttribute = UrlAttribute.FromAttributeData(property);
		var stringLengthAttribute = StringLengthAttribute.FromAttributeData(property);
		var minLengthAttribute = MinLengthAttributeData.FromAttributeData(property);
		var maxLengthAttribute = MaxLengthAttributeData.FromAttributeData(property);
		var regularExpressionAttribute = GeneratorResult<RegularExpressionAttributeData>.Empty;
		if (RegularExpressionAttributeData.TryFromAttributeData(property, out var regexData, out var attribute))
		{
			regularExpressionAttribute =
				attribute is not null && property.Type.SpecialType != SpecialType.System_String
					? GeneratorResult<RegularExpressionAttributeData>.Create(
						regexData,
						DiagnosticInfo.Create(
							DiagnosticLibrary.UnsupportedDataAnnotationsUsage,
							attribute,
							string.Format(
								CultureInfo.InvariantCulture,
								"RegularExpressionAttribute can only be applied to string properties, but '{0}' is '{1}'.",
								property.Name,
								propertyType.MetadataFullName
							)
						)
					)
					: GeneratorResult<RegularExpressionAttributeData>.Create(regexData);
		}

		var base64StringAttribute = Base64StringAttributeData.FromAttributeData(property);
		var deniedValuesAttribute = DeniedValuesAttributeData.FromAttributeData(property);
		var allowedValuesAttribute = AllowedValuesAttributeData.FromAttributeData(property);
		var lengthAttribute = GeneratorResult<LengthAttributeData>.Empty;
		if (LengthAttributeData.TryFromAttributeData(property, out var lengthData, out attribute))
		{
			lengthAttribute =
				attribute is not null && !supportsLengthAttribute
					? GeneratorResult<LengthAttributeData>.Create(
						lengthData,
						DiagnosticInfo.Create(
							DiagnosticLibrary.UnsupportedLengthAttributeTarget,
							attribute,
							string.Format(
								CultureInfo.InvariantCulture,
								"LengthAttribute cannot be applied to '{0}' because '{1}' exposes no accessible Length or Count member and is not an enumerable shape that ZodSharp can count safely.",
								property.Name,
								propertyType.MetadataFullName
							)
						)
					)
					: GeneratorResult<LengthAttributeData>.Create(lengthData);
		}

		var rangeAttribute = RangeAttributeData.FromAttributeData(property.GetAttributes(), out attribute);
		var rangeAttributeResult = TryBuildRangeBoundaryExpressions(
			property.Type,
			rangeAttribute,
			out var minimumExpression,
			out var maximumExpression
		)
			? GeneratorResult<RangeAttributeData>.Create(
				rangeAttribute with
				{
					MinimumExpression = minimumExpression,
					MaximumExpression = maximumExpression,
				}
			)
			: GeneratorResult<RangeAttributeData>.Create(
				rangeAttribute,
				DiagnosticInfo.Create(
					DiagnosticLibrary.UnsupportedDataAnnotationsUsage,
					attribute!,
					string.Format(
						CultureInfo.InvariantCulture,
						"RangeAttribute can only be applied to numeric properties, but '{0}' is '{1}'.",
						property.Name,
						propertyType.MetadataFullName
					)
				)
			);

		if (attribute is not null && !TypeHelpers.IsNumericType(property.Type))
		{
			diagnostics.Add(
				DiagnosticInfo.Create(
					DiagnosticLibrary.UnsupportedDataAnnotationsUsage,
					attribute,
					string.Format(
						CultureInfo.InvariantCulture,
						"RangeAttribute can only be applied to numeric properties, but '{0}' is '{1}'.",
						property.Name,
						propertyType.MetadataFullName
					)
				)
			);
		}

		return GeneratorResult<ZodPropertyDescriptor>.Create(
			new(
				propertyType,
				property.Name,
				propertyCanBeNull,
				new(
					requiredAttribute,
					compareAttribute,
					displayAttribute,
					emailAddressAttribute,
					creditCardAttribute,
					phoneAttribute,
					urlAttribute,
					stringLengthAttribute,
					minLengthAttribute,
					maxLengthAttribute,
					regularExpressionAttribute,
					base64StringAttribute,
					deniedValuesAttribute,
					allowedValuesAttribute,
					lengthAttribute,
					rangeAttributeResult
				)
			),
			diagnostics.ToImmutable()
		);
	}

	static bool TryGetNestedSchemaType(IPropertySymbol property, out INamedTypeSymbol nested)
	{
		var propertyType = TypeHelpers.UnwrapNullableType(property.Type);
		if (propertyType is IArrayTypeSymbol array)
			propertyType = array.ElementType;
		else if (propertyType is INamedTypeSymbol named)
		{
			var enumerable = named.AllInterfaces.FirstOrDefault(TypeLibrary.Collections.IEnumerableT.Equals);
			if (enumerable is not null)
				propertyType = enumerable.TypeArguments[0];
		}

		propertyType = TypeHelpers.UnwrapNullableType(propertyType);
		nested = propertyType as INamedTypeSymbol ?? null!;
		return nested is not null
			&& nested.Locations.Any(static location => location.IsInSource)
			&& !ZodSchemaGenerator.IsScalarType(nested);
	}

	static bool TryBuildRangeBoundaryExpressions(
		ITypeSymbol propertyType,
		RangeAttributeData rangeAttribute,
		out string minimumExpression,
		out string maximumExpression
	)
	{
		if (!rangeAttribute.Exists)
		{
			minimumExpression = string.Empty;
			maximumExpression = string.Empty;

			return false;
		}

		propertyType = TypeHelpers.UnwrapNullableType(propertyType);
		if (TypeHelpers.IsNumericType(propertyType))
			return TryBuildNumericRangeBoundaryExpressions(
				propertyType,
				rangeAttribute,
				out minimumExpression,
				out maximumExpression
			);

		if (
			TypeHelpers.IsNamedType(propertyType, "System.DateTime")
			&& rangeAttribute.Kind == RangeAttributeKind.Converted
		)
		{
			minimumExpression = BuildDateTimeParseExpression(
				(string)rangeAttribute.Minimum!,
				rangeAttribute.ParseLimitsInInvariantCulture
			);
			maximumExpression = BuildDateTimeParseExpression(
				(string)rangeAttribute.Maximum!,
				rangeAttribute.ParseLimitsInInvariantCulture
			);
			return true;
		}

		if (
			TypeHelpers.IsNamedType(propertyType, "System.DateOnly")
			&& rangeAttribute.Kind == RangeAttributeKind.Converted
		)
		{
			minimumExpression = BuildDateOnlyParseExpression(
				(string)rangeAttribute.Minimum!,
				rangeAttribute.ParseLimitsInInvariantCulture
			);
			maximumExpression = BuildDateOnlyParseExpression(
				(string)rangeAttribute.Maximum!,
				rangeAttribute.ParseLimitsInInvariantCulture
			);
			return true;
		}

		if (
			TypeHelpers.IsNamedType(propertyType, "System.TimeOnly")
			&& rangeAttribute.Kind == RangeAttributeKind.Converted
		)
		{
			minimumExpression = BuildTimeOnlyParseExpression(
				(string)rangeAttribute.Minimum!,
				rangeAttribute.ParseLimitsInInvariantCulture
			);
			maximumExpression = BuildTimeOnlyParseExpression(
				(string)rangeAttribute.Maximum!,
				rangeAttribute.ParseLimitsInInvariantCulture
			);
			return true;
		}

		minimumExpression = string.Empty;
		maximumExpression = string.Empty;

		return false;
	}

	static bool TryBuildNumericRangeBoundaryExpressions(
		ITypeSymbol propertyType,
		RangeAttributeData rangeAttribute,
		out string minimumExpression,
		out string maximumExpression
	)
	{
		if (rangeAttribute.Kind == RangeAttributeKind.Int32)
		{
			minimumExpression = ConvertNumericLiteralExpression(propertyType, (int)rangeAttribute.Minimum!);
			maximumExpression = ConvertNumericLiteralExpression(propertyType, (int)rangeAttribute.Maximum!);
			return minimumExpression.Length > 0 && maximumExpression.Length > 0;
		}

		if (rangeAttribute.Kind == RangeAttributeKind.Double)
		{
			minimumExpression = ConvertNumericLiteralExpression(propertyType, (double)rangeAttribute.Minimum!);
			maximumExpression = ConvertNumericLiteralExpression(propertyType, (double)rangeAttribute.Maximum!);
			return minimumExpression.Length > 0 && maximumExpression.Length > 0;
		}

		if (
			rangeAttribute.Kind == RangeAttributeKind.Converted
			&& rangeAttribute.Minimum is string minimum
			&& rangeAttribute.Maximum is string maximum
		)
		{
			minimumExpression = BuildNumericParseExpression(
				propertyType,
				minimum,
				rangeAttribute.ParseLimitsInInvariantCulture
			);
			maximumExpression = BuildNumericParseExpression(
				propertyType,
				maximum,
				rangeAttribute.ParseLimitsInInvariantCulture
			);
			return minimumExpression.Length > 0 && maximumExpression.Length > 0;
		}

		minimumExpression = string.Empty;
		maximumExpression = string.Empty;
		return false;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0072:Add missing cases")]
	static string ConvertNumericLiteralExpression(ITypeSymbol propertyType, int value) =>
		propertyType.SpecialType switch
		{
			SpecialType.System_Byte => $"(byte){value.ToString(CultureInfo.InvariantCulture)}",
			SpecialType.System_SByte => $"(sbyte){value.ToString(CultureInfo.InvariantCulture)}",
			SpecialType.System_Int16 => $"(short){value.ToString(CultureInfo.InvariantCulture)}",
			SpecialType.System_UInt16 => $"(ushort){value.ToString(CultureInfo.InvariantCulture)}",
			SpecialType.System_Int32 => value.ToString(CultureInfo.InvariantCulture),
			SpecialType.System_UInt32 => $"{value.ToString(CultureInfo.InvariantCulture)}U",
			SpecialType.System_Int64 => $"{value.ToString(CultureInfo.InvariantCulture)}L",
			SpecialType.System_UInt64 => $"{value.ToString(CultureInfo.InvariantCulture)}UL",
			SpecialType.System_Single => value.ToString(CultureInfo.InvariantCulture) + "F",
			SpecialType.System_Double => value.ToString(CultureInfo.InvariantCulture) + "D",
			SpecialType.System_Decimal => value.ToString(CultureInfo.InvariantCulture) + "M",
			_ => string.Empty,
		};

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0072:Add missing cases")]
	static string ConvertNumericLiteralExpression(ITypeSymbol propertyType, double value) =>
		propertyType.SpecialType switch
		{
			SpecialType.System_Single => $"(float){value.ToString("R", CultureInfo.InvariantCulture)}D",
			SpecialType.System_Double => value.ToString("R", CultureInfo.InvariantCulture) + "D",
			SpecialType.System_Decimal => $"(decimal){value.ToString("R", CultureInfo.InvariantCulture)}D",
			_ => BuildNumericParseExpression(
				propertyType,
				value.ToString("R", CultureInfo.InvariantCulture),
				invariantCulture: true
			),
		};

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0072:Add missing cases")]
	static string BuildNumericParseExpression(ITypeSymbol propertyType, string value, bool invariantCulture)
	{
		var cultureExpression = invariantCulture
			? "global::System.Globalization.CultureInfo.InvariantCulture"
			: "global::System.Globalization.CultureInfo.CurrentCulture";

		return propertyType.SpecialType switch
		{
			SpecialType.System_Byte =>
				$"global::System.Byte.Parse({CodeGenHelpers.Quote(value)}, global::System.Globalization.NumberStyles.Integer, {cultureExpression})",
			SpecialType.System_SByte =>
				$"global::System.SByte.Parse({CodeGenHelpers.Quote(value)}, global::System.Globalization.NumberStyles.Integer, {cultureExpression})",
			SpecialType.System_Int16 =>
				$"global::System.Int16.Parse({CodeGenHelpers.Quote(value)}, global::System.Globalization.NumberStyles.Integer, {cultureExpression})",
			SpecialType.System_UInt16 =>
				$"global::System.UInt16.Parse({CodeGenHelpers.Quote(value)}, global::System.Globalization.NumberStyles.Integer, {cultureExpression})",
			SpecialType.System_Int32 =>
				$"global::System.Int32.Parse({CodeGenHelpers.Quote(value)}, global::System.Globalization.NumberStyles.Integer, {cultureExpression})",
			SpecialType.System_UInt32 =>
				$"global::System.UInt32.Parse({CodeGenHelpers.Quote(value)}, global::System.Globalization.NumberStyles.Integer, {cultureExpression})",
			SpecialType.System_Int64 =>
				$"global::System.Int64.Parse({CodeGenHelpers.Quote(value)}, global::System.Globalization.NumberStyles.Integer, {cultureExpression})",
			SpecialType.System_UInt64 =>
				$"global::System.UInt64.Parse({CodeGenHelpers.Quote(value)}, global::System.Globalization.NumberStyles.Integer, {cultureExpression})",
			SpecialType.System_Single =>
				$"global::System.Single.Parse({CodeGenHelpers.Quote(value)}, global::System.Globalization.NumberStyles.Float | global::System.Globalization.NumberStyles.AllowThousands, {cultureExpression})",
			SpecialType.System_Double =>
				$"global::System.Double.Parse({CodeGenHelpers.Quote(value)}, global::System.Globalization.NumberStyles.Float | global::System.Globalization.NumberStyles.AllowThousands, {cultureExpression})",
			SpecialType.System_Decimal =>
				$"global::System.Decimal.Parse({CodeGenHelpers.Quote(value)}, global::System.Globalization.NumberStyles.Number, {cultureExpression})",
			_ => string.Empty,
		};
	}

	static string BuildDateTimeParseExpression(string value, bool invariantCulture)
	{
		var cultureExpression = invariantCulture
			? "global::System.Globalization.CultureInfo.InvariantCulture"
			: "global::System.Globalization.CultureInfo.CurrentCulture";
		return $"global::System.DateTime.Parse({CodeGenHelpers.Quote(value)}, {cultureExpression})";
	}

	static string BuildDateOnlyParseExpression(string value, bool invariantCulture)
	{
		var cultureExpression = invariantCulture
			? "global::System.Globalization.CultureInfo.InvariantCulture"
			: "global::System.Globalization.CultureInfo.CurrentCulture";
		return $"global::System.DateOnly.Parse({CodeGenHelpers.Quote(value)}, {cultureExpression})";
	}

	static string BuildTimeOnlyParseExpression(string value, bool invariantCulture)
	{
		var cultureExpression = invariantCulture
			? "global::System.Globalization.CultureInfo.InvariantCulture"
			: "global::System.Globalization.CultureInfo.CurrentCulture";
		return $"global::System.TimeOnly.Parse({CodeGenHelpers.Quote(value)}, {cultureExpression})";
	}
}
