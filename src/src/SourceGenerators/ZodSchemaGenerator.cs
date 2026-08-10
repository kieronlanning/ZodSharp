using Microsoft.CodeAnalysis;
using ZodSharp.SourceGenerators.Helpers;

namespace ZodSharp.SourceGenerators;

/// <summary>
/// Source generator that creates optimized validators for classes marked with [ZodSchema].
/// Uses IIncrementalGenerator for better performance and incremental compilation support.
/// </summary>
[Generator]
public sealed partial class ZodSchemaGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(static ctx =>
		{
			// Adds the EmbeddedAttribute definition to the compilation if not already present
			// ensuring that internal generated code doesn't cause visibility issues in referenced
			// assemblies where InternalVisibleTo is set.
			ctx.AddEmbeddedAttributeDefinition();

			ctx.AddSource(
				$"{nameof(TypeLibrary.ZodSchemaAttribute)}.g.cs",
				EmbeddedResources.Load(nameof(TypeLibrary.ZodSchemaAttribute))
			);
		});

		var generationValueProviders = SourceGenLibrary.GetGeneratorValueProviders(
			context,
			_logger
		);

		// Register source outputs
		context.RegisterSourceOutput(
			generationValueProviders,
			static (spc, source) =>
			{
				if (!source.IsSourceGeneratorEnabled)
					return;

				foreach (var schema in source.ZodSchemas)
				{
					if (schema.HasDiagnostics)
					{
						ReportDiagnostics(spc, schema.Diagnostics, source.GenerationContext.Logger);
					}

					if (schema.IsFatal)
						return;
				}

				var primarySchemas = source
					.ZodSchemas.Where(static s => s.IsSuccess && s.Value is not null)
					.Select(static s => s.Value!)
					.ToList();

				var allSchemas = DiscoverAllSchemas(primarySchemas, source.GenerationContext);

				foreach (var (descriptor, isPrimary) in allSchemas)
				{
					Execute(descriptor, source.GenerationContext, spc, isPrimary);
				}
			}
		);
	}
}
