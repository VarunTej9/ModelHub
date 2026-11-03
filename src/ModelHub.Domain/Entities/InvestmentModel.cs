using ModelHub.Domain.Models;

namespace ModelHub.Domain.Entities;

public class InvestmentModel : BaseEntity
{
    private InvestmentModel()
    {
    }

public InvestmentModel(
    string name,
    string? description,
    int createdByUserId)
    : base()
{
    Name = name;
    Description = description;
    CreatedByUserId = createdByUserId;
    Status = ModelStatus.Draft;

    var now = DateTime.UtcNow;
    CreatedUtc = now;
    UpdatedUtc = now;
}


    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public ModelStatus Status { get; private set; } = ModelStatus.Draft;

    public int CreatedByUserId { get; private set; }

    public void UpdateDraft(string name, string? description)
    {
        if (Status is not (ModelStatus.Draft or ModelStatus.Rejected))
        {
            throw new InvalidOperationException(
                "Only draft or rejected models can be updated.");
        }

        Name = name;
        Description = description;
        Status = ModelStatus.Draft;
    }

    public void Submit()
    {
        if (Status is not (ModelStatus.Draft or ModelStatus.Rejected))
        {
            throw new InvalidOperationException(
                "Only draft or rejected models can be submitted.");
        }

        Status = ModelStatus.Submitted;
    }

    public void Approve()
    {
        if (Status != ModelStatus.Submitted)
        {
            throw new InvalidOperationException(
                "Only submitted models can be approved.");
        }

        Status = ModelStatus.Approved;
    }

    public void Reject()
    {
        if (Status != ModelStatus.Submitted)
        {
            throw new InvalidOperationException(
                "Only submitted models can be rejected.");
        }

        Status = ModelStatus.Rejected;
    }
}
