using System.Collections.Immutable;
using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema for discriminated unions.
/// More efficient than regular unions when a discriminator field is present.
/// Equivalent to Zod's discriminatedUnion method.
/// </summary>
public class ZodDiscriminatedUnion : ZodType<object, object>
{
    private readonly string _discriminator;
    private readonly ImmutableDictionary<string, IZodSchema<object, object>> _options;

    public ZodDiscriminatedUnion(
        string discriminator,
        ImmutableDictionary<string, IZodSchema<object, object>> options)
    {
        _discriminator = discriminator;
        _options = options;
    }

    protected override ValidationResult<object> ParseInternal(object value)
    {
        if (value == null)
        {
            return ValidationResult<object>.Failure(new ValidationError(
                "invalid_type",
                "Expected object, but got null",
                Array.Empty<string>()
            ));
        }

        var type = value.GetType();
        var discriminatorProperty = type.GetProperty(_discriminator);
        
        if (discriminatorProperty == null)
        {
            return ValidationResult<object>.Failure(new ValidationError(
                "missing_discriminator",
                $"Discriminator field '{_discriminator}' not found",
                Array.Empty<string>()
            ));
        }

        var discriminatorValue = discriminatorProperty.GetValue(value)?.ToString();
        
        if (discriminatorValue == null || !_options.TryGetValue(discriminatorValue, out var schema))
        {
            return ValidationResult<object>.Failure(new ValidationError(
                "invalid_discriminator",
                $"Invalid discriminator value '{discriminatorValue}'. Expected one of: {string.Join(", ", _options.Keys)}",
                Array.Empty<string>()
            ));
        }

        return schema.Validate(value);
    }
}

/// <summary>
/// Builder for creating discriminated unions.
/// </summary>
public class ZodDiscriminatedUnionBuilder
{
    private readonly string _discriminator;
    private readonly Dictionary<string, IZodSchema<object, object>> _options = new();

    public ZodDiscriminatedUnionBuilder(string discriminator)
    {
        _discriminator = discriminator;
    }

    /// <summary>
    /// Adds an option to the discriminated union.
    /// </summary>
    public ZodDiscriminatedUnionBuilder Option(string value, IZodSchema<object, object> schema)
    {
        _options[value] = schema;
        return this;
    }

    /// <summary>
    /// Builds the discriminated union schema.
    /// </summary>
    public ZodDiscriminatedUnion Build()
    {
        return new ZodDiscriminatedUnion(_discriminator, _options.ToImmutableDictionary());
    }
}

