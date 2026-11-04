using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelHub.Application.Contracts;
using ModelHub.Domain.Entities;
using ModelHub.Domain.Models;
using ModelHub.Infrastructure.Persistence;

namespace ModelHub.Api.Controllers;

[ApiController]
[Route("api/investment-models")]
public sealed class InvestmentModelsController(ModelHubDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "AnalystOnly")]
    public async Task<ActionResult<InvestmentModelResponse>> CreateModel(CreateInvestmentModelRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Model name is required."
            });
        }

        var model = new InvestmentModel(
            request.Name.Trim(),
            request.Description,
            userId.Value);

        dbContext.InvestmentModels.Add(model);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }


        return CreatedAtAction(
            nameof(GetModelById),
            new { id = model.Id },
            ToResponse(model));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<InvestmentModelResponse>> GetModelById(
        int id,
        CancellationToken cancellationToken)
    {
        var model = await dbContext.InvestmentModels
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(model));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AnalystOnly")]
    public async Task<ActionResult<InvestmentModelResponse>> UpdateModel(
        int id,
        UpdateInvestmentModelRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Model name is required."
            });
        }

        var model = await dbContext.InvestmentModels
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        if (model.CreatedByUserId != userId.Value)
        {
            return Forbid();
        }

        if (model.Status is not
            (ModelStatus.Draft or ModelStatus.Rejected))
        {
            return Conflict(new
            {
                message = "Only draft or rejected models can be updated."
            });
        }

        model.UpdateDraft(
            request.Name.Trim(),
            request.Description);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(model));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AnalystOnly")]
    public async Task<IActionResult> DeleteModel(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var model = await dbContext.InvestmentModels
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        if (model.CreatedByUserId != userId.Value)
        {
            return Forbid();
        }

        if (model.Status is not
            (ModelStatus.Draft or ModelStatus.Rejected))
        {
            return Conflict(new
            {
                message = "Only draft or rejected models can be deleted."
            });
        }

        dbContext.InvestmentModels.Remove(model);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:int}/submit")]
    [Authorize(Policy = "AnalystOnly")]
    public async Task<ActionResult<InvestmentModelResponse>> SubmitModel(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var model = await dbContext.InvestmentModels
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        if (model.CreatedByUserId != userId.Value)
        {
            return Forbid();
        }

        if (model.Status is not
            (ModelStatus.Draft or ModelStatus.Rejected))
        {
            return Conflict(new
            {
                message = "Only draft or rejected models can be submitted."
            });
        }

        model.Submit();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(model));
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = "ReviewerOnly")]
    public async Task<ActionResult<InvestmentModelResponse>> ApproveModel(
        int id,
        CancellationToken cancellationToken)
    {
        var model = await dbContext.InvestmentModels
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        if (model.Status != ModelStatus.Submitted)
        {
            return Conflict(new
            {
                message = "Only submitted models can be approved."
            });
        }

        model.Approve();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(model));
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = "ReviewerOnly")]
    public async Task<ActionResult<InvestmentModelResponse>> RejectModel(
        int id,
        CancellationToken cancellationToken)
    {
        var model = await dbContext.InvestmentModels
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        if (model.Status != ModelStatus.Submitted)
        {
            return Conflict(new
            {
                message = "Only submitted models can be rejected."
            });
        }

        model.Reject();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(model));
    }

    private int? GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return int.TryParse(value, out var userId)
            ? userId
            : null;
    }

    private static InvestmentModelResponse ToResponse(
        InvestmentModel model)
    {
        return new InvestmentModelResponse(
            model.Id,
            model.Name,
            model.Description,
            model.Status,
            model.CreatedByUserId);
    }
}
