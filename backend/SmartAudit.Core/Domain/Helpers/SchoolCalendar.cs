using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Dny, kdy je škola prázdná — státní svátky, víkendy, prázdniny (F39)
    public static class SchoolCalendar
    {
        private static readonly TimeZoneInfo PragueZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Prague");

        //TODO: doplnit podzimní, pololetní a jarní prázdniny 2026/27 podle harmonogramu MŠMT (jarní se liší podle okresu)
        private static readonly (DateOnly From, DateOnly To)[] Holidays =
        [
            (new DateOnly(2026, 7, 1), new DateOnly(2026, 8, 31)),
            (new DateOnly(2026, 12, 23), new DateOnly(2027, 1, 3)),
            (new DateOnly(2027, 7, 1), new DateOnly(2027, 8, 31)),
        ];

        private static readonly (int Month, int Day)[] FixedHolidays = [(1, 1), (5, 1), (5, 8), (7, 5), (7, 6), (9, 28), (10, 28), (11, 17), (12, 24), (12, 25), (12, 26)];

        // Počet pravidel kalendáře (svátky + Velikonoce + prázdninová období) — pro panel zdrojů dat
        public static int DefinitionCount => FixedHolidays.Length + 2 + Holidays.Length;

        public static DateTime ToLocal(DateTimeOffset time)
        {
            return TimeZoneInfo.ConvertTime(time, PragueZone).DateTime;
        }

        public static DateTimeOffset LocalMidnightUtc(DateOnly day)
        {
            var local = day.ToDateTime(TimeOnly.MinValue);
            var offset = PragueZone.GetUtcOffset(local);

            return new DateTimeOffset(local, offset).ToUniversalTime();
        }

        public static bool IsWeekend(DateOnly day)
        {
            return day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        }

        public static bool IsPublicHoliday(DateOnly day)
        {
            if (FixedHolidays.Contains((day.Month, day.Day)))
                return true;

            var easterSunday = EasterSunday(day.Year);

            return day == easterSunday.AddDays(-2) || day == easterSunday.AddDays(1);
        }

        public static bool IsSchoolHoliday(DateOnly day)
        {
            return Holidays.Any(h => day >= h.From && day <= h.To);
        }

        public static bool IsSchoolDay(DateOnly day)
        {
            return !IsWeekend(day) && !IsPublicHoliday(day) && !IsSchoolHoliday(day);
        }

        public static bool IsWorkday(DateOnly day)
        {
            return !IsWeekend(day) && !IsPublicHoliday(day);
        }

        // Den, kdy má být budova v útlumu — sdílí ho simulace i vyhodnocení auditu
        public static bool IsFreeDay(FacilityCategory category, DateOnly day)
        {
            return category switch
            {
                FacilityCategory.School => !IsSchoolDay(day),
                FacilityCategory.Culture => day.DayOfWeek == DayOfWeek.Monday || IsPublicHoliday(day),
                FacilityCategory.Health => false,
                _ => !IsWorkday(day),
            };
        }

        // Gaussův algoritmus pro gregoriánský kalendář
        private static DateOnly EasterSunday(int year)
        {
            var a = year % 19;
            var b = year / 100;
            var c = year % 100;
            var d = b / 4;
            var e = b % 4;
            var f = (b + 8) / 25;
            var g = (b - f + 1) / 3;
            var h = (19 * a + b - d - g + 15) % 30;
            var i = c / 4;
            var k = c % 4;
            var l = (32 + 2 * e + 2 * i - h - k) % 7;
            var m = (a + 11 * h + 22 * l) / 451;
            var month = (h + l - 7 * m + 114) / 31;
            var day = ((h + l - 7 * m + 114) % 31) + 1;

            return new DateOnly(year, month, day);
        }
    }
}
