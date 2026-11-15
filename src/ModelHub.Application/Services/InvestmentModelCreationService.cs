using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelHub.Application.Contracts;
using ModelHub.Application.Exceptions;
using ModelHub.Application.Interfaces;
using ModelHub.Domain.Entities;
using ModelHub.Domain.Models;
using ModelHub.Domain.ValueObjects;
using DomainValidationException =
    ModelHub.Domain.Exceptions.ValidationException;

namespace ModelHub.Application.Services;

public sealed class InvestmentModelCreationService
    : IInvestmentModelCreationService
{
    private readonly IInvestmentModelCreationRepository _repository;

    public InvestmentModelCreationService(
        IInvestmentModelCreationRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreatedInvestmentModelResponse> CreateAsync(
        CreateInvestmentModelRequest request,
        int createdByUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var name = ModelName.Create(request.Name);

        var nameExists = await _repository.ActiveNameExistsAsync(
            name,
            cancellationToken);

        if (nameExists)
        {
            throw new DuplicateInvestmentModelNameException();
        }

        var riskLevel = Enum.Parse<RiskLevel>(
            request.RiskLevel,
            ignoreCase: false);

        var model = new InvestmentModel(
            name.Value,
            request.Description,
            riskLevel,
            createdByUserId);

        await _repository.AddAsync(
            model,
            cancellationToken);

        return new CreatedInvestmentModelResponse(
            model.Id,
            model.Name,
            model.Description,
            model.RiskLevel.ToString(),
            model.Status.ToString(),
            model.IsActive,
            model.CreatedByUserId,
            model.CreatedUtc,
            model.UpdatedUtc);
    }


    private static void ValidateRequest(
        CreateInvestmentModelRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            results,
            validateAllProperties: true);

        if (valid)
        {
            return;
        }

        var errors = results
            .SelectMany(result =>
                result.MemberNames
                    .DefaultIfEmpty("Request")
                    .Select(member => new
                    {
                        Member = member,
                        Message = result.ErrorMessage ?? "Invalid value."
                    }))
            .GroupBy(item => item.Member)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.Message)
                    .Distinct()
                    .ToArray());

        throw new DomainValidationException(
            "Validation failed.",
            errors);
    }
}
