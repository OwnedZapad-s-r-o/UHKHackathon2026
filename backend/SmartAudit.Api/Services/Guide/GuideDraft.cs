namespace SmartAudit.Api.Services.Guide
{
    public enum GuideFocus
    {
        None,
        // Jeden objekt — mapa k němu přeletí a otevře panel
        Facility,
        // Několik objektů — mapa je očísluje a ukáže všechny naráz
        Facilities,
        District,
        Region,
    }

    // Odkaz na stránku aplikace — jen relativní adresy (průvodce nikdy neposílá mimo aplikaci)
    public record GuideLink(string Label, string Href);

    // Kontext, ze kterého se uživatel ptá: stránka a co má v mapě vybrané
    public record GuideAskContext(string? Page, Guid? FacilityId, string? DistrictCode);

    // Rozpracovaná odpověď — model i záložní režim ji skládají stejně, handler z ní udělá karty a povel pro mapu
    public record GuideDraft(
        string Answer,
        GuideFocus Focus,
        IReadOnlyList<GuideFacility> Facilities,
        IReadOnlyList<GuideDistrict> Districts,
        string? DistrictCode,
        string? Panel,
        IReadOnlyList<KnowledgeHit> Knowledge,
        bool UsedFacilityData,
        IReadOnlyList<string> Suggestions,
        // Jaký graf odpověď chce — data do něj doplní GuideChartBuilder
        GuideChartRequest Chart = GuideChartRequest.None,
        GuideChartMetric ChartMetric = GuideChartMetric.Auto);
}
