using System.Threading;
using System.Threading.Tasks;
using ModelHub.Domain.Entities;
using ModelHub.Domain.ValueObjects;

namespace ModelHub.Application.Interfaces;

public interface IInvestmentModelCreationRepository
{
    Task<bool> ActiveNameExistsAsync(
        ModelName name,
        CancellationToken cancellationToken);

    Task AddAsync(
        InvestmentModel model,
        CancellationToken cancellationToken);
}
