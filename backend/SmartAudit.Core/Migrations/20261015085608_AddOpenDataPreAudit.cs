using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAudit.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenDataPreAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Elevation",
                table: "Regions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                table: "DataSources",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LiveAvailable",
                table: "DataSources",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LiveError",
                table: "DataSources",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LiveRecordCount",
                table: "DataSources",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LiveResponseMs",
                table: "DataSources",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LiveVersion",
                table: "DataSources",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FacilityPreAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Profile = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DominantDriver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParametersJson = table.Column<string>(type: "jsonb", nullable: false),
                    FindingsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SpecificHeatLoss = table.Column<double>(type: "double precision", nullable: false),
                    HeatLossWattsPerKelvin = table.Column<double>(type: "double precision", nullable: false),
                    DegreeDays = table.Column<double>(type: "double precision", nullable: false),
                    AnnualHeatDemandKwh = table.Column<double>(type: "double precision", nullable: false),
                    AnnualHeatCostCzk = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    HeatPriceCzkPerKwh = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    HeatPriceFromContract = table.Column<bool>(type: "boolean", nullable: false),
                    RegulationSavingPercent = table.Column<double>(type: "double precision", nullable: false),
                    RegulationPotentialCzk = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    IqrfGateways = table.Column<int>(type: "integer", nullable: false),
                    IqrfSensors = table.Column<int>(type: "integer", nullable: false),
                    IqrfHardwareCostCzk = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityPreAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacilityPreAudits_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacilityPreAudits_FacilityId",
                table: "FacilityPreAudits",
                column: "FacilityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacilityPreAudits_Score",
                table: "FacilityPreAudits",
                column: "Score");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacilityPreAudits");

            migrationBuilder.DropColumn(
                name: "Elevation",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "DataSources");

            migrationBuilder.DropColumn(
                name: "LiveAvailable",
                table: "DataSources");

            migrationBuilder.DropColumn(
                name: "LiveError",
                table: "DataSources");

            migrationBuilder.DropColumn(
                name: "LiveRecordCount",
                table: "DataSources");

            migrationBuilder.DropColumn(
                name: "LiveResponseMs",
                table: "DataSources");

            migrationBuilder.DropColumn(
                name: "LiveVersion",
                table: "DataSources");
        }
    }
}
