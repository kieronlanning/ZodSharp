using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema for array validation.
/// </summary>
/// <typeparam name="T">The element type</typeparam>
public class ZodArray<T> : ZodType<T[], T[]>
{
    private static readonly string[] EmptyPath = Array.Empty<string>();

    private readonly IZodSchema<T, T> _elementSchema;
    private int? _minLength;
    private int? _maxLength;

    public ZodArray(IZodSchema<T, T> elementSchema)
    {
        _elementSchema = elementSchema;
    }

    protected override ValidationResult<T[]> ParseInternal(T[] value)
    {
        if (value == null)
        {
            return ValidationResult<T[]>.Failure(new ValidationError(
                "invalid_type",
                "Expected array, but got null",
                EmptyPath
            ));
        }

        var errors = new List<ValidationError>(value.Length);
        var validatedItems = new List<T>(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            var itemResult = _elementSchema.Validate(value[i]);
            if (!itemResult.IsSuccess)
            {
                foreach (var error in itemResult.Errors)
                {
                    var path = new string[error.Path.Length + 1];
                    Array.Copy(error.Path, path, error.Path.Length);
                    path[error.Path.Length] = $"[{i}]";
                    errors.Add(new ValidationError(error.Code, error.Message, path, error.Parameters));
                }
            }
            else
            {
                validatedItems.Add(itemResult.Value!);
            }
        }

        if (errors.Count > 0)
        {
            return ValidationResult<T[]>.Failure(errors);
        }

        if (_minLength.HasValue && validatedItems.Count < _minLength.Value)
        {
            return ValidationResult<T[]>.Failure(new ValidationError(
                "too_small",
                $"Array must have at least {_minLength.Value} elements, but got {validatedItems.Count}",
                EmptyPath
            ));
        }

        if (_maxLength.HasValue && validatedItems.Count > _maxLength.Value)
        {
            return ValidationResult<T[]>.Failure(new ValidationError(
                "too_big",
                $"Array must have at most {_maxLength.Value} elements, but got {validatedItems.Count}",
                EmptyPath
            ));
        }

        return ValidationResult<T[]>.Success(validatedItems.ToArray());
    }

    public ZodArray<T> Min(int minLength)
    {
        _minLength = minLength;
        return this;
    }

    public ZodArray<T> Max(int maxLength)
    {
        _maxLength = maxLength;
        return this;
    }

    public ZodArray<T> Length(int length)
    {
        _minLength = length;
        _maxLength = length;
        return this;
    }

    public ZodArray<T> NonEmpty(string? message = null)
    {
        _minLength = 1;
        return this;
    }
}

