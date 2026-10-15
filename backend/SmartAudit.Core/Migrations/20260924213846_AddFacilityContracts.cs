using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAudit.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ContractsSyncedAt",
                table: "Organizations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FacilityContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VersionId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContractNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Title = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PartnerName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PartnerIco = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AmountWithoutVat = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: true),
                    AmountWithVat = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: true),
                    DateSigned = table.Column<DateOnly>(type: "date", nullable: true),
                    PublishedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AttachmentsJson = table.Column<string>(type: "jsonb", nullable: false),
                    Commodity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UnitPriceCzkPerMwh = table.Column<double>(type: "double precision", nullable: true),
                    PriceSource = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacilityContracts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacilityContracts_ContractId",
                table: "FacilityContracts",
                column: "ContractId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacilityContracts_OrganizationId_Category",
                table: "FacilityContracts",
                columns: new[] { "OrganizationId", "Category" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacilityContracts");

            migrationBuilder.DropColumn(
                name: "ContractsSyncedAt",
                table: "Organizations");
        }
    }
}
