namespace ZodSharp.SourceGenerators;

partial class ZodSchemaGeneratorTests
{
	[Test]
	public async Task Generate_GivenEmptyZodSchema_OutputCompilationHasNoErrors(
		CancellationToken cancellationToken
	)
	{
		// Arrange
		const string source =
			@"
namespace Testing
{
	[ZodSchema]
	class BasicModel { }
}
";

		// Act
		var driverResult = await GenerateAsync(source, cancellationToken);

		// Assert
		await AssertNoGeneratorExceptions(driverResult);
		await AssertNoCompilationErrors(driverResult, cancellationToken);
	}

	[Test]
	[Arguments("public")]
	[Arguments("private")]
	[Arguments("internal")]
	// This is internal too
	[Arguments("")]
	public async Task Generate_GivenClassModifier_GeneratesSchemaWithSameModifier(
		string modifier,
		CancellationToken cancellationToken
	)
	{
		// Arrange
		var expectation = $"{modifier} static partial class ModifierTestSchema";

		var source =
			$@"
namespace Testing
{{
	[ZodSchema]
	{modifier} class ModifierTest {{ }}
}}
";

		// Act
		var driverResult = await GenerateAsync(source, cancellationToken);
		var generatedSource = GetSchemaGeneratedSource(driverResult);

		// Assert — generated file starts with auto-generated header
		await Assert.That(generatedSource).Contains(expectation);
	}
}
