using System.Collections.Immutable;

namespace ZodSharp.Core;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ZodException : Exception
{
    /// <summary>
    /// The validation errors that caused this exception.
    /// </summary>
    public ImmutableArray<ValidationError> Errors { get; }

    public ZodException(ImmutableArray<ValidationError> errors)
        : base($"Validation failed with {errors.Length} error(s)")
    {
        Errors = errors;
    }

    public ZodException(IEnumerable<ValidationError> errors)
        : this(errors.ToImmutableArray())
    {
    }

    public override string ToString()
    {
        if (Errors.IsDefaultOrEmpty)
            return base.ToString();

        var errorMessages = Errors.Select(e => 
            $"{string.Join(".", e.Path)}: {e.Message} ({e.Code})");
        
        return $"{Message}\n{string.Join("\n", errorMessages)}";
    }
}

