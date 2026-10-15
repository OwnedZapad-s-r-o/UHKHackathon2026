using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace SmartAudit.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingEdits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AreaOverride",
                table: "FacilityBuildings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EditedAt",
                table: "FacilityBuildings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FloorsOverride",
                table: "FacilityBuildings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearOverride",
                table: "FacilityBuildings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FacilityBuildingParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Footprint = table.Column<Polygon>(type: "geometry(Polygon, 4326)", nullable: false),
                    AreaSquareMeters = table.Column<double>(type: "double precision", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityBuildingParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacilityBuildingParts_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacilityBuildingParts_FacilityId",
                table: "FacilityBuildingParts",
                column: "FacilityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacilityBuildingParts");

            migrationBuilder.DropColumn(
                name: "AreaOverride",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "FloorsOverride",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "YearOverride",
                table: "FacilityBuildings");
        }
    }
}
