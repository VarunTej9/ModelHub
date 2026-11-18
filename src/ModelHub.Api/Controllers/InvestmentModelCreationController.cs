using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelHub.Api.Contracts;
using ModelHub.Application.Contracts;
using ModelHub.Application.Interfaces;

namespace ModelHub.Api.Controllers;

[ApiController]
[Route("api/investment-models")]
[Authorize(Policy = "AnalystOnly")]
public sealed class InvestmentModelCreationController : ControllerBase
{
    private readonly IInvestmentModelCreationService _service;

    public InvestmentModelCreationController(
        IInvestmentModelCreationService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CreatedInvestmentModelResponse),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatedInvestmentModelResponse>> Create(
        [FromBody] CreateInvestmentModelRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId)
            || userId <= 0)
        {
            return Unauthorized(
                new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "A valid authenticated user is required.",
                    HttpContext.TraceIdentifier));
        }

        var response = await _service.CreateAsync(
            request,
            userId,
            cancellationToken);

        return Created(
            $"/api/investment-models/{response.Id}",
            response);
    }
}
