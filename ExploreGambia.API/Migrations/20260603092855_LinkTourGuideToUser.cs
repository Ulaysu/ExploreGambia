using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExploreGambia.API.Migrations
{
    /// <inheritdoc />
    public partial class LinkTourGuideToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "TourGuides",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourGuides_UserId",
                table: "TourGuides",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TourGuides_AspNetUsers_UserId",
                table: "TourGuides",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourGuides_AspNetUsers_UserId",
                table: "TourGuides");

            migrationBuilder.DropIndex(
                name: "IX_TourGuides_UserId",
                table: "TourGuides");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TourGuides");
        }
    }
}
