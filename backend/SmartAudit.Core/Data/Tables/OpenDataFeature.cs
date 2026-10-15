using NetTopologySuite.Geometries;

namespace SmartAudit.Core.Data.Tables
{
    // Surový záznam z otevřené datové sady — ukládá se beze změny, doménové tabulky se z něj odvozují
    public class OpenDataFeature
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Obsah záznamu
        public string ExternalId { get; private set; } = string.Empty;
        public string Properties { get; private set; } = "{}";
        public Geometry? Geometry { get; private set; }


        // Cizí klíče
        public Guid DataSourceId { get; private set; }


        // Navigační vlastnosti
        public DataSource DataSource { get; set; } = null!;

        private OpenDataFeature() { }

        public OpenDataFeature(Guid dataSourceId, string externalId, string properties, Geometry? geometry = null)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("Identifikátor záznamu nesmí být prázdný.", nameof(externalId));

            DataSourceId = dataSourceId;
            ExternalId = externalId;
            Properties = string.IsNullOrWhiteSpace(properties) ? "{}" : properties;
            Geometry = geometry;
        }
    }
}
