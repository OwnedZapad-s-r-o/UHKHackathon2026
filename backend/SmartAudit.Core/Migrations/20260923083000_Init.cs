using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace SmartAudit.Core.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "DataSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DatasetId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ServiceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PortalUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Purpose = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LastImportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastRecordCount = table.Column<int>(type: "integer", nullable: true),
                    LastStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MunicipalityContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MunicipalityCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    MunicipalityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataBox = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Web = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalityContacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ico = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Redizo = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    LegalForm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Web = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Geometry = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: true),
                    OrpCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    IsThreatened = table.Column<bool>(type: "boolean", nullable: false),
                    ThreatLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GrantCount = table.Column<int>(type: "integer", nullable: true),
                    GrantTotal = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: true),
                    IndividualGrantCount = table.Column<int>(type: "integer", nullable: true),
                    IndividualGrantTotal = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: true),
                    ProgramGrantCount = table.Column<int>(type: "integer", nullable: true),
                    ProgramGrantTotal = table.Column<decimal>(type: "numeric(16,2)", precision: 16, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherObservations",
                columns: table => new
                {
                    OrpCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Temperature = table.Column<double>(type: "double precision", nullable: false),
                    Pm10 = table.Column<double>(type: "double precision", nullable: true),
                    Pm25 = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherObservations", x => new { x.OrpCode, x.Timestamp });
                });

            migrationBuilder.CreateTable(
                name: "DataImports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RecordCount = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DataSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataImports_DataSources_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenDataFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Properties = table.Column<string>(type: "jsonb", nullable: false),
                    Geometry = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: true),
                    DataSourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenDataFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenDataFeatures_DataSources_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Facilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Kinds = table.Column<string[]>(type: "text[]", nullable: false),
                    Izos = table.Column<string[]>(type: "text[]", nullable: false),
                    Redizo = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    Ico = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Web = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HouseNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Psc = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MunicipalityCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    MunicipalityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrpCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    OrpName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DistrictCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    DistrictName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Location = table.Column<Point>(type: "geometry(Point, 4326)", nullable: false),
                    HeritageStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HeritageName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SimulationProfile = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EditedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facilities_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsDemo = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FacilityAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Headline = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FindingsJson = table.Column<string>(type: "jsonb", nullable: false),
                    LatestTemperature = table.Column<double>(type: "double precision", nullable: true),
                    LatestCo2 = table.Column<double>(type: "double precision", nullable: true),
                    LatestPower = table.Column<double>(type: "double precision", nullable: true),
                    Co2HoursOverLimit = table.Column<double>(type: "double precision", nullable: false),
                    FreeDayMeanTemperature = table.Column<double>(type: "double precision", nullable: true),
                    NightMeanTemperature = table.Column<double>(type: "double precision", nullable: true),
                    WeeklyEnergyKwh = table.Column<double>(type: "double precision", nullable: false),
                    AnnualLossCzk = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    SavingPercent = table.Column<double>(type: "double precision", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacilityAssessments_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacilityBuildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Found = table.Column<bool>(type: "boolean", nullable: false),
                    Footprint = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: true),
                    BuiltUpArea = table.Column<double>(type: "double precision", nullable: true),
                    ConstructionYear = table.Column<int>(type: "integer", nullable: true),
                    CurrentUse = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Condition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParcelRef = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RuianCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EnergyCategory = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EstimatedFloors = table.Column<int>(type: "integer", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    FetchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityBuildings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacilityBuildings_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacilityMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Students = table.Column<int>(type: "integer", nullable: true),
                    Graduates = table.Column<int>(type: "integer", nullable: true),
                    Admitted = table.Column<int>(type: "integer", nullable: true),
                    BedCapacity = table.Column<int>(type: "integer", nullable: true),
                    Residents = table.Column<int>(type: "integer", nullable: true),
                    Applicants = table.Column<int>(type: "integer", nullable: true),
                    OfferedPlaces = table.Column<int>(type: "integer", nullable: true),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacilityMetrics_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Floor = table.Column<int>(type: "integer", nullable: false),
                    Wing = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsHotspot = table.Column<bool>(type: "boolean", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zones_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Metric = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Seed = table.Column<int>(type: "integer", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstalledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sensors_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sensors_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    SensorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => new { x.SensorId, x.Timestamp });
                    table.ForeignKey(
                        name: "FK_SensorReadings_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataImports_DataSourceId",
                table: "DataImports",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSources_Key",
                table: "DataSources",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_Code",
                table: "Facilities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_Location",
                table: "Facilities",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_OrganizationId",
                table: "Facilities",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_SourceKey",
                table: "Facilities",
                column: "SourceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacilityAssessments_FacilityId",
                table: "FacilityAssessments",
                column: "FacilityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacilityAssessments_Status",
                table: "FacilityAssessments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityBuildings_FacilityId",
                table: "FacilityBuildings",
                column: "FacilityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacilityMetrics_FacilityId_SchoolYear",
                table: "FacilityMetrics",
                columns: new[] { "FacilityId", "SchoolYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MunicipalityContacts_MunicipalityCode",
                table: "MunicipalityContacts",
                column: "MunicipalityCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataFeatures_DataSourceId_ExternalId",
                table: "OpenDataFeatures",
                columns: new[] { "DataSourceId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenDataFeatures_Geometry",
                table: "OpenDataFeatures",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Ico",
                table: "Organizations",
                column: "Ico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Redizo",
                table: "Organizations",
                column: "Redizo");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Geometry",
                table: "Regions",
                column: "Geometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Type_Code",
                table: "Regions",
                columns: new[] { "Type", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_Timestamp",
                table: "SensorReadings",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_FacilityId",
                table: "Sensors",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_ZoneId",
                table: "Sensors",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Zones_FacilityId",
                table: "Zones",
                column: "FacilityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataImports");

            migrationBuilder.DropTable(
                name: "FacilityAssessments");

            migrationBuilder.DropTable(
                name: "FacilityBuildings");

            migrationBuilder.DropTable(
                name: "FacilityMetrics");

            migrationBuilder.DropTable(
                name: "MunicipalityContacts");

            migrationBuilder.DropTable(
                name: "OpenDataFeatures");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WeatherObservations");

            migrationBuilder.DropTable(
                name: "DataSources");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "Zones");

            migrationBuilder.DropTable(
                name: "Facilities");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
