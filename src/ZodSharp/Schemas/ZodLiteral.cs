using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema for literal value validation.
/// </summary>
/// <typeparam name="T">The literal type</typeparam>
public class ZodLiteral<T> : ZodType<T, T> where T : IEquatable<T>
{
    private readonly T _value;

    public ZodLiteral(T value)
    {
        _value = value;
    }

    protected override ValidationResult<T> ParseInternal(T value)
    {
        if (!value.Equals(_value))
        {
            return ValidationResult<T>.Failure(new ValidationError(
                "invalid_literal",
                $"Expected literal value {_value}, but got {value}",
                Array.Empty<string>()
            ));
        }

        return ValidationResult<T>.Success(value);
    }
}

