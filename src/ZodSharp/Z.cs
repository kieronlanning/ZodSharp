using ZodSharp.Core;
using ZodSharp.JsonSchema;
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
    public static ZodOptional<T> Optional<T>(IZodSchema<T, T> schema) where T : class => new(schema);
    
    /// <summary>
    /// Creates a nullable schema wrapper (for value types).
    /// </summary>
    public static ZodNullable<T> Nullable<T>(IZodSchema<T, T> schema) where T : struct => new(schema);
    
    /// <summary>
    /// Creates an object schema builder.
    /// </summary>
    public static ZodObjectBuilder Object() => new();
    
    /// <summary>
    /// Creates a union schema (one of multiple options).
    /// </summary>
    public static ZodUnion Union(params IZodSchema<object, object>[] options) => new(options);
    
    /// <summary>
    /// Creates a literal schema.
    /// </summary>
    public static ZodLiteral<T> Literal<T>(T value) where T : IEquatable<T> => new(value);
    
    /// <summary>
    /// Creates a lazy schema for recursive or circular references.
    /// </summary>
    public static ZodLazy<T> Lazy<T>(Func<IZodSchema<T, T>> schemaGetter) => new(schemaGetter);
    
    /// <summary>
    /// Creates a discriminated union builder.
    /// </summary>
    public static ZodDiscriminatedUnionBuilder DiscriminatedUnion(string discriminator) => new(discriminator);
    
    // ========== JSON Schema Interoperability ==========
    
    /// <summary>
    /// Converts a ZodSharp schema to JSON Schema (Draft 2020-12).
    /// Enables cross-platform schema sharing with TypeScript Zod.
    /// </summary>
    /// <typeparam name="T">The schema output type</typeparam>
    /// <param name="schema">The ZodSharp schema to convert</param>
    /// <param name="options">Conversion options</param>
    /// <returns>A JSON Schema definition</returns>
    /// <example>
    /// <code>
    /// var schema = Z.Object()
    ///     .Field("name", Z.String().Min(1))
    ///     .Field("age", Z.Number().Min(0))
    ///     .Build();
    /// 
    /// var jsonSchema = Z.ToJsonSchema(schema);
    /// // => { "type": "object", "properties": { "name": { "type": "string", "minLength": 1 }, ... } }
    /// </code>
    /// </example>
    public static JsonSchemaDefinition ToJsonSchema<T>(IZodSchema<T, T> schema, ToJsonSchemaOptions? options = null)
        => ToJsonSchemaConverter.Convert(schema, options);
    
    /// <summary>
    /// Creates a ZodSharp schema from a JSON Schema definition.
    /// Enables consuming schemas defined in TypeScript Zod.
    /// </summary>
    /// <param name="schema">The JSON Schema definition</param>
    /// <param name="options">Parsing options</param>
    /// <returns>A ZodSharp schema that validates according to the JSON Schema</returns>
    /// <example>
    /// <code>
    /// var jsonSchema = new JsonSchemaDefinition
    /// {
    ///     Type = "object",
    ///     Properties = new Dictionary&lt;string, JsonSchemaDefinition&gt;
    ///     {
    ///         ["name"] = new() { Type = "string", MinLength = 1 },
    ///         ["age"] = new() { Type = "number", Minimum = 0 }
    ///     },
    ///     Required = new List&lt;string&gt; { "name", "age" }
    /// };
    /// 
    /// var schema = Z.FromJsonSchema(jsonSchema);
    /// var result = schema.Validate(userData);
    /// </code>
    /// </example>
    public static IZodSchema<object, object> FromJsonSchema(JsonSchemaDefinition schema, FromJsonSchemaOptions? options = null)
        => FromJsonSchemaParser.Parse(schema, options);
    
    /// <summary>
    /// Creates a ZodSharp schema from a JSON Schema string.
    /// Enables consuming schemas defined in TypeScript Zod via JSON files or APIs.
    /// </summary>
    /// <param name="jsonSchema">The JSON Schema as a string</param>
    /// <param name="options">Parsing options</param>
    /// <returns>A ZodSharp schema that validates according to the JSON Schema</returns>
    /// <example>
    /// <code>
    /// var jsonSchemaString = File.ReadAllText("schema.json");
    /// var schema = Z.FromJsonSchema(jsonSchemaString);
    /// var result = schema.Validate(userData);
    /// </code>
    /// </example>
    public static IZodSchema<object, object> FromJsonSchema(string jsonSchema, FromJsonSchemaOptions? options = null)
        => FromJsonSchemaParser.Parse(jsonSchema, options);
}

