using System;
using System.Drawing;
using System.Windows.Forms;
using CaprICE.Security;

namespace CaprICE.UI
{
    /// <summary>Rendu enrichi d'un événement intercepté dans le RichTextBox de l'inspecteur.</summary>
    internal sealed class InspectorRenderer
    {
        private readonly RichTextBox rtbInspector;

        public InspectorRenderer(RichTextBox target)
        {
            rtbInspector = target;
        }

        /// <summary>Affiche le détail complet d'un événement (en-tête, détections DLP, prompt, mémoire, réponse).</summary>
        public void Render(InterceptEvent ev)
        {
            rtbInspector.Clear();
            rtbInspector.SuspendLayout();

            // 1. En-tête de synthèse / Badges
            Color statusColor = ev.Status == InterceptStatus.Blocked ? NeonTheme.Rose :
                               (ev.Status == InterceptStatus.DlpMasked ? NeonTheme.StatusWarning : NeonTheme.Cyan);

            AppendStyledText($"EVENT #{ev.Id}", new Font("Consolas", 12F, FontStyle.Bold), NeonTheme.Rose);
            AppendStyledText($"  //  {ev.Timestamp:yyyy-MM-dd HH:mm:ss}  //  ", new Font("Consolas", 10F), NeonTheme.Faint);
            AppendStyledText($"[{ev.Status.ToString().ToUpper()}]\n", new Font("Consolas", 11F, FontStyle.Bold), statusColor);

            AppendStyledText($"TARGET  : ", new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.Faint);
            AppendStyledText($"{ev.TargetName} ", new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.White);
            AppendStyledText($"({(ev.IsExternal ? "EXTERNAL CLOUD" : "LOCAL LOOPBACK")})\n", new Font("Consolas", 9F), ev.IsExternal ? Color.Orange : NeonTheme.Cyan);

            AppendStyledText($"URL     : ", new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.Faint);
            AppendStyledText($"{ev.Url}\n", new Font("Consolas", 9F), NeonTheme.White);

            AppendStyledText($"REASON  : ", new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.Faint);
            AppendStyledText($"{ev.Reason} ", new Font("Consolas", 9.5F), NeonTheme.White);
            if (ev.LatencyMs >= 0)
            {
                AppendStyledText($" (Latency: {ev.LatencyMs} ms)\n", new Font("Consolas", 9F), NeonTheme.Faint);
            }
            else
            {
                AppendStyledText("\n", new Font("Consolas", 9F), NeonTheme.Faint);
            }

            if (ev.HistoryTurns > 0)
            {
                AppendStyledText($"CONTEXTE: ", new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.Faint);
                AppendStyledText($"{ev.HistoryTurns} active history turn(s)\n", new Font("Consolas", 9.5F), NeonTheme.Cyan);
            }

            // Masquages DLP / Alertes
            if (ev.DlpMatches != null && ev.DlpMatches.Count > 0)
            {
                AppendStyledText("\n[!] DLP DETECTIONS (SENSITIVE DATA):\n", new Font("Consolas", 10F, FontStyle.Bold), NeonTheme.StatusWarning);
                foreach (var match in ev.DlpMatches)
                {
                    AppendStyledText($"    • {match}\n", new Font("Consolas", 9.5F), NeonTheme.StatusWarning);
                }
            }

            // 2. Sections de données
            if (!string.IsNullOrEmpty(ev.RequestPayload))
            {
                RenderSectionHeader("1. CURRENT USER PROMPT SENT", NeonTheme.Cyan);
                AppendContentBlock(ev.RequestPayload);
            }

            if (!string.IsNullOrEmpty(ev.ContextMemory))
            {
                RenderSectionHeader("2. CONTEXT & MEMORY USED (me.md & HISTORY)", Color.MediumSpringGreen);
                AppendContentBlock(ev.ContextMemory);
            }

            if (!string.IsNullOrEmpty(ev.ResponsePayload))
            {
                RenderSectionHeader("3. MODEL RESPONSE", Color.Violet);
                AppendContentBlock(ev.ResponsePayload);
            }

            rtbInspector.SelectionStart = 0;
            rtbInspector.ScrollToCaret();
            rtbInspector.ResumeLayout();
        }

        private void RenderSectionHeader(string title, Color headerColor)
        {
            rtbInspector.AppendText("\n");
            rtbInspector.SelectionColor = headerColor;
            rtbInspector.SelectionFont = new Font("Consolas", 10F, FontStyle.Bold);
            rtbInspector.AppendText($"─── {title} ───────────────────────────────────────\n");
        }

        private void AppendContentBlock(string content)
        {
            rtbInspector.SelectionFont = new Font("Consolas", 9.5F, FontStyle.Regular);
            rtbInspector.SelectionColor = NeonTheme.White;
            
            // Formatage de sécurité pour faire ressortir les éléments masqués [REDACTED]
            if (content.Contains("[REDACTED"))
            {
                string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    if (line.Contains("[REDACTED"))
                    {
                        int index = 0;
                        while (index < line.Length)
                        {
                            int rIndex = line.IndexOf("[REDACTED", index, StringComparison.Ordinal);
                            if (rIndex == -1)
                            {
                                AppendStyledText(line.Substring(index) + "\n", rtbInspector.SelectionFont, NeonTheme.White);
                                break;
                            }

                            if (rIndex > index)
                            {
                                AppendStyledText(line.Substring(index, rIndex - index), rtbInspector.SelectionFont, NeonTheme.White);
                            }

                            int closeIndex = line.IndexOf("]", rIndex, StringComparison.Ordinal);
                            if (closeIndex != -1)
                            {
                                string redactedTag = line.Substring(rIndex, closeIndex - rIndex + 1);
                                AppendStyledText(redactedTag, new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.StatusWarning);
                                index = closeIndex + 1;
                            }
                            else
                            {
                                AppendStyledText(line.Substring(rIndex) + "\n", rtbInspector.SelectionFont, NeonTheme.StatusWarning);
                                break;
                            }
                        }
                    }
                    else
                    {
                        AppendStyledText(line + "\n", rtbInspector.SelectionFont, NeonTheme.White);
                    }
                }
            }
            else
            {
                rtbInspector.AppendText(content + "\n");
            }
        }

        private void AppendStyledText(string text, Font font, Color color)
        {
            rtbInspector.SelectionStart = rtbInspector.TextLength;
            rtbInspector.SelectionLength = 0;
            rtbInspector.SelectionFont = font;
            rtbInspector.SelectionColor = color;
            rtbInspector.AppendText(text);
        }
    }
}
