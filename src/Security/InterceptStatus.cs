namespace CaprICE.Security
{
    /// <summary>Décision prise par la politique pour un échange intercepté.</summary>
    public enum InterceptStatus
    {
        Allowed,
        Blocked,
        DlpMasked,
        PendingApproval
    }
}
