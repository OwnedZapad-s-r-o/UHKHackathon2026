namespace SmartAudit.Core.Data.Tables
{
    // Obsazenost objektu za jeden školní rok — žáci, lůžka, přihlášky
    public class FacilityMetric
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        public string SchoolYear { get; private set; } = string.Empty;


        // Žáci a studenti
        public int? Students { get; private set; }
        public int? Graduates { get; private set; }
        public int? Admitted { get; private set; }


        // Ubytování (domovy mládeže, internáty)
        public int? BedCapacity { get; private set; }
        public int? Residents { get; private set; }


        // Přijímací řízení
        public int? Applicants { get; private set; }
        public int? OfferedPlaces { get; private set; }


        // Cizí klíče
        public Guid FacilityId { get; private set; }


        // Navigační vlastnosti
        public Facility Facility { get; set; } = null!;

        private FacilityMetric() { }

        public FacilityMetric(Guid facilityId, string schoolYear)
        {
            if (string.IsNullOrWhiteSpace(schoolYear))
                throw new ArgumentException("Školní rok nesmí být prázdný.", nameof(schoolYear));

            FacilityId = facilityId;
            SchoolYear = schoolYear;
        }

        public void SetStudents(int students, int? graduates, int? admitted)
        {
            if (students < 0)
                throw new ArgumentException("Počet žáků nesmí být záporný.", nameof(students));

            Students = students;
            Graduates = graduates;
            Admitted = admitted;
        }

        public void SetAccommodation(int bedCapacity, int residents)
        {
            if (bedCapacity < 0 || residents < 0)
                throw new ArgumentException("Kapacita ani počet ubytovaných nesmí být záporné.", nameof(bedCapacity));

            BedCapacity = bedCapacity;
            Residents = residents;
        }

        public void SetAdmissions(int applicants, int offeredPlaces)
        {
            if (applicants < 0 || offeredPlaces < 0)
                throw new ArgumentException("Počty v přijímacím řízení nesmí být záporné.", nameof(applicants));

            Applicants = applicants;
            OfferedPlaces = offeredPlaces;
        }
    }
}
