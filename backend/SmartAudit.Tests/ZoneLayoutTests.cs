using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    // Budovy, které přidá správce, dostanou schéma místností podle druhu z formuláře — tělocvična nemá učebny
    public class ZoneLayoutTests
    {
        [Fact]
        public void Gym_has_no_classrooms()
        {
            var zones = ZoneLayout.Create(FacilityCategory.School, SimulationProfile.Nominal, ["Tělocvična"], "Tělocvična, Pulická 21", 1, 7);

            Assert.Contains(zones, z => z.Type == ZoneType.Gym);
            Assert.DoesNotContain(zones, z => z.Type == ZoneType.Classroom);
        }

        [Fact]
        public void Library_kind_gives_a_study_room_even_without_it_in_the_name()
        {
            var zones = ZoneLayout.Create(FacilityCategory.Culture, SimulationProfile.Nominal, ["Knihovna"], "Pobočka Slezské Předměstí", 2, 7);

            Assert.Contains(zones, z => z.Name.StartsWith("Studovna"));
        }

        [Fact]
        public void Regular_school_still_has_classrooms()
        {
            var zones = ZoneLayout.Create(FacilityCategory.School, SimulationProfile.Nominal, ["Střední škola"], "Gymnázium", 3, 7);

            Assert.Contains(zones, z => z.Type == ZoneType.Classroom);
        }
    }
}
