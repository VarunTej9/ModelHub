using System.Collections.Generic;
using ModelHub.Domain.Exceptions;

namespace ModelHub.Domain.ValueObjects;

public sealed record ModelName
{
    public const int MaximumLength = 200;

    private ModelName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ModelName Create(string? value)
    {
        var trimmedValue = value?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            throw new ValidationException(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["Name"] = new[]
                    {
                        "Model name is required."
                    }
                });
        }

        if (trimmedValue.Length > MaximumLength)
        {
            throw new ValidationException(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["Name"] = new[]
                    {
                        $"Model name cannot exceed {MaximumLength} characters."
                    }
                });
        }

        return new ModelName(trimmedValue);
    }

    public override string ToString()
    {
        return Value;
    }
}
