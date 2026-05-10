using ExploreGambia.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExploreGambia.API.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ExploreGambiaDbContext))]
    [Migration("20260510120000_AddPaymentStatusLifecycle")]
    public partial class AddPaymentStatusLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderReference",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE Payments SET Status = CASE WHEN IsSuccessful = 1 THEN 2 ELSE 0 END");

            migrationBuilder.DropColumn(
                name: "IsSuccessful",
                table: "Payments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSuccessful",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE Payments SET IsSuccessful = CASE WHEN Status = 2 THEN 1 ELSE 0 END");

            migrationBuilder.DropColumn(
                name: "ProviderReference",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Payments");
        }
    }
}
