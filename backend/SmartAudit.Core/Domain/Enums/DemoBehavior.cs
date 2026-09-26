namespace SmartAudit.Core.Domain.Enums
{
    // Co demo zařízení naměří mimo provoz budovy — podle toho se po přiřazení opraví odhad ztráty
    public enum DemoBehavior
    {
        // Regulace funguje: v noci a o víkendu útlum na ~18 °C → ztráta klesne
        Setback,

        // Budova přetápí i mimo provoz (~22 °C) → ztráta zůstane nebo vzroste
        Overheating,
    }
}
