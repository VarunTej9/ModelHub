using ModelHub.Domain.Models;

namespace ModelHub.Application.Contracts;

public sealed record CreateInvestmentModelRequest(
    string Name,
    string? Description);

public sealed record UpdateInvestmentModelRequest(
    string Name,
    string? Description);

public sealed record InvestmentModelResponse(
    int Id,
    string Name,
    string? Description,
    ModelStatus Status,
    int CreatedByUserId);
