using ZodSharp.Core;

namespace ZodSharp.Schemas;

/// <summary>
/// Schema for array validation.
/// </summary>
/// <typeparam name="T">The element type</typeparam>
public class ZodArray<T> : ZodType<T[], T[]>
{
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
                Array.Empty<string>()
            ));
        }

        var errors = new List<ValidationError>();
        var validatedItems = new List<T>();

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
                Array.Empty<string>()
            ));
        }

        if (_maxLength.HasValue && validatedItems.Count > _maxLength.Value)
        {
            return ValidationResult<T[]>.Failure(new ValidationError(
                "too_big",
                $"Array must have at most {_maxLength.Value} elements, but got {validatedItems.Count}",
                Array.Empty<string>()
            ));
        }

        return ValidationResult<T[]>.Success(validatedItems.ToArray());
    }

    /// <summary>
    /// Sets the minimum array length.
    /// </summary>
    public ZodArray<T> Min(int minLength)
    {
        _minLength = minLength;
        return this;
    }

    /// <summary>
    /// Sets the maximum array length.
    /// </summary>
    public ZodArray<T> Max(int maxLength)
    {
        _maxLength = maxLength;
        return this;
    }

    /// <summary>
    /// Sets the exact array length.
    /// </summary>
    public ZodArray<T> Length(int length)
    {
        _minLength = length;
        _maxLength = length;
        return this;
    }
}

