using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ModelHub.Application.Exceptions;
using ModelHub.Application.Interfaces;
using ModelHub.Domain.Entities;
using ModelHub.Domain.ValueObjects;

namespace ModelHub.Infrastructure.Persistence;

public sealed class InvestmentModelCreationRepository
    : IInvestmentModelCreationRepository
{
    private readonly ModelHubDbContext _db;

    public InvestmentModelCreationRepository(
        ModelHubDbContext db)
    {
        _db = db;
    }

    public Task<bool> ActiveNameExistsAsync(
        ModelName name,
        CancellationToken cancellationToken)
    {
        return _db.InvestmentModels
            .AsNoTracking()
            .AnyAsync(
                model =>
                    model.IsActive
                    && model.NameValue == name,
                cancellationToken);
    }

    public async Task AddAsync(
        InvestmentModel model,
        CancellationToken cancellationToken)
    {
        await _db.InvestmentModels.AddAsync(
            model,
            cancellationToken);

        try
        {
            await _db.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqliteException
            {
                SqliteExtendedErrorCode: 2067
            } sqliteException
            && sqliteException.Message.Contains(
                "InvestmentModels.Name",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new DuplicateInvestmentModelNameException(
                exception);
        }
    }
}
