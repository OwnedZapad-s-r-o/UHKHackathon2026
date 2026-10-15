namespace SmartAudit.Core.Data.Tables
{
    // Kontakt na obecní úřad (sada Adresář obcí) — komu poslat dopis o objektu v obci
    public class MunicipalityContact
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        public string MunicipalityCode { get; private set; } = string.Empty;
        public string MunicipalityName { get; private set; } = string.Empty;


        // Kontakty
        public string? DataBox { get; private set; }
        public string? Email { get; private set; }
        public string? Phone { get; private set; }
        public string? Web { get; private set; }
        public string? Address { get; private set; }

        private MunicipalityContact() { }

        public MunicipalityContact(string municipalityCode, string municipalityName, string? dataBox, string? email, string? phone, string? web, string? address)
        {
            if (string.IsNullOrWhiteSpace(municipalityCode))
                throw new ArgumentException("Kód obce nesmí být prázdný.", nameof(municipalityCode));

            MunicipalityCode = municipalityCode;
            MunicipalityName = municipalityName;
            DataBox = dataBox;
            Email = email;
            Phone = phone;
            Web = web;
            Address = address;
        }
    }
}
