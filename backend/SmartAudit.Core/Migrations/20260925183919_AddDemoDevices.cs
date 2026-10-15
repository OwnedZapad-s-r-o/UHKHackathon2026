using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAudit.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LossBeforeSensorsCzk",
                table: "FacilityAssessments",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SensorsSince",
                table: "FacilityAssessments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DemoDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Serial = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Model = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Code = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Templates = table.Column<string[]>(type: "text[]", nullable: false),
                    Behavior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ParentGatewayId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NodeAddress = table.Column<int>(type: "integer", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoDevices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DemoDevices_ParentGatewayId",
                table: "DemoDevices",
                column: "ParentGatewayId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoDevices_Serial",
                table: "DemoDevices",
                column: "Serial",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DemoDevices");

            migrationBuilder.DropColumn(
                name: "LossBeforeSensorsCzk",
                table: "FacilityAssessments");

            migrationBuilder.DropColumn(
                name: "SensorsSince",
                table: "FacilityAssessments");
        }
    }
}
