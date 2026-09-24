using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAudit.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddPhysicalSensors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeviceId",
                table: "Sensors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sensors",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSeenAt",
                table: "Sensors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastValue",
                table: "Sensors",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Sensors",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Simulated");

            migrationBuilder.CreateTable(
                name: "SensorDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GatewayId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PollSeconds = table.Column<int>(type: "integer", nullable: false),
                    NodeAddresses = table.Column<int[]>(type: "integer[]", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorDevices_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensorChannels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NodeAddress = table.Column<int>(type: "integer", nullable: false),
                    IqrfType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Metric = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastValue = table.Column<double>(type: "double precision", nullable: true),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SensorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorChannels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorChannels_SensorDevices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "SensorDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SensorChannels_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_DeviceId",
                table: "Sensors",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorDevices_FacilityId",
                table: "SensorDevices",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorDevices_GatewayId",
                table: "SensorDevices",
                column: "GatewayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorChannels_DeviceId_ChannelKey",
                table: "SensorChannels",
                columns: new[] { "DeviceId", "ChannelKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorChannels_SensorId",
                table: "SensorChannels",
                column: "SensorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_SensorDevices_DeviceId",
                table: "Sensors",
                column: "DeviceId",
                principalTable: "SensorDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_SensorDevices_DeviceId",
                table: "Sensors");

            migrationBuilder.DropTable(
                name: "SensorChannels");

            migrationBuilder.DropTable(
                name: "SensorDevices");

            migrationBuilder.DropIndex(
                name: "IX_Sensors_DeviceId",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "LastValue",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Sensors");
        }
    }
}
