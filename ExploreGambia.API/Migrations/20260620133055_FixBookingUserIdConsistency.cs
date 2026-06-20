using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExploreGambia.API.Migrations
{
    /// <inheritdoc />
    public partial class FixBookingUserIdConsistency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clean orphaned UserId references BEFORE FK enforcement
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
