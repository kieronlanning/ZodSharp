using ZodSharp.Core;
using ZodSharp.Schemas;

namespace ZodSharp;

/// <summary>
/// Main entry point for creating Zod schemas.
/// Provides factory methods for all schema types.
/// </summary>
public static class Z
{
	/// <summary>
	/// Creates a string schema.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name")]
	public static ZodString String() => new();

	/// <summary>
	/// Creates a number schema.
	/// </summary>
	public static ZodNumber Number() => new();

	/// <summary>
	/// Creates a boolean schema.
	/// </summary>
	public static ZodBoolean Boolean() => new();

	/// <summary>
	/// Creates a null schema.
	/// </summary>
	public static ZodNull Null() => new();

	/// <summary>
	/// Creates an array schema for the specified element type.
	/// </summary>
	public static ZodArray<T> Array<T>(IZodSchema<T, T> elementSchema) => new(elementSchema);

	/// <summary>
	/// Creates an optional schema wrapper.
	/// </summary>
	public static ZodOptional<T> Optional<T>(IZodSchema<T, T> schema)
		where T : class => new(schema);

	/// <summary>
	/// Creates a nullable schema wrapper (for value types).
	/// </summary>
	public static ZodNullable<T> Nullable<T>(IZodSchema<T, T> schema)
		where T : struct => new(schema);

	/// <summary>
	/// Creates an object schema builder.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name")]
	public static ZodObjectBuilder Object() => new();

	/// <summary>
	/// Creates a union schema (one of multiple options).
	/// </summary>
	public static ZodUnion Union(params IZodSchema<object, object>[] options) => new(options);

	/// <summary>
	/// Creates a typed union schema of two options, yielding a
	/// <see cref="Unions.Union{T1,T2}"/> on success.
	/// </summary>
	/// <typeparam name="T1">The first option's type.</typeparam>
	/// <typeparam name="T2">The second option's type.</typeparam>
	/// <param name="option1">The first option schema.</param>
	/// <param name="option2">The second option schema.</param>
	public static ZodTypedUnion<T1, T2> Union<T1, T2>(IZodSchema<T1, T1> option1, IZodSchema<T2, T2> option2) =>
		new(option1, option2);

	/// <summary>
	/// Creates an intersection schema requiring both <paramref name="left"/> and
	/// <paramref name="right"/> to pass. Equivalent to Zod's
	/// <c>z.intersection(a, b)</c>.
	/// </summary>
	/// <typeparam name="T">The validated type.</typeparam>
	/// <param name="left">The first schema.</param>
	/// <param name="right">The second schema.</param>
	public static ZodIntersection<T> Intersection<T>(IZodSchema<T, T> left, IZodSchema<T, T> right) => new(left, right);

	/// <summary>
	/// Creates a literal schema.
	/// </summary>
	public static ZodLiteral<T> Literal<T>(T value)
		where T : IEquatable<T> => new(value);

	/// <summary>
	/// Creates a lazy schema for recursive or circular references.
	/// </summary>
	public static ZodLazy<T> Lazy<T>(Func<IZodSchema<T, T>> schemaGetter) => new(schemaGetter);

	/// <summary>
	/// Creates a discriminated union builder.
	/// </summary>
	public static ZodDiscriminatedUnionBuilder DiscriminatedUnion(string discriminator) => new(discriminator);
}
