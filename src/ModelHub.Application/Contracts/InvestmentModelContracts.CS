using System;
using System.ComponentModel.DataAnnotations;
using ModelHub.Domain.Models;

namespace ModelHub.Application.Contracts;

public sealed record CreateInvestmentModelRequest(
    [property: Required(ErrorMessage = "Model name is required.")]
    [property: StringLength(
        200,
        ErrorMessage = "Model name cannot exceed 200 characters.")]
    string Name,

    [property: StringLength(
        4000,
        ErrorMessage = "Description cannot exceed 4000 characters.")]
    string? Description,

    [property: Required(ErrorMessage = "Risk level is required.")]
    [property: RegularExpression(
        "^(Low|Medium|High)$",
        ErrorMessage = "Risk level must be Low, Medium, or High.")]
    string RiskLevel);

public sealed record UpdateInvestmentModelRequest(
    string Name,
    string? Description);

public sealed record InvestmentModelResponse(
    int Id,
    string Name,
    string? Description,
    ModelStatus Status,
    int CreatedByUserId);

public sealed record CreatedInvestmentModelResponse(
    int Id,
    string Name,
    string? Description,
    string RiskLevel,
    string Status,
    bool IsActive,
    int CreatedByUserId,
    DateTime CreatedUtc,
    DateTime UpdatedUtc);
