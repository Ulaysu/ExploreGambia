using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExploreGambia.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftTourDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tours",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tours");
        }
    }
}
