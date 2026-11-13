using System;
using System.Collections.Generic;
using ModelHub.Domain.Exceptions;
using ModelHub.Domain.Models;
using ModelHub.Domain.ValueObjects;

namespace ModelHub.Domain.Entities;

public sealed class InvestmentModel
{
    private InvestmentModel()
    {
        // Required by Entity Framework Core.
    }

    public InvestmentModel(
        string name,
        string? description,
        RiskLevel riskLevel,
        int createdByUserId)
    {
        var validatedName = ModelName.Create(name);

        ValidateDescription(description);
        ValidateRiskLevel(riskLevel);

        if (createdByUserId <= 0)
        {
            throw new ValidationException(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["CreatedByUserId"] = new[]
                    {
                        "A valid creator is required."
                    }
                });
        }

        NameValue = validatedName;
        Description = NormalizeDescription(description);
        RiskLevel = riskLevel;

        // The caller cannot choose the initial status.
        Status = ModelStatus.Draft;

        IsActive = true;
        CreatedByUserId = createdByUserId;

        var now = DateTime.UtcNow;

        CreatedUtc = now;
        UpdatedUtc = now;
    }

    public int Id { get; private set; }

    public ModelName NameValue { get; private set; } = null!;

    // Preserves the string property used by existing response mapping.
    public string Name => NameValue.Value;

    public string? Description { get; private set; }

    public RiskLevel RiskLevel { get; private set; }

    public ModelStatus Status { get; private set; }

    public bool IsActive { get; private set; }

    public int CreatedByUserId { get; private set; }

    public DateTime CreatedUtc { get; private set; }

    public DateTime UpdatedUtc { get; private set; }

    public void UpdateDraft(
        string name,
        string? description)
    {
        UpdateDraft(name, description, RiskLevel);
    }

    public void UpdateDraft(
        string name,
        string? description,
        RiskLevel riskLevel)
    {
        EnsureActive();

        if (Status is not (ModelStatus.Draft or ModelStatus.Rejected))
        {
            throw new BusinessRuleException(
                "Only draft or rejected models can be updated.");
        }

        // Validate everything before changing state.
        var validatedName = ModelName.Create(name);

        ValidateDescription(description);
        ValidateRiskLevel(riskLevel);

        NameValue = validatedName;
        Description = NormalizeDescription(description);
        RiskLevel = riskLevel;
        Status = ModelStatus.Draft;

        Touch();
    }

    public void Submit()
    {
        EnsureActive();

        if (Status is not (ModelStatus.Draft or ModelStatus.Rejected))
        {
            throw new BusinessRuleException(
                "Only draft or rejected models can be submitted.");
        }

        Status = ModelStatus.Submitted;

        Touch();
    }

    public void Approve()
    {
        EnsureActive();

        if (Status != ModelStatus.Submitted)
        {
            throw new BusinessRuleException(
                "Only submitted models can be approved.");
        }

        Status = ModelStatus.Approved;

        Touch();
    }

    public void Reject()
    {
        EnsureActive();

        if (Status != ModelStatus.Submitted)
        {
            throw new BusinessRuleException(
                "Only submitted models can be rejected.");
        }

        Status = ModelStatus.Rejected;

        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        Touch();
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new BusinessRuleException(
                "Inactive models cannot be changed.");
        }
    }

    private void Touch()
    {
        UpdatedUtc = DateTime.UtcNow;
    }

    private static void ValidateDescription(string? description)
    {
        if (description is not null && description.Length > 4000)
        {
            throw new ValidationException(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["Description"] = new[]
                    {
                        "Description cannot exceed 4000 characters."
                    }
                });
        }
    }

    private static void ValidateRiskLevel(RiskLevel riskLevel)
    {
        if (!Enum.IsDefined(typeof(RiskLevel), riskLevel))
        {
            throw new ValidationException(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["RiskLevel"] = new[]
                    {
                        "Risk level must be Low, Medium, or High."
                    }
                });
        }
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }
}
