using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    // Adresy z našeptávače RÚIAN se rozkládají na ulici, číslo a PSČ pro kartu objektu
    public class RuianAddressTests
    {
        [Fact]
        public void Street_with_descriptive_and_orientation_number()
        {
            var address = RuianAddress.Parse("Pivovarské náměstí 1245/2, 50003 Hradec Králové");

            Assert.Equal(new ParsedAddress("Pivovarské náměstí", "1245/2", "50003", "Hradec Králové"), address);
        }

        [Fact]
        public void Part_of_town_in_the_middle_is_skipped()
        {
            var address = RuianAddress.Parse("Komenského 63a, Horní Předměstí, 54101 Trutnov");

            Assert.Equal(new ParsedAddress("Komenského", "63a", "54101", "Trutnov"), address);
        }

        [Fact]
        public void Village_without_streets_has_only_the_house_number()
        {
            var address = RuianAddress.Parse("č.p. 16, 50327 Radostov");

            Assert.Equal(new ParsedAddress(null, "č.p. 16", "50327", "Radostov"), address);
        }

        [Theory]
        [InlineData("50003", true)]
        [InlineData("51601", true)]
        [InlineData("55203", true)]
        [InlineData("67551", false)]
        [InlineData("53002", false)]
        [InlineData(null, false)]
        public void Postal_codes_of_the_region(string? psc, bool regional)
        {
            Assert.Equal(regional, RuianAddress.IsRegionalPostalCode(psc));
        }
    }
}
