namespace CaprICE.Security
{
    /// <summary>Les 4 politiques de sécurité sélectionnables dans l'interface.</summary>
    public enum PolicyMode
    {
        MonitorOnly,
        EnforceDlp,
        BlockExternal,
        AskConfirmation
    }
}
