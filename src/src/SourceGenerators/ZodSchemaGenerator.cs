using Microsoft.CodeAnalysis;
using ZodSharp.SourceGenerators.Helpers;
using ZodSharp.SourceGenerators.Models;

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
		context
			.RegisterEmbeddedAttribute<ZodSchemaGenerator>()
			.RegisterPostInitializationOutput(static ctx =>
			{
				foreach (var (HintName, SourceText) in AttributeGenHelper.GenerateMarkers())
					ctx.AddSource($"{HintName}.g.cs", SourceText);
			});

		var generationValueProviders = SourceGenLibrary.GetGeneratorValueProviders(context);

		// Register source outputs
		context.RegisterSourceOutput(
			generationValueProviders,
			(spc, model) =>
			{
				if (model.Context.Settings.IsSourceGeneratorDisabled)
					return;

				if (!model.Context.Capabilities.HasRequiredAttribute)
				{
					var diagnostic = DiagnosticInfo.Create(DiagnosticLibrary.DataAnnotationsReferenceNotFound);
					spc.ReportDiagnostic(diagnostic);

					return;
				}

				foreach (var schema in model.ZodSchemas)
				{
					if (schema.HasDiagnostics)
						spc.ReportDiagnostics(schema.Diagnostics);

					if (schema.Value.CustomValidationMethod.HasDiagnostics)
						spc.ReportDiagnostics(schema.Value.CustomValidationMethod.Diagnostics);

					foreach (var property in schema.Value.Properties)
					{
						if (property.HasDiagnostics)
							spc.ReportDiagnostics(property.Diagnostics);

						if (property.Value.ValidationAttributes.HasDiagnostics)
							spc.ReportDiagnostics(property.Value.ValidationAttributes.GetDiagnostics());
					}

					if (!schema.ShouldProcess)
						continue;

					try
					{
						SchemaGenerationOutputContext outputContext = new(model.Context, schema.Value);
						BuildSchema(outputContext, spc, schema.Value.IsPrimary);
					}
					catch (CodeWriterScopeValidationException)
					{
						// This is an opt-in framework invariant failure. Let Roslyn and the test
						// harness retain the exception instead of reducing it to ZODSGEN001.
						throw;
					}
					catch (Exception ex)
					{
						// Report diagnostic if generation fails
						var diagnostic = DiagnosticInfo.Create(
							DiagnosticLibrary.UnhandledException,
							schema.Value.TargetType.Name,
							ex.Message
						);

						spc.ReportDiagnostic(diagnostic);
					}
				}
			}
		);
	}
}
