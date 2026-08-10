using System.Globalization;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Purview.SourceGenerators.Testing;
using ZodSharp.SourceGenerators.Infra;

namespace ZodSharp.SourceGenerators;

public partial class ZodSchemaGeneratorTests
	: IncrementalSourceGeneratorTestBase<ZodSchemaGenerator>
{
	static string GetSchemaGeneratedSource(DriverRunResult driverRunResults) =>
		GetSchemaGeneratedSource(driverRunResults.Result);

	static string GetSchemaGeneratedSource(GeneratorDriverRunResult result)
	{
		var syntaxTree = result.GeneratedTrees.FirstOrDefault(static tree =>
		{
			var source = tree.GetText().ToString();
			return source.Contains(" static partial class ", StringComparison.Ordinal)
				&& source.Contains("Schema", StringComparison.Ordinal);
		});

		return syntaxTree?.GetText().ToString() ?? string.Empty;
	}

	static string GetSchemaGeneratedSource(DriverRunResult driverRunResults, string schemaName) =>
		GetSchemaGeneratedSource(driverRunResults.Result, schemaName);

	static string GetSchemaGeneratedSource(GeneratorDriverRunResult result, string schemaName)
	{
		var syntaxTree = result.GeneratedTrees.FirstOrDefault(tree =>
		{
			var source = tree.GetText().ToString();
			return source.Contains($" static partial class {schemaName}", StringComparison.Ordinal);
		});

		return syntaxTree?.GetText().ToString() ?? string.Empty;
	}

	static async Task AssertNoGeneratorExceptions(GeneratorDriverRunResult result)
	{
		foreach (var genResult in result.Results)
		{
			await Assert.That(genResult.Exception).IsNull().Because(genResult.Exception?.Message!);
		}
	}

	static async Task AssertNoGeneratorExceptions(DriverRunResult driverRunResults) =>
		await AssertNoGeneratorExceptions(driverRunResults.Result);

	static async Task AssertNoCompilationErrors(
		DriverRunResult driverRunResults,
		CancellationToken cancellationToken
	) => await AssertNoCompilationErrors(driverRunResults.OutputCompilation, cancellationToken);

	static async Task AssertNoCompilationErrors(
		Compilation compilation,
		CancellationToken cancellationToken
	)
	{
		var errors = compilation
			.GetDiagnostics(cancellationToken)
			.Where(static d => d.Severity == DiagnosticSeverity.Error)
			.ToArray();

		await Assert
			.That(errors)
			.IsEmpty()
			.Because(
				"Errors:\n\t"
					+ string.Join(
						"\t",
						errors.Select(static e => e.ToString() + Environment.NewLine)
					)
			);
	}

	static async Task AssertNoDiagnostics(
		DriverRunResult driverRunResults,
		DiagnosticSeverity? minimumSeverity = null
	) => await AssertNoDiagnostics(driverRunResults.Result, minimumSeverity);

	static async Task AssertNoDiagnostics(
		GeneratorDriverRunResult result,
		DiagnosticSeverity? minimumSeverity = null
	)
	{
		if (minimumSeverity.HasValue)
		{
			foreach (var diag in result.Diagnostics)
			{
				await Assert.That(diag.Severity < minimumSeverity.Value).IsTrue();
			}
		}
		else
			await Assert
				.That(result.Diagnostics.Length)
				.IsZero()
				.Because(
					$"Expecting no diagnostics:\n"
						+ string.Join(
							"  ",
							result.Diagnostics.Select(static d =>
								$"[{d.Severity}]{d.Id}: {d.GetMessage(CultureInfo.InvariantCulture)}\n"
							)
						)
				);
	}

	static async Task<Assembly> CompileToAssemblyAsync(
		Compilation compilation,
		CancellationToken cancellationToken
	)
	{
		await using MemoryStream assemblyStream = new();
		var emitResult = compilation.Emit(assemblyStream, cancellationToken: cancellationToken);
		if (!emitResult.Success)
		{
			var diagnostics = string.Join(
				Environment.NewLine,
				emitResult
					.Diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error)
					.Select(static d => d.ToString())
			);

			throw new InvalidOperationException(diagnostics);
		}

		assemblyStream.Position = 0;
		return System.Reflection.Assembly.Load(assemblyStream.ToArray());
	}

	static Diagnostic[] GetGeneratorDiagnostics(DriverRunResult driverRunResults) =>
		GetGeneratorDiagnostics(driverRunResults.Result);

	static Diagnostic[] GetGeneratorDiagnostics(GeneratorDriverRunResult result) =>
		[
			.. result
				.Results.SelectMany(static generatorResult => generatorResult.Diagnostics)
				.OrderBy(static d => d.Id),
		];
}
