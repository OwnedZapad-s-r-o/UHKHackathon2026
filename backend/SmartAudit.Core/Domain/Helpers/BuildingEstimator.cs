using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Odhady tam, kde otevřená data chybí — v UI se musí zobrazovat jako odhad
    public static class BuildingEstimator
    {
        public static EnergyCategory CategoryFromYear(int? constructionYear)
        {
            return constructionYear switch
            {
                null => EnergyCategory.Unknown,
                <= 1945 => EnergyCategory.Before1945,
                <= 1975 => EnergyCategory.From1946To1975,
                <= 1990 => EnergyCategory.From1976To1990,
                <= 2005 => EnergyCategory.From1991To2005,
                _ => EnergyCategory.After2006,
            };
        }

        // Měrná tepelná ztráta v W/(m² podlahy · K) podle období výstavby
        public static double SpecificHeatLoss(EnergyCategory category)
        {
            return category switch
            {
                EnergyCategory.Before1945 => 1.6,
                EnergyCategory.From1946To1975 => 1.9,
                EnergyCategory.From1976To1990 => 1.5,
                EnergyCategory.From1991To2005 => 1.0,
                EnergyCategory.After2006 => 0.6,
                _ => 1.5,
            };
        }

        // ČÚZK počet podlaží neposkytuje (200 z 200 budov "Unpopulated") — odhad z plochy a typu provozu
        public static int EstimateFloors(double? builtUpArea, FacilityCategory category)
        {
            if (builtUpArea == null)
                return category == FacilityCategory.Health ? 4 : 2;

            var floors = builtUpArea.Value switch
            {
                < 250 => 1,
                < 700 => 2,
                < 1800 => 3,
                _ => 4,
            };

            return category == FacilityCategory.Health ? Math.Max(floors, 3) : floors;
        }

        // Podlahová plocha pro výpočet ztrát — při neznámé ploše typická hodnota pro kategorii
        public static double FloorArea(double? builtUpArea, int floors, FacilityCategory category)
        {
            if (builtUpArea is > 0)
                return builtUpArea.Value * floors;

            return category switch
            {
                FacilityCategory.School => 3000,
                FacilityCategory.Social => 2500,
                FacilityCategory.Health => 8000,
                _ => 1500,
            };
        }

        public static string CategoryLabel(EnergyCategory category)
        {
            return category switch
            {
                EnergyCategory.Before1945 => "do 1945",
                EnergyCategory.From1946To1975 => "1946–1975",
                EnergyCategory.From1976To1990 => "1976–1990",
                EnergyCategory.From1991To2005 => "1991–2005",
                EnergyCategory.After2006 => "po 2006",
                _ => "neznámé stáří",
            };
        }
    }
}
