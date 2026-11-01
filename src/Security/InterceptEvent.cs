using System;
using System.Collections.Generic;

namespace CaprICE.Security
{
    /// <summary>Un échange intercepté (requête, réponse ou connexion) affiché et journalisé.</summary>
    public class InterceptEvent
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Url { get; set; }
        public string TargetName { get; set; }
        public bool IsExternal { get; set; }
        public string Method { get; set; }
        public string RequestPayload { get; set; }
        public string ResponsePayload { get; set; }
        public string ContextMemory { get; set; }
        public int HistoryTurns { get; set; }
        public InterceptStatus Status { get; set; }
        public string Reason { get; set; }
        public long LatencyMs { get; set; }
        public List<string> DlpMatches { get; set; }

        public InterceptEvent()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            Timestamp = DateTime.Now;
            DlpMatches = new List<string>();
            Status = InterceptStatus.Allowed;
            LatencyMs = -1;
        }
    }
}
