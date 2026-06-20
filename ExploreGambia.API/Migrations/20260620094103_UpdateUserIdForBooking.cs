using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore.Migrations;
using static System.Runtime.InteropServices.JavaScript.JSType;

#nullable disable

namespace ExploreGambia.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserIdForBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        UPDATE ""Bookings""
        SET ""UserId"" = NULL
        WHERE ""UserId"" IS NOT NULL
        AND ""UserId"" NOT IN (
            SELECT ""Id"" FROM ""AspNetUsers""
        );
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
