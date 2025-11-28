using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema for union types (one of multiple schemas).
/// </summary>
public class ZodUnion : ZodType<object, object>
{
    private readonly IReadOnlyList<IZodSchema<object, object>> _options;

    public ZodUnion(IReadOnlyList<IZodSchema<object, object>> options)
    {
        _options = options;
    }

    protected override ValidationResult<object> ParseInternal(object value)
    {
        var allErrors = new List<ValidationError>();

        foreach (var option in _options)
        {
            var result = option.Validate(value);
            if (result.IsSuccess)
            {
                return result;
            }

            allErrors.AddRange(result.Errors);
        }

        return ValidationResult<object>.Failure(new ValidationError(
            "invalid_union",
            "Value does not match any of the union options",
            Array.Empty<string>(),
            new Dictionary<string, object?> { { "errors", allErrors } }
        ));
    }
}

