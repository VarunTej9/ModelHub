using System;
using System.ComponentModel.DataAnnotations;

namespace ModelHub.Application.Contracts;

public sealed class InvestmentModelCreationContracts
{
    [Required(ErrorMessage = "Model name is required.")]
    [StringLength(
        200,
        ErrorMessage = "Model name cannot exceed 200 characters.")]
    public string Name { get; init; } = string.Empty;

    [StringLength(
        4000,
        ErrorMessage = "Description cannot exceed 4000 characters.")]
    public string? Description { get; init; }

    [Required(ErrorMessage = "Risk level is required.")]
    [RegularExpression(
        "^(Low|Medium|High)$",
        ErrorMessage = "Risk level must be Low, Medium, or High.")]
    public string RiskLevel { get; init; } = string.Empty;
}

