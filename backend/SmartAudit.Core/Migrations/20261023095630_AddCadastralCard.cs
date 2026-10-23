using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAudit.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCadastralCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CadastralTerritory",
                table: "FacilityBuildings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EnclosedVolume",
                table: "FacilityBuildings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeatingType",
                table: "FacilityBuildings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityPart",
                table: "FacilityBuildings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "FacilityBuildings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnershipSheet",
                table: "FacilityBuildings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ParcelArea",
                table: "FacilityBuildings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParcelNumbers",
                table: "FacilityBuildings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RegisteredFloorArea",
                table: "FacilityBuildings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StructureUse",
                table: "FacilityBuildings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CadastralTerritory",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "EnclosedVolume",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "HeatingType",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "MunicipalityPart",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "OwnershipSheet",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "ParcelArea",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "ParcelNumbers",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "RegisteredFloorArea",
                table: "FacilityBuildings");

            migrationBuilder.DropColumn(
                name: "StructureUse",
                table: "FacilityBuildings");
        }
    }
}
