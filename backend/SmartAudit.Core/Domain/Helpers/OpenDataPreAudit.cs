using System.Globalization;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Vstup pre-auditu — jen údaje z otevřených dat u objektu (žádná náhoda, žádný hash).
    public record PreAuditInput(
        FacilityCategory Category,
        string[] Kinds,

        // A — tepelná obálka (ČÚZK INSPIRE Budovy + opravy správce) a investice ze smluv (Registr smluv)
        int? ConstructionYear,
        EnergyCategory EnergyCategory,
        double FloorArea,
        int Floors,
        bool AreaFromCadastre,
        ContractEvidence Contracts,
        HeritageStatus Heritage,

        // B — klima lokality (Open-Meteo: nadmořská výška a teplota posledních dní v ORP; hranice ORP z Data KHK)
        string OrpName,
        double? Elevation,
        bool ElevationFromApi,
        double? MeanOutdoor7Days,
        double? RegionMeanOutdoor7Days,

        // C — ekonomika ze smluv (Registr smluv): cena tepla organizace a krajský medián
        decimal HeatPriceCzkPerKwh,
        bool HeatPriceFromContract,
        decimal RegionalMedianHeatPriceCzkPerKwh,

        // D — kapacita osob (Data KHK: žáci, studenti, lůžka) na podlahovou plochu (ČÚZK)
        int? Occupants,
        string? OccupantsLabel,

        // E — okolí (Data KHK: silnice II./III. třídy, IZS, HSOO, dotace; Open-Meteo Air Quality)
        double? DistanceToRegionalRoadMeters,
        string? NearestRoadLabel,
        double? MeanPm10,
        double? DistanceToIzsKm,
        string? NearestIzsLabel,
        bool IsThreatenedArea);

    // Jeden z pěti parametrů modelu — body, vysvětlení a zdrojové sady (klíče z katalogu zdrojů, v UI proklik do katalogu)
    public record PreAuditParameter(string Key, string Name, int Score, int MaxScore, string Value, string Explanation, string[] Sources);

    public record PreAuditFinding(string Title, string Detail, string Tag, AuditStatus Severity, string Source, string[] SourceKeys);

    public record PreAuditResult(
        int Score,
        AuditStatus Status,
        SimulationProfile Profile,
        string DominantDriver,
        IReadOnlyList<PreAuditParameter> Parameters,
        IReadOnlyList<PreAuditFinding> Findings,
        double SpecificHeatLoss,
        double HeatLossWattsPerKelvin,
        double DegreeDays,
        double AnnualHeatDemandKwh,
        decimal AnnualHeatCostCzk,
        double RegulationSavingPercent,
        decimal RegulationPotentialCzk,
        IqrfKit IqrfKit);

    // Vícekriteriální stavebně-energetický a mikroklimatický pre-audit z otevřených dat.
    // Deterministický: stejná data = stejné skóre, stav i profil digitálního dvojčete (fyzikální model telemetrie,
    // dokud budova nemá skutečné čidlo IQRF). Každé zjištění cituje sady, ze kterých vzešlo.
    public static class OpenDataPreAudit
    {
        // Prahy z kalibrace na portfoliu kraje (seed ČÚZK + Data KHK): ~p75 skóre = varování, ~p95 = kritický stav
        public const int CriticalScore = 46;
        public const int WarningScore = 38;

        // Podíl úspory chytré regulace (útlum podle rozvrhu, zónová regulace) na ročních nákladech na teplo
        private const double MinRegulationSaving = 4;
        private const double MaxRegulationSaving = 15;

        // Typická hustota osob na m² podlahové plochy (hrubá plocha včetně chodeb) — 100 % zátěže
        private static double ReferenceDensity(FacilityCategory category) => category switch
        {
            FacilityCategory.School => 0.12,
            FacilityCategory.Social => 0.035,
            FacilityCategory.Health => 0.03,
            _ => 0.02,
        };

        private static readonly CultureInfo Cs = CultureInfo.GetCultureInfo("cs-CZ");

        public static PreAuditResult Evaluate(PreAuditInput input)
        {
            var findings = new List<PreAuditFinding>();
            var c = input.Contracts;

            // ---------- A: Tepelná obálka + regulace ----------
            var baseLoss = BuildingEstimator.SpecificHeatLoss(input.EnergyCategory);
            var specificLoss = c.EnvelopeRetrofit != null ? Math.Round(baseLoss * 0.65, 2) : baseLoss;
            var envelopePoints = input.EnergyCategory switch
            {
                EnergyCategory.Before1945 => 18,
                EnergyCategory.From1946To1975 => 22,
                EnergyCategory.From1976To1990 => 19,
                EnergyCategory.From1991To2005 => 9,
                EnergyCategory.After2006 => 3,
                _ => 13,
            };

            if (c.EnvelopeRetrofit != null)
                envelopePoints = (int)Math.Round(envelopePoints * 0.35);

            // Regulace: bez servisní smlouvy na MaR / kotelnu je nejpravděpodobnější ruční nebo zastaralé řízení vytápění
            var controlPoints = !c.RegistryKnown ? 5 : c.ControlService == null ? 11 : 2;
            if (c.HeatingModernization != null)
                controlPoints = Math.Max(0, controlPoints - 4);

            var paramA = Math.Min(35, envelopePoints + controlPoints);
            var yearText = input.ConstructionYear is { } year ? $"rok výstavby {year}" : "rok výstavby ČÚZK neuvádí";
            var retrofitText = c.EnvelopeRetrofit != null
                ? $"zateplení/okna doložena smlouvou „{Short(c.EnvelopeRetrofit.Title)}“"
                : c.RegistryKnown ? "bez smlouvy o zateplení či výměně oken" : "smlouvy organizace zatím nenačteny";
            var controlText = c.ControlService != null
                ? $"servis MaR/kotelny: „{Short(c.ControlService.Title)}“"
                : c.RegistryKnown ? "bez platné servisní smlouvy na MaR a kotelnu" : "servis MaR neověřen";

            var parameterA = new PreAuditParameter(
                "A",
                "Tepelná obálka a regulace",
                paramA,
                35,
                $"{F(specificLoss, "0.0#")} W/m²K",
                $"{Capitalize(yearText)} ({BuildingEstimator.CategoryLabel(input.EnergyCategory)}), {retrofitText}; {controlText}.",
                ["cuzk-budovy", "registr-smluv", "organizace-khk"]);

            if (paramA >= 24)
            {
                findings.Add(new PreAuditFinding(
                    c.ControlService == null ? "Vysoká měrná tepelná ztráta obálky a absence MaR regulace" : "Vysoká měrná tepelná ztráta obálky",
                    $"{Capitalize(yearText)} → měrná ztráta {F(specificLoss, "0.0#")} W/m²K na {F(input.FloorArea, "N0")} m² podlahy; {retrofitText}; {controlText}. U takové budovy chytrá regulace s čidly v místnostech uspoří nejvíc.",
                    "Obálka budovy",
                    paramA >= 30 ? AuditStatus.Critical : AuditStatus.Warning,
                    "ČÚZK rok výstavby + Registr smluv + Data KHK Organizace kraje",
                    parameterA.Sources));
            }
            else if (c.EnvelopeRetrofit != null)
            {
                findings.Add(new PreAuditFinding(
                    "Zateplení doloženo v Registru smluv",
                    $"Organizace uzavřela smlouvu „{Short(c.EnvelopeRetrofit.Title)}“{SignedText(c.EnvelopeRetrofit)} — měrná ztráta snížena na {F(specificLoss, "0.0#")} W/m²K.",
                    "Obálka budovy",
                    AuditStatus.Nominal,
                    "Registr smluv ČR + ČÚZK",
                    ["registr-smluv", "cuzk-budovy"]));
            }

            // ---------- B: Klima lokality ----------
            var elevation = input.Elevation ?? 260;
            var degreeDays = ClimateModel.DegreeDays(elevation);
            var coldAnomaly = input.MeanOutdoor7Days is { } local && input.RegionMeanOutdoor7Days is { } region ? region - local : 0;
            var paramB = (int)Math.Clamp(Math.Round((degreeDays - ClimateModel.ReferenceDegreeDays) / 900.0 * 13 + Math.Max(0, coldAnomaly) * 1.5), 0, 15);
            var climateSources = input.ElevationFromApi ? new[] { "open-meteo-pocasi", "open-meteo-vyska", "hranice-orp" } : ["open-meteo-pocasi", "hranice-orp"];
            var parameterB = new PreAuditParameter(
                "B",
                "Klimatická náročnost lokality",
                paramB,
                15,
                $"{F(degreeDays, "N0")} K·den",
                $"Objekt ve výšce {F(elevation, "0")} m n. m.{(input.ElevationFromApi ? " (Open-Meteo Elevation API)" : " (správní centrum ORP)")}, ORP {input.OrpName} → {F(degreeDays, "N0")} denostupňů ({F(degreeDays / ClimateModel.ReferenceDegreeDays * 100 - 100, "+0;-0;0")} % proti Hradci Králové)"
                    + (input.MeanOutdoor7Days is { } t7 ? $"; posledních 7 dní průměrně {F(t7, "0.0")} °C (Open-Meteo)." : "."),
                climateSources);

            if (paramB >= 8)
            {
                findings.Add(new PreAuditFinding(
                    "Zvýšená klimatická expozice objektu v chladné oblasti",
                    $"Objekt stojí v ORP {input.OrpName} ve výšce ~{F(elevation, "0")} m n. m. — topná sezóna má {F(degreeDays, "N0")} denostupňů, o {F(degreeDays - ClimateModel.ReferenceDegreeDays, "N0")} víc než Polabí. Každý stupeň přetápění tu stojí víc; útlumový profil podle rozvrhu je nutnost.",
                    "Klima",
                    AuditStatus.Warning,
                    "Open-Meteo API + Data KHK Hranice ORP",
                    climateSources));
            }

            // ---------- C: Ekonomika ze smluv ----------
            var priceRatio = input.RegionalMedianHeatPriceCzkPerKwh > 0 ? (double)(input.HeatPriceCzkPerKwh / input.RegionalMedianHeatPriceCzkPerKwh) : 1;
            var heatLoss = input.FloorArea * specificLoss;
            var demandKwh = ClimateModel.AnnualHeatDemandKwh(heatLoss, degreeDays);
            var annualCost = Math.Round((decimal)demandKwh * input.HeatPriceCzkPerKwh / 1000) * 1000;

            var pricePoints = input.HeatPriceFromContract ? (int)Math.Clamp(Math.Round((priceRatio - 1) * 30), 0, 8) : 2;
            var costPoints = annualCost switch { >= 2_000_000 => 7, >= 1_000_000 => 5, >= 500_000 => 3, _ => 1 };
            var paramC = Math.Min(15, pricePoints + costPoints);
            var priceText = input.HeatPriceFromContract
                ? $"cena tepla ze smlouvy {F((double)input.HeatPriceCzkPerKwh * 1000, "N0")} Kč/MWh ({F((priceRatio - 1) * 100, "+0;-0;0")} % proti krajskému mediánu {F((double)input.RegionalMedianHeatPriceCzkPerKwh * 1000, "N0")} Kč/MWh)"
                : $"cena ve smlouvách nenalezena — krajský medián {F((double)input.RegionalMedianHeatPriceCzkPerKwh * 1000, "N0")} Kč/MWh";
            var parameterC = new PreAuditParameter(
                "C",
                "Ekonomika energií ze smluv",
                paramC,
                15,
                $"{F((double)annualCost, "N0")} Kč/rok",
                $"Odhad nákladů na teplo {F(demandKwh / 1000, "N0")} MWh/rok; {priceText}.",
                ["registr-smluv", "cuzk-budovy", "open-meteo-pocasi"]);

            if (input.HeatPriceFromContract && priceRatio >= 1.12)
            {
                findings.Add(new PreAuditFinding(
                    "Nadprůměrná jednotková cena tepelné energie oproti krajskému mediánu",
                    $"Organizace platí podle smlouvy v Registru smluv {F((double)input.HeatPriceCzkPerKwh * 1000, "N0")} Kč/MWh, o {F((priceRatio - 1) * 100, "0")} % víc než medián organizací kraje. Úspora každé kWh se tu vrací rychleji.",
                    "Ekonomika",
                    AuditStatus.Warning,
                    "Registr smluv ČR + Data KHK",
                    ["registr-smluv", "organizace-khk"]));
            }

            if (c.RegistryKnown && c.ControlService == null && annualCost >= 400_000)
            {
                findings.Add(new PreAuditFinding(
                    "Chybí servisní smlouva na měření a regulaci (MaR)",
                    $"V Registru smluv není za posledních 6 let smlouva na servis MaR, kotelny ani regulace vytápění, přitom teplo stojí odhadem {F((double)annualCost, "N0")} Kč ročně. Nastavení časových programů nikdo pravidelně nekontroluje.",
                    "Regulace",
                    AuditStatus.Warning,
                    "Registr smluv ČR + Data KHK",
                    ["registr-smluv", "organizace-khk"]));
            }

            // ---------- D: Hustota osob a větrání ----------
            var density = input.Occupants is > 0 && input.FloorArea > 0 ? input.Occupants.Value / input.FloorArea : (double?)null;
            var relative = density / ReferenceDensity(input.Category);
            var densityPoints = relative is { } r ? (int)Math.Clamp(Math.Round(r * 12), 0, 20) : 4;
            if (c.Ventilation != null)
                densityPoints = (int)Math.Round(densityPoints * 0.3);

            var paramD = densityPoints;
            var occupancySources = OccupancySources(input);
            var parameterD = new PreAuditParameter(
                "D",
                "Kapacitní a větrací zátěž CO₂",
                paramD,
                20,
                density is { } d ? $"{F(d, "0.000")} os./m²" : "kapacitu data neuvádějí",
                density is { } dd
                    ? $"{input.OccupantsLabel}: {F(input.Occupants!.Value, "N0")} osob na {F(input.FloorArea, "N0")} m² ({F(dd * 100, "0.0")} os. na 100 m², {F(relative!.Value * 100, "0")} % typické zátěže); {(c.Ventilation != null ? $"VZT/rekuperace doložena: „{Short(c.Ventilation.Title)}“" : "bez záznamu o rekuperaci/VZT v Registru smluv")}."
                    : "Otevřená data u tohoto typu zařízení kapacitu neuvádějí — počítá se s typickou zátěží.",
                occupancySources);

            if (relative is >= 1.15 && c.Ventilation == null)
            {
                findings.Add(new PreAuditFinding(
                    input.Category == FacilityCategory.School
                        ? "Vysoká koncentrace osob na m² a riziko překračování limitů CO₂ při výuce bez VZT"
                        : "Vysoká obsazenost a riziko vydýchaného vzduchu bez řízeného větrání",
                    $"{input.OccupantsLabel}: {F(input.Occupants!.Value, "N0")} osob na {F(input.FloorArea, "N0")} m² podlahy — {F(relative.Value * 100, "0")} % typické hustoty. Bez rekuperace se větrá okny, v mrazech to znamená tepelnou ztrátu nuceným větráním a nad 1 500 ppm CO₂ klesá soustředění.",
                    "Vnitřní prostředí",
                    relative >= 1.6 ? AuditStatus.Critical : AuditStatus.Warning,
                    $"Data KHK {OccupancySourceLabel(input)} + ČÚZK podlahová plocha + Registr smluv",
                    [.. occupancySources, "registr-smluv"]));
            }

            // ---------- E: Okolní kontext ----------
            var roadPoints = input.DistanceToRegionalRoadMeters switch { < 25 => 5, < 60 => 3, < 120 => 1, _ => 0 };
            var airPoints = input.MeanPm10 switch { >= 35 => 2, >= 25 => 1, _ => 0 };
            var heritagePoints = input.Heritage == HeritageStatus.None ? 0 : 2;
            var paramE = Math.Min(10, roadPoints + airPoints + heritagePoints + (input.IsThreatenedArea ? 1 : 0));
            var contextParts = new List<string>();
            if (input.DistanceToRegionalRoadMeters is { } road)
                contextParts.Add($"nejbližší silnice kraje {input.NearestRoadLabel ?? ""} {F(road, "0")} m".Replace("  ", " "));
            if (input.MeanPm10 is { } pm10)
                contextParts.Add($"PM10 průměr {F(pm10, "0.0")} µg/m³");
            if (input.DistanceToIzsKm is { } izs)
                contextParts.Add($"IZS {F(izs, "0.0")} km ({input.NearestIzsLabel})");
            if (input.Heritage != HeritageStatus.None)
                contextParts.Add("památková ochrana omezuje zásahy do obálky");
            if (input.IsThreatenedArea)
                contextParts.Add("hospodářsky a sociálně ohrožená obec");

            var contextSources = new[] { "silnice-kraj", "open-meteo-ovzdusi", "zzs-zakladny", "hzs-stanice", "hsoo", "pamatkove-zony" };
            var parameterE = new PreAuditParameter(
                "E",
                "Okolní kontext objektu",
                paramE,
                10,
                input.DistanceToRegionalRoadMeters is { } rd ? $"silnice {F(rd, "0")} m" : "—",
                contextParts.Count > 0 ? Capitalize(string.Join("; ", contextParts)) + "." : "Bez omezujících vlivů okolí.",
                contextSources);

            if (roadPoints >= 3)
            {
                findings.Add(new PreAuditFinding(
                    "Objekt u silnice II./III. třídy — větrání okny omezuje hluk a prach",
                    $"Budova stojí {F(input.DistanceToRegionalRoadMeters!.Value, "0")} m od silnice {input.NearestRoadLabel}. Okna do ulice se při provozu větrají méně, CO₂ roste rychleji{(input.MeanPm10 is { } p ? $"; prašnost PM10 v ORP {F(p, "0.0")} µg/m³" : "")}. Čidla CO₂ ukážou, kdy větrat a kdy raději ne.",
                    "Okolí",
                    AuditStatus.Nominal,
                    "Data KHK Stav povrchu silnic + Open-Meteo Air Quality",
                    ["silnice-kraj", "open-meteo-ovzdusi"]));
            }

            // ---------- Souhrn ----------
            var parameters = new[] { parameterA, parameterB, parameterC, parameterD, parameterE };
            var score = parameters.Sum(p => p.Score);
            var byScore = score >= CriticalScore ? AuditStatus.Critical : score >= WarningScore ? AuditStatus.Warning : AuditStatus.Nominal;
            // Kritické zjištění (např. dvojnásobná hustota žáků bez VZT) zvedne stav samo; ostatní zjištění nejsou přísnější než celkový stav
            var status = findings.Any(f => f.Severity == AuditStatus.Critical) ? AuditStatus.Critical : byScore;
            findings = findings.Select(f => f.Severity > status ? f with { Severity = status } : f).ToList();

            // Dominantní příčina = parametr s největším podílem svého maxima
            var dominant = parameters.Take(4).OrderByDescending(p => (double)p.Score / p.MaxScore).ThenBy(p => p.Key).First();
            var profile = DeriveProfile(input.Category, status, dominant.Key);

            var savingPercent = Math.Round(Math.Clamp(MinRegulationSaving + score * 0.13, MinRegulationSaving, MaxRegulationSaving), 1);
            var potential = Math.Round(annualCost * (decimal)savingPercent / 100 / 100) * 100;

            var floorsForKit = Math.Max(1, input.Floors);

            return new PreAuditResult(
                score,
                status,
                profile,
                dominant.Name,
                parameters,
                findings.OrderByDescending(f => f.Severity).ToList(),
                specificLoss,
                Math.Round(heatLoss),
                degreeDays,
                Math.Round(demandKwh),
                annualCost,
                savingPercent,
                potential,
                IqrfRoiCalculator.Kit(input.FloorArea, floorsForKit));
        }

        // Profil fyzikálního modelu odpovídá tomu, co pre-audit z dat vyvodil — nemocnice jedou 24/7 (víkendový útlum nemají),
        // nedostatečné větrání je problém učeben, chladná lokalita bez MaR se projeví ranním náběhem nebo chybějícím nočním útlumem
        public static SimulationProfile DeriveProfile(FacilityCategory category, AuditStatus status, string dominantKey)
        {
            if (status == AuditStatus.Nominal)
                return SimulationProfile.Nominal;

            if (dominantKey == "D" && category == FacilityCategory.School)
                return SimulationProfile.PoorVentilation;

            if (status == AuditStatus.Critical)
                return category == FacilityCategory.Health ? SimulationProfile.NoNightSetback : SimulationProfile.WeekendOverheating;

            return dominantKey switch
            {
                "B" when category == FacilityCategory.School => SimulationProfile.EarlyHeatingStart,
                "D" => SimulationProfile.PoorVentilation,
                _ => SimulationProfile.NoNightSetback,
            };
        }

        private static string[] OccupancySources(PreAuditInput input)
        {
            if (input.Category != FacilityCategory.School)
                return input.Category == FacilityCategory.Social ? ["socialni-sluzby", "cuzk-budovy"] : ["cuzk-budovy"];

            if (input.Kinds.Any(k => k is "Domov mládeže" or "Internát"))
                return ["ubytovani-dm", "skoly", "cuzk-budovy"];

            return input.Kinds.Contains("Vyšší odborná škola") ? ["zaci-ss-kraj", "studenti-vos-kraj", "skoly", "cuzk-budovy"] : ["zaci-ss-kraj", "zaci-ss-historie", "skoly", "cuzk-budovy"];
        }

        private static string OccupancySourceLabel(PreAuditInput input)
        {
            return input.Kinds.Any(k => k is "Domov mládeže" or "Internát") ? "Ubytovaní v domovech mládeže" : "Počty žáků a studentů / Rejstřík škol";
        }

        private static string SignedText(ContractRecord contract) => contract.DateSigned is { } date ? $" ({date.Year})" : string.Empty;

        private static string Short(string title) => title.Length <= 70 ? title : title[..67].TrimEnd() + "…";

        private static string Capitalize(string text) => text.Length == 0 ? text : char.ToUpper(text[0], Cs) + text[1..];

        private static string F(double value, string format) => value.ToString(format, Cs).Trim();
    }
}
