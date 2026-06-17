using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExploreGambia.API.Migrations.ExploreGambiaAuthDb
{
    /// <inheritdoc />
    public partial class ActivateExistingUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        UPDATE "AspNetUsers"
        SET "IsActive" = TRUE
        WHERE "IsActive" = FALSE;
    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
