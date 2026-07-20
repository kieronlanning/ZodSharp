using ZodSharp.Core;

namespace ZodSharp.SourceGenerators;

partial class ZodSchemaGeneratorTests
{
	[Test]
	public async Task CompositionMethods_GeneratedAsApplyNamedOverloads(CancellationToken cancellationToken)
	{
		// Arrange
		const string source =
			@"
namespace Testing
{
	[ZodSchema]
	public class ComposeModel
	{
		public string? Name { get; set; }
	}
}
";

		// Act
		var (result, outputCompilation) = await GenerateAsync(source, cancellationToken);
		var generatedSource = GetSchemaGeneratedSource(result, "ComposeModelSchema");

		// Assert — the renamed value-first composition methods are emitted.
		await AssertNoGeneratorExceptions(result);
		await AssertNoCompilationErrors(outputCompilation, cancellationToken);
		await Assert.That(generatedSource).Contains("ApplyAnd");
		await Assert.That(generatedSource).Contains("ApplyOr");
		await Assert.That(generatedSource).Contains("ApplyRefine");
		// Compilation succeeding proves the previously-buggy `Or` emission (a stray
		// dollar sign before the return type) is fixed.
	}

	[Test]
	public async Task CompositionMethods_GivenValidValue_ApplyRefineSucceeds(CancellationToken cancellationToken)
	{
		// Arrange
		const string source =
			@"
namespace Testing
{
	[ZodSchema]
	public class RefineModel
	{
		public int Age { get; set; }
	}
}
";

		// Act
		var assembly = await CompileToAssemblyAsync(source, cancellationToken);
		var modelType = assembly.GetType("Testing.RefineModel")!;
		var schemaType = assembly.GetType("Testing.RefineModelSchema")!;

		var instance = Activator.CreateInstance(modelType)!;
		modelType.GetProperty("Age")!.SetValue(instance, 21);

		var refineMethod = schemaType.GetMethod("ApplyRefine")!;
		var result = refineMethod.Invoke(null, [instance, (System.Func<dynamic, bool>)(m => m.Age >= 18), null])!;

		// Assert
		var isSuccess = (bool)result.GetType().GetProperty("IsSuccess")!.GetValue(result)!;
		await Assert.That(isSuccess).IsTrue();
	}
}
