using ZodSharp.Core;
using ZodSharp.Schemas;

namespace ZodSharp;

public static class Z
{
    public static ZodString String() => new();
    public static ZodNumber Number() => new();
    public static ZodBoolean Boolean() => new();
    public static ZodNull Null() => new();
    public static ZodArray<T> Array<T>(IZodSchema<T, T> elementSchema) => new(elementSchema);
    public static ZodOptional<T> Optional<T>(IZodSchema<T, T> schema) where T : class => new(schema);
    public static ZodNullable<T> Nullable<T>(IZodSchema<T, T> schema) where T : struct => new(schema);
    public static ZodObjectBuilder Object() => new();
    public static ZodUnion Union(params IZodSchema<object, object>[] options) => new(options);
    public static ZodLiteral<T> Literal<T>(T value) where T : IEquatable<T> => new(value);
    public static ZodLazy<T> Lazy<T>(Func<IZodSchema<T, T>> schemaGetter) => new(schemaGetter);
    public static ZodDiscriminatedUnionBuilder DiscriminatedUnion(string discriminator) => new(discriminator);
}

