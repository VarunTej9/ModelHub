using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModelHub.Domain.Entities;
using ModelHub.Domain.Models;
using ModelHub.Domain.ValueObjects;

namespace ModelHub.Infrastructure.Persistence;

public sealed class InvestmentModelConfiguration
    : IEntityTypeConfiguration<InvestmentModel>
{
    public void Configure(
        EntityTypeBuilder<InvestmentModel> builder)
    {
        builder.ToTable("InvestmentModels");

        builder.HasKey(model => model.Id);

        builder.Property(model => model.Id)
            .ValueGeneratedOnAdd();

        // Name is a convenience getter.
        // NameValue is the actual persisted value object.
        builder.Ignore(model => model.Name);

        builder.Property(model => model.NameValue)
            .HasColumnName("Name")
            .HasConversion(
                name => name.Value,
                value => ModelName.Create(value))
            .HasMaxLength(ModelName.MaximumLength)
            .UseCollation("NOCASE")
            .IsRequired();

        builder.HasIndex(model => model.NameValue)
            .HasDatabaseName("UX_InvestmentModels_ActiveName")
            .IsUnique()
            .HasFilter("\"IsActive\" = 1");

        builder.Property(model => model.Description)
            .HasMaxLength(4000);

        builder.Property(model => model.RiskLevel)
            .HasConversion<int>()
            .HasDefaultValue(RiskLevel.Medium)
            .IsRequired();

        builder.Property(model => model.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(model => model.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(model => model.CreatedByUserId)
            .IsRequired();

        builder.Property(model => model.CreatedUtc)
            .IsRequired();

        builder.Property(model => model.UpdatedUtc)
            .IsRequired();
    }
}
