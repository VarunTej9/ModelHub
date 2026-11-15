using System.Threading;
using System.Threading.Tasks;
using ModelHub.Application.Contracts;

namespace ModelHub.Application.Interfaces;

public interface IInvestmentModelCreationService
{
    Task<CreatedInvestmentModelResponse> CreateAsync(
        CreateInvestmentModelRequest request,
        int createdByUserId,
        CancellationToken cancellationToken = default);
}
