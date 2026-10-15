using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    // Registr smluv nemá vyhledávací API — čte se HTML podrobného vyhledávání a detailu smlouvy (výřezy ze smlouvy.gov.cz, září 2026)
    public class ContractRegistryTests
    {
        private const string SearchSnippet = """
            <table class="searchResultList">
                <thead class="theader"><tr><th>Publikující smluvní strana</th><th>Předmět smlouvy</th></tr></thead>
                <tr>
                    <td class="1">Česká lesnická akademie Trutnov - střední škola a vyšší odborná škola</td>
                    <td class="2">Havarijní oprava regulace teplovodní kotelny v objektu Horská 134</td>
                    <td class="3">ano</td>
                    <td class="4">14.01.2020</td>
                    <td class="number nobr 5">260 238 CZK bez DPH</td>
                    <td class="6">RegulTech servis, s.r.o.</td>
                    <td class="btn no-sort"><a href="/smlouva/10755669?backlink=3h9ov" class="btn">Detail</a></td>
                </tr>
            </table>
            """;

        private const string DetailPage = """
            <h2>Česká lesnická akademie Trutnov - Snížení energetické náročnosti</h2>
            <div class="row detail">
            <div class="w-col w-col-4 first-col">
            <h2>Publikující smluvní strana</h2>
            <div class="item"><span class="name lg">Název subjektu:</span><span class="value lg">Česká lesnická akademie Trutnov</span></div>
            <div class="item"><span class="name">IČO:</span><span class="value">60153296 <a href="/vyhledavani?subject_idnum=60153296" title="Hledat IČO"><img src="x.png" alt="Hledat IČO"></a></span></div>
            <h3>Informace o zápisu</h3>
            <div class="item"><span class="name">ID smlouvy:</span><span class="value">32228453</span></div>
            <div class="item"><span class="name">ID verze:</span><span class="value">38571260</span></div>
            <div class="item"><span class="name">Zveřejnění:</span><span class="value">29.06.2026 14:47:04</span></div>
            <h3>Smlouva</h3>
            <div class="item"><span class="name">Předmět smlouvy:</span><span class="value">Snížení energetické náročnosti ČLA Trutnov - hlavní budova - stavební práce</span></div>
            <div class="item"><span class="name">Datum uzavření:</span><span class="value">06.08.2025</span></div>
            <div class="item"><span class="name">Číslo smlouvy / č.j.:</span><span class="value">Smlouva o dílo 06.08. 2025</span></div>
            <div class="item"><span class="name">Ev. číslo zak. z VVZ:</span></div>
            <div class="item"><span class="name">Hodnota bez DPH:</span><span class="value">32 387 340,10 CZK</span></div>
            <div class="item"><span class="name">Hodnota vč. DPH:</span><span class="value">39 188 681,52 CZK</span></div>
            </div>
            <div class="w-col w-col-4">
            <h2>Smluvní strany</h2>
            <div class="contractor">
            <div class="item"><span class="name lg">Název:</span><span class="value lg">CE -  ING s.r.o.</span></div>
            <div class="item"><span class="name">IČO:</span><span class="value">04475631 <a href="/vyhledavani?party_idnum=04475631" title="Hledat IČO"><img src="x.png" alt="Hledat IČO"></a></span></div>
            </div>
            </div>
            <div class="w-col w-col-4 last-col">
            <h2>Verze záznamu</h2>
            <h3>Soubory</h3>
            <div class="item">
            <span class="name source">Přílohy smlouvy:</span>
            <span class="value source pdf"><a href="/smlouva/soubor/41782245/CLA_Trutnov_Smlouva_o_dilo%20%282%29.pdf">CLA_Trutnov_Smlouva_o_dilo (2).pdf</a><span class="size">(1.41 MB)</span></span>
            <span class="value source pdf"><a href="/smlouva/soubor/46573488/optimized_document.pdf">optimized_document.pdf</a><span class="size">(10.48 MB)</span></span>
            </div>
            </div>
            </div>
            """;

        [Fact]
        public void Search_results_give_version_title_and_publication_date()
        {
            var rows = ContractRegistryParser.ParseSearchResults(SearchSnippet);

            var row = Assert.Single(rows);
            Assert.Equal("10755669", row.VersionId);
            Assert.Equal("Havarijní oprava regulace teplovodní kotelny v objektu Horská 134", row.Title);
            Assert.Equal(new DateOnly(2020, 1, 14), row.PublishedOn);
        }

        [Fact]
        public void Detail_gives_contract_partner_amounts_and_attachments()
        {
            var contract = ContractRegistryParser.ParseDetail(DetailPage);

            Assert.NotNull(contract);
            Assert.Equal("32228453", contract.ContractId);
            Assert.Equal("38571260", contract.VersionId);
            Assert.Equal("60153296", contract.PublisherIco);
            Assert.Equal("CE - ING s.r.o.", contract.PartnerName);
            Assert.Equal("04475631", contract.PartnerIco);
            Assert.Equal(32_387_340.10m, contract.AmountWithoutVat);
            Assert.Equal(39_188_681.52m, contract.AmountWithVat);
            Assert.Equal(new DateOnly(2025, 8, 6), contract.DateSigned);
            Assert.Equal(new DateOnly(2026, 6, 29), contract.PublishedOn);
            Assert.Equal("Smlouva o dílo 06.08. 2025", contract.ContractNumber);

            Assert.Equal(2, contract.Attachments.Count);
            Assert.Equal("41782245", contract.Attachments[0].Id);
            Assert.Equal("CLA_Trutnov_Smlouva_o_dilo (2).pdf", contract.Attachments[0].Name);
            Assert.Equal("https://smlouvy.gov.cz/smlouva/soubor/41782245/CLA_Trutnov_Smlouva_o_dilo%20%282%29.pdf", contract.Attachments[0].Url);
        }

        [Fact]
        public void Page_without_contract_sections_is_not_a_contract()
        {
            Assert.Null(ContractRegistryParser.ParseDetail("<html><body>Stránka nenalezena</body></html>"));
        }

        // Registr občas uvádí publikující organizaci i mezi smluvními stranami — dodavatel je „Příjemce“
        [Fact]
        public void Partner_is_the_recipient_not_the_publisher_listed_among_parties()
        {
            var page = DetailPage.Replace(
                """<div class="contractor">""",
                """
                <div class="contractor">
                <div class="item"><span class="name lg">Název:</span><span class="value lg">Česká lesnická akademie Trutnov</span></div>
                <div class="item"><span class="name">IČO:</span><span class="value">60153296 </span></div>
                </div>
                <div class="contractor">
                """);

            var contract = ContractRegistryParser.ParseDetail(page);

            Assert.NotNull(contract);
            Assert.Equal("CE - ING s.r.o.", contract.PartnerName);
            Assert.Equal("04475631", contract.PartnerIco);
        }

        [Theory]
        [InlineData("Dodávka tepelné energie pro objekt školy 2026", ContractCategory.EnergySupply)]
        [InlineData("Smlouva o dodávce tepla - Teplárny Hradec Králové", ContractCategory.EnergySupply)]
        [InlineData("Sdružené služby dodávky zemního plynu", ContractCategory.EnergySupply)]
        [InlineData("Havarijní oprava řídícího systému plynové kotelny", ContractCategory.FacilityManagement)]
        [InlineData("Oprava plynového kotle, roční kontrola plynových kotlů organizace", ContractCategory.FacilityManagement)]
        [InlineData("Servis měření a regulace ÚT", ContractCategory.FacilityManagement)]
        [InlineData("Snížení energetické náročnosti ČLA Trutnov - hlavní budova", ContractCategory.Reconstruction)]
        [InlineData("Smlouva o dílo na výměnu plynového kotle", ContractCategory.Reconstruction)]
        [InlineData("Zateplení obvodového pláště a výměna oken", ContractCategory.Reconstruction)]
        public void Energy_and_building_contracts_are_classified(string title, ContractCategory expected)
        {
            Assert.Equal(expected, ContractClassifier.Classify(title));
        }

        [Theory]
        [InlineData("Dodávka klestu pro energetické využití")]
        [InlineData("Doprava energetické štěpky")]
        [InlineData("Servis a údržba IVECO")]
        [InlineData("Elektronická registrační průměrka")]
        [InlineData("Svoz a nájem odpadu (komunální, nebezpečný, separovaný, elektroodpad)")]
        [InlineData("Zajištění stravování, výroba teplých a studených jídel")]
        public void Unrelated_contracts_are_skipped(string title)
        {
            Assert.Null(ContractClassifier.Classify(title));
        }

        [Fact]
        public void Heat_price_per_gigajoule_is_converted_to_megawatt_hour()
        {
            const string text = "Cena tepelné energie činí 612,50 Kč/GJ bez DPH. Záloha 15 000 Kč. Pro rok 2027 platí cena 612,50 Kč/GJ.";

            var price = EnergyPriceExtractor.Extract(text, EnergyCommodity.Heat);

            Assert.NotNull(price);
            Assert.Equal(2205, price.CzkPerMwh);
            Assert.Contains("Kč/GJ", price.Snippet);
        }

        // Přílohy smluv na teplo obsahují dodatky od roku 2002 — tehdejší cena by ztrátu podhodnotila
        [Fact]
        public void Old_heat_price_list_is_not_taken_as_todays_price()
        {
            Assert.Null(EnergyPriceExtractor.Extract("Cena tepla od 1. 1. 2003 činí 210 Kč/GJ.", EnergyCommodity.Heat));
        }

        [Fact]
        public void Entity_entered_by_publisher_is_decoded()
        {
            var rows = ContractRegistryParser.ParseSearchResults(SearchSnippet.Replace("Havarijní oprava", "Oprava &amp;quot;kotelny&amp;quot;"));

            Assert.StartsWith("Oprava \"kotelny\"", Assert.Single(rows).Title);
        }

        [Fact]
        public void Implausible_numbers_next_to_a_unit_are_ignored()
        {
            const string text = "Stálý plat 12 Kč/MWh, jistina 150 000 Kč/MWh.";

            Assert.Null(EnergyPriceExtractor.Extract(text, EnergyCommodity.Gas));
        }

        [Fact]
        public void Heat_contract_price_wins_over_gas_and_electricity_is_never_used()
        {
            var heat = new ContractPrice(Guid.NewGuid(), EnergyCommodity.Heat, 2450, new DateOnly(2024, 1, 1));
            var gas = new ContractPrice(Guid.NewGuid(), EnergyCommodity.Gas, 1800, new DateOnly(2025, 1, 1));
            var electricity = new ContractPrice(Guid.NewGuid(), EnergyCommodity.Electricity, 4000, new DateOnly(2026, 1, 1));

            var withHeat = EnergyPriceExtractor.SelectHeatPrice([gas, electricity, heat], 2.2m);
            var onlyElectricity = EnergyPriceExtractor.SelectHeatPrice([electricity], 2.2m);
            var onlyGas = EnergyPriceExtractor.SelectHeatPrice([gas], 2.2m);

            Assert.Equal(2.45m, withHeat.CzkPerKwh);
            Assert.Equal(heat.ContractId, withHeat.ContractId);
            Assert.False(onlyElectricity.FromContract);
            Assert.Equal(2.2m, onlyElectricity.CzkPerKwh);
            Assert.Equal(2m, onlyGas.CzkPerKwh);
        }
    }
}
