using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    // Stav budovy vychází z otevřených dat, ne z náhody: stejná data = stejné skóre, profil i zjištění,
    // a každé zjištění cituje sady, ze kterých vzniklo.
    public class OpenDataPreAuditTests
    {
        private static readonly DateOnly Today = new(2026, 10, 15);

        private static ContractEvidence NoContracts(bool known = true) => ContractEvidence.From([], known, Today);

        private static PreAuditInput School(
            int? year = 1968,
            double floorArea = 3000,
            int? students = 300,
            ContractEvidence? contracts = null,
            double elevation = 235,
            double? roadMeters = 400,
            decimal price = 2.2m,
            bool priceFromContract = false)
        {
            return new PreAuditInput(
                FacilityCategory.School,
                ["Střední škola"],
                year,
                BuildingEstimator.CategoryFromYear(year),
                floorArea,
                3,
                true,
                contracts ?? NoContracts(),
                HeritageStatus.None,
                "Hradec Králové",
                elevation,
                true,
                8.0,
                8.0,
                price,
                priceFromContract,
                2.2m,
                students,
                students == null ? null : "Žáci a studenti 2025/2026",
                roadMeters,
                "III/29840",
                18,
                3.5,
                "ZZS Hradec Králové",
                false);
        }

        [Fact]
        public void Same_open_data_give_the_same_result()
        {
            var first = OpenDataPreAudit.Evaluate(School());
            var second = OpenDataPreAudit.Evaluate(School());

            Assert.Equal(first.Score, second.Score);
            Assert.Equal(first.Status, second.Status);
            Assert.Equal(first.Profile, second.Profile);
            Assert.Equal(first.Findings.Select(f => f.Title), second.Findings.Select(f => f.Title));
        }

        [Fact]
        public void Retrofit_from_contract_registry_lowers_envelope_score_and_heat_loss()
        {
            var retrofit = ContractEvidence.From(
                [new ContractRecord("1", "Zateplení obvodového pláště a výměna oken budovy školy", ContractCategory.Reconstruction, new DateOnly(2019, 5, 1))],
                true,
                Today);

            var old = OpenDataPreAudit.Evaluate(School());
            var renovated = OpenDataPreAudit.Evaluate(School(contracts: retrofit));

            var envelopeOld = old.Parameters.Single(p => p.Key == "A").Score;
            var envelopeRenovated = renovated.Parameters.Single(p => p.Key == "A").Score;

            Assert.True(envelopeRenovated < envelopeOld);
            Assert.True(renovated.SpecificHeatLoss < old.SpecificHeatLoss);
            Assert.Contains(renovated.Findings, f => f.Title == "Zateplení doloženo v Registru smluv" && f.SourceKeys.Contains("registr-smluv"));
        }

        [Fact]
        public void Dense_school_without_ventilation_gets_poor_ventilation_profile_and_cited_finding()
        {
            // 900 žáků na 2 400 m² = 0,375 os./m² (3× typická hustota), žádná VZT ve smlouvách
            var result = OpenDataPreAudit.Evaluate(School(floorArea: 2400, students: 900));

            var finding = Assert.Single(result.Findings, f => f.Tag == "Vnitřní prostředí");
            Assert.Equal(AuditStatus.Critical, finding.Severity);
            Assert.Contains("ČÚZK", finding.Source);
            Assert.Contains("cuzk-budovy", finding.SourceKeys);
            Assert.Equal(SimulationProfile.PoorVentilation, result.Profile);
            Assert.Equal(AuditStatus.Critical, result.Status);
        }

        [Fact]
        public void Ventilation_investment_removes_the_co2_finding()
        {
            var air = ContractEvidence.From(
                [new ContractRecord("2", "Dodávka a montáž vzduchotechniky s rekuperací do učeben", ContractCategory.Reconstruction, new DateOnly(2022, 3, 1))],
                true,
                Today);

            var result = OpenDataPreAudit.Evaluate(School(floorArea: 2400, students: 900, contracts: air));

            Assert.DoesNotContain(result.Findings, f => f.Tag == "Vnitřní prostředí");
        }

        [Fact]
        public void Mountain_location_raises_climate_score()
        {
            var lowland = OpenDataPreAudit.Evaluate(School(elevation: 235));
            var mountains = OpenDataPreAudit.Evaluate(School(elevation: 650));

            Assert.True(mountains.DegreeDays > lowland.DegreeDays);
            Assert.True(mountains.Parameters.Single(p => p.Key == "B").Score > lowland.Parameters.Single(p => p.Key == "B").Score);
            Assert.True(mountains.AnnualHeatDemandKwh > lowland.AnnualHeatDemandKwh);
        }

        [Fact]
        public void Expensive_heat_from_contract_is_reported_against_regional_median()
        {
            var result = OpenDataPreAudit.Evaluate(School(price: 2.9m, priceFromContract: true));

            Assert.Contains(result.Findings, f => f.Title.StartsWith("Nadprůměrná jednotková cena") && f.Source.Contains("Registr smluv"));
        }

        [Fact]
        public void Modern_building_with_low_occupancy_is_nominal()
        {
            var result = OpenDataPreAudit.Evaluate(School(year: 2012, students: 150, roadMeters: null, contracts: NoContracts(known: false)));

            Assert.Equal(AuditStatus.Nominal, result.Status);
            Assert.Equal(SimulationProfile.Nominal, result.Profile);
        }

        [Fact]
        public void Every_parameter_cites_at_least_one_source_dataset()
        {
            var result = OpenDataPreAudit.Evaluate(School());

            Assert.Equal(["A", "B", "C", "D", "E"], result.Parameters.Select(p => p.Key));
            Assert.All(result.Parameters, p => Assert.NotEmpty(p.Sources));
            Assert.All(result.Findings, f => Assert.False(string.IsNullOrWhiteSpace(f.Source)));
        }

        [Theory]
        [InlineData(FacilityCategory.Health, AuditStatus.Critical, "A", SimulationProfile.NoNightSetback)]
        [InlineData(FacilityCategory.School, AuditStatus.Critical, "A", SimulationProfile.WeekendOverheating)]
        [InlineData(FacilityCategory.School, AuditStatus.Warning, "B", SimulationProfile.EarlyHeatingStart)]
        [InlineData(FacilityCategory.Social, AuditStatus.Warning, "A", SimulationProfile.NoNightSetback)]
        [InlineData(FacilityCategory.Culture, AuditStatus.Nominal, "D", SimulationProfile.Nominal)]
        public void Profile_follows_status_and_dominant_driver(FacilityCategory category, AuditStatus status, string driver, SimulationProfile expected)
        {
            Assert.Equal(expected, OpenDataPreAudit.DeriveProfile(category, status, driver));
        }

        [Fact]
        public void Contract_evidence_detects_retrofit_heating_ventilation_and_control_service()
        {
            ContractRecord[] contracts =
            [
                new("1", "Snížení energetické náročnosti budovy tělocvičny", ContractCategory.Reconstruction, new DateOnly(2021, 6, 1)),
                new("2", "Výměna plynového kotle", ContractCategory.Reconstruction, new DateOnly(2023, 9, 1)),
                new("3", "Servisní činnost a provádění oprav vzduchotechniky", ContractCategory.Reconstruction, new DateOnly(2021, 8, 3)),
                new("4", "Revize a servis plynového zařízení", ContractCategory.FacilityManagement, new DateOnly(2024, 1, 10)),
            ];

            var evidence = ContractEvidence.From(contracts, true, Today);

            Assert.Equal("1", evidence.EnvelopeRetrofit?.ContractId);
            Assert.Equal("2", evidence.HeatingModernization?.ContractId);
            Assert.Equal("3", evidence.Ventilation?.ContractId);
            Assert.NotNull(evidence.ControlService);
        }

        [Fact]
        public void Old_service_contracts_do_not_count_as_current_control()
        {
            var evidence = ContractEvidence.From(
                [new ContractRecord("9", "Servis kotelny a regulace vytápění", ContractCategory.FacilityManagement, new DateOnly(2012, 1, 1))],
                true,
                Today);

            Assert.Null(evidence.ControlService);
        }
    }

    public class IqrfRoiCalculatorTests
    {
        [Fact]
        public void Small_building_gets_gateway_and_minimum_sensor_set()
        {
            var kit = IqrfRoiCalculator.Kit(600, 2);

            Assert.Equal(1, kit.Gateways);
            Assert.Equal(6, kit.Sensors);
            Assert.Equal(8_500m + 6 * 2_400m, kit.HardwareCostCzk);
        }

        [Fact]
        public void Typical_school_kit_costs_between_25_and_55_thousand()
        {
            var kit = IqrfRoiCalculator.Kit(3000, 3);

            Assert.InRange(kit.HardwareCostCzk, 25_000m, 55_000m);
        }

        [Fact]
        public void Large_building_gets_second_gateway_and_sensor_cap()
        {
            var kit = IqrfRoiCalculator.Kit(9000, 4);

            Assert.Equal(2, kit.Gateways);
            Assert.Equal(24, kit.Sensors);
        }

        [Fact]
        public void Payback_uses_the_larger_of_measured_loss_and_regulation_potential()
        {
            var kit = new IqrfKit(1, 12, 37_300m);

            var fromLoss = IqrfRoiCalculator.Calculate(kit, 120_000m, 40_000m);
            var fromPotential = IqrfRoiCalculator.Calculate(kit, 0m, 40_000m);

            Assert.Equal(120_000m, fromLoss.AnnualSavingsCzk);
            Assert.Equal(3.7, fromLoss.PaybackMonths);
            Assert.Equal(11.2, fromPotential.PaybackMonths);
            Assert.Contains("pre-auditu", fromPotential.SavingsBasis);
        }

        [Fact]
        public void No_savings_means_no_payback()
        {
            var roi = IqrfRoiCalculator.Calculate(new IqrfKit(1, 6, 22_900m), 0, 0);

            Assert.Null(roi.PaybackMonths);
        }
    }
}
