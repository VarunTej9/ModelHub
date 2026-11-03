using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModelHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndInvestmentModelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedUtc",
                table: "InvestmentModels",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "InvestmentModels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "InvestmentModels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "InvestmentModels");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "InvestmentModels");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedUtc",
                table: "InvestmentModels",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");
        }
    }
}
