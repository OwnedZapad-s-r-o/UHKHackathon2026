using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Data.Tables;

namespace SmartAudit.Core.Data
{
    public class SmartAuditDbContext : DbContext
    {
        public SmartAuditDbContext(DbContextOptions<SmartAuditDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<DataSource> DataSources { get; set; }
        public DbSet<DataImport> DataImports { get; set; }
        public DbSet<OpenDataFeature> OpenDataFeatures { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<FacilityMetric> FacilityMetrics { get; set; }
        public DbSet<FacilityBuilding> FacilityBuildings { get; set; }
        public DbSet<FacilityAssessment> FacilityAssessments { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<MunicipalityContact> MunicipalityContacts { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }
        public DbSet<SensorDevice> SensorDevices { get; set; }
        public DbSet<SensorChannel> SensorChannels { get; set; }
        public DbSet<WeatherObservation> WeatherObservations { get; set; }
        public DbSet<AiAnalysis> AiAnalyses { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("postgis");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartAuditDbContext).Assembly);
        }
    }
}
