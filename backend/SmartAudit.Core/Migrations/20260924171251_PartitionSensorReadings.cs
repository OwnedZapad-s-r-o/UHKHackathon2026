using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAudit.Core.Migrations
{
    /// <summary>
    /// Tabulka měření se rozdělí na měsíční oddíly (declarative partitioning podle Timestamp):
    /// dotazy na posledních 7 dní čtou jen poslední oddíl(y), staré měsíce jde levně odpojit a archivovat
    /// a index nad časem je BRIN místo B-tree. Existující data se přesunou do nové tabulky.
    /// </summary>
    public partial class PartitionSensorReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "SensorReadings" RENAME TO "SensorReadings_Old";
                ALTER TABLE "SensorReadings_Old" RENAME CONSTRAINT "PK_SensorReadings" TO "PK_SensorReadings_Old";
                ALTER TABLE "SensorReadings_Old" DROP CONSTRAINT "FK_SensorReadings_Sensors_SensorId";
                DROP INDEX "IX_SensorReadings_Timestamp";

                CREATE TABLE "SensorReadings" (
                    "SensorId" uuid NOT NULL,
                    "Timestamp" timestamp with time zone NOT NULL,
                    "Value" double precision NOT NULL,
                    CONSTRAINT "PK_SensorReadings" PRIMARY KEY ("SensorId", "Timestamp"),
                    CONSTRAINT "FK_SensorReadings_Sensors_SensorId" FOREIGN KEY ("SensorId") REFERENCES "Sensors" ("Id") ON DELETE CASCADE
                ) PARTITION BY RANGE ("Timestamp");

                -- Pojistka pro měření mimo založené měsíce (např. špatné hodiny gatewaye); běžně zůstává prázdná
                CREATE TABLE "SensorReadings_default" PARTITION OF "SensorReadings" DEFAULT;

                -- Oddíl pro každý měsíc s daty a dva měsíce dopředu (další zakládá ReadingPartitionService)
                DO $$
                DECLARE
                    month date;
                    last_month date := (date_trunc('month', now()) + interval '2 months')::date;
                BEGIN
                    SELECT date_trunc('month', coalesce(min("Timestamp"), now()))::date INTO month FROM "SensorReadings_Old";
                    WHILE month <= last_month LOOP
                        EXECUTE format('CREATE TABLE IF NOT EXISTS %I PARTITION OF "SensorReadings" FOR VALUES FROM (%L) TO (%L)',
                            'SensorReadings_' || to_char(month, 'YYYY_MM'), month, (month + interval '1 month')::date);
                        month := (month + interval '1 month')::date;
                    END LOOP;
                END $$;

                INSERT INTO "SensorReadings" ("SensorId", "Timestamp", "Value")
                SELECT "SensorId", "Timestamp", "Value" FROM "SensorReadings_Old";

                DROP TABLE "SensorReadings_Old";

                CREATE INDEX "IX_SensorReadings_Timestamp" ON "SensorReadings" USING brin ("Timestamp");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "SensorReadings" RENAME TO "SensorReadings_Partitioned";
                ALTER TABLE "SensorReadings_Partitioned" RENAME CONSTRAINT "PK_SensorReadings" TO "PK_SensorReadings_Partitioned";
                ALTER TABLE "SensorReadings_Partitioned" DROP CONSTRAINT "FK_SensorReadings_Sensors_SensorId";
                DROP INDEX "IX_SensorReadings_Timestamp";

                CREATE TABLE "SensorReadings" (
                    "SensorId" uuid NOT NULL,
                    "Timestamp" timestamp with time zone NOT NULL,
                    "Value" double precision NOT NULL,
                    CONSTRAINT "PK_SensorReadings" PRIMARY KEY ("SensorId", "Timestamp"),
                    CONSTRAINT "FK_SensorReadings_Sensors_SensorId" FOREIGN KEY ("SensorId") REFERENCES "Sensors" ("Id") ON DELETE CASCADE
                );

                INSERT INTO "SensorReadings" ("SensorId", "Timestamp", "Value")
                SELECT "SensorId", "Timestamp", "Value" FROM "SensorReadings_Partitioned";

                DROP TABLE "SensorReadings_Partitioned";

                CREATE INDEX "IX_SensorReadings_Timestamp" ON "SensorReadings" ("Timestamp");
                """);
        }
    }
}
