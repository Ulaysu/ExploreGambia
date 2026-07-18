using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExploreGambia.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderVerifications",
                columns: table => new
                {
                    ProviderVerificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TourGuideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IssuingCountry = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    MaskedDocumentNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DocumentExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TemporaryDocumentFrontKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TemporaryDocumentBackKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ReviewReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EvidenceDeletionStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EvidenceDeletionAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EvidenceDeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastEvidenceDeletionAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastEvidenceDeletionError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderVerifications", x => x.ProviderVerificationId);
                    table.CheckConstraint("CK_ProviderVerifications_EvidenceDeletionAttempts", "\"EvidenceDeletionAttempts\" >= 0");
                    table.CheckConstraint("CK_ProviderVerifications_EvidenceDeletionStatus", "\"EvidenceDeletionStatus\" BETWEEN 0 AND 3");
                    table.CheckConstraint("CK_ProviderVerifications_Status", "\"Status\" BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "FK_ProviderVerifications_TourGuides_TourGuideId",
                        column: x => x.TourGuideId,
                        principalTable: "TourGuides",
                        principalColumn: "TourGuideId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderVerifications_DocumentExpiryDate",
                table: "ProviderVerifications",
                column: "DocumentExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderVerifications_EvidenceDeletionStatus",
                table: "ProviderVerifications",
                column: "EvidenceDeletionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderVerifications_Status_SubmittedAt",
                table: "ProviderVerifications",
                columns: new[] { "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderVerifications_TourGuideId",
                table: "ProviderVerifications",
                column: "TourGuideId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderVerifications");
        }
    }
}
