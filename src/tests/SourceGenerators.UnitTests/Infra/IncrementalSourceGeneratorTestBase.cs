using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;
using Purview.SourceGenerators.Testing;
using Purview.SourceGenerators.Testing.TUnit;
using ZodSharp.SourceGenerators.Helpers;

namespace ZodSharp.SourceGenerators.Infra;

public abstract class IncrementalSourceGeneratorTestBase<TGenerator>(bool throwOnLogError = true)
	: TUnitSourceGeneratorTestBase<TGenerator>
	where TGenerator : class, IIncrementalGenerator, new()
{
	static readonly Type[] DefaultAssemblyTypes =
	[
		typeof(Z),
		typeof(ImmutableArray),
		typeof(RequiredAttribute),
		typeof(System.Text.Json.JsonSerializer),
		typeof(System.Text.RegularExpressions.Regex),
	];

	public static readonly string[] GeneratedAttributes =
	[
		"EmbeddedAttribute.cs",
		"ZodSchema.g.cs",
	];

	public static readonly int ExpectedFileCount = GeneratedAttributes.Length;
	public static readonly int ExpectedFileCountPlusGen = ExpectedFileCount + 1;

	public const int HintNameHashHexLength = 16;
	public const string GeneratedSourceFileSuffix = ".g.cs";

	protected async Task<DriverRunResult> GenerateAsync(
		string source,
		CancellationToken cancellationToken
	) => await GenerateAsync(source, GenerationDriverContext.Default, cancellationToken);

	protected async Task<DriverRunResult> GenerateAsync(
		string source,
		GenerationDriverContext driverContext,
		CancellationToken cancellationToken
	)
	{
		var options = new SourceGeneratorTestOptions
		{
			IncludeDefaultNamespaces = driverContext.IncludeNamespaces,
			AdditionalNamespaces = ["ZodSharp"],
			AdditionalAssemblyTypes = ImmutableArray.Create(DefaultAssemblyTypes),
			ThrowOnGenerationException = driverContext.ThrowOnGenerationException,
			CompileToAssembly = driverContext.CompileToAssembly,
			ThrowOnLogError = throwOnLogError,
			DisableSourceGeneratorPropertyName =
				SourceGenHelpers.DisableZodSharpSourceGeneratorProperty,
			DisableSourceGeneratorValue = driverContext.DisableSourceGenerator,
			PreprocessReferences = driverContext.PreprocessReferences,
			ExcludeGeneratedAttributes = ImmutableArray.Create(GeneratedAttributes),
		};

		return await GenerateAsync(source, options, cancellationToken);
	}
}
