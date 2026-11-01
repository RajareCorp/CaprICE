using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CaprICE.Hooks;
using CaprICE.Security;
using CaprICE.Services;

namespace CaprICE.UI
{
    public partial class CaprICEForm
    {
        private Panel BuildPolicyBar()
        {
            // Policy Selection Bar
            Panel pnlModes = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = NeonTheme.Band
            };

            Label lblModeTag = new Label
            {
                Text = "POLITIQUE :",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Rose,
                Location = new Point(24, 14),
                AutoSize = true
            };

            rbMonitor = CreateRadio("1. Monitor Only", 130, SecurityPolicy.CurrentMode == PolicyMode.MonitorOnly, PolicyMode.MonitorOnly, "Mode passif : surveille et journalise chaque requête sans jamais bloquer.");
            rbDlp = CreateRadio("2. Enforce DLP (Masquage)", 290, SecurityPolicy.CurrentMode == PolicyMode.EnforceDlp, PolicyMode.EnforceDlp, "Pare-feu DLP : détecte et masque automatiquement clés API, noms, téléphones, IP, adresses.");
            rbStrictLocal = CreateRadio("3. Strict Local (Bloquer externe)", 510, SecurityPolicy.CurrentMode == PolicyMode.BlockExternal, PolicyMode.BlockExternal, "Bloque toute URL non-loopback, y compris les connexions vocales.");
            rbAskConfirm = CreateRadio("4. Ask Confirmation", 750, SecurityPolicy.CurrentMode == PolicyMode.AskConfirmation, PolicyMode.AskConfirmation, "Demande l'approbation avant chaque connexion externe Brain ou Voice, avec l'URL réelle.");

            pnlModes.Controls.Add(lblModeTag);
            pnlModes.Controls.Add(rbMonitor);
            pnlModes.Controls.Add(rbDlp);
            pnlModes.Controls.Add(rbStrictLocal);
            pnlModes.Controls.Add(rbAskConfirm);
            return pnlModes;
        }

        private void BuildExplanationBanner()
        {
            // Explanation Banner
            pnlExplanation = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = NeonTheme.Deep,
                Padding = new Padding(24, 0, 24, 0)
            };

            lblExplainTitle = new Label
            {
                Text = "[MODE ACTIF] 1. MONITOR ONLY :",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Cyan,
                Location = new Point(24, 11),
                AutoSize = true
            };

            lblExplainBody = new Label
            {
                Text = "Audit passif. Tous les prompts, contextes et mémoires envoyés aux modèles sont affichés en toute transparence.",
                Font = NeonTheme.FontBody,
                ForeColor = NeonTheme.White,
                Location = new Point(265, 11),
                AutoSize = true
            };

            pnlExplanation.Controls.Add(lblExplainTitle);
            pnlExplanation.Controls.Add(lblExplainBody);
            UpdateExplanationBanner(SecurityPolicy.CurrentMode);
        }

        private RadioButton CreateRadio(string text, int x, bool check, PolicyMode mode, string tooltip)
        {
            RadioButton rb = new RadioButton
            {
                Text = text,
                Location = new Point(x, 11),
                AutoSize = true,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontBody,
                Checked = check
            };
            toolTips.SetToolTip(rb, tooltip);

            rb.CheckedChanged += (s, e) =>
            {
                if (rb.Checked)
                {
                    SecurityPolicy.CurrentMode = mode;
                    SecurityPolicy.SaveSettings();
                    UpdateExplanationBanner(mode);
                }
            };

            return rb;
        }

        private void UpdateExplanationBanner(PolicyMode mode)
        {
            switch (mode)
            {
                case PolicyMode.MonitorOnly:
                    lblStatusChip.Text = "AUDIT / MONITOR";
                    lblStatusChip.BackColor = NeonTheme.Cyan;
                    lblExplainTitle.Text = "[MODE ACTIF] 1. MONITOR ONLY :";
                    lblExplainTitle.ForeColor = NeonTheme.Cyan;
                    lblExplainBody.Text = "Audit passif. Les prompts et contextes sortants sont inspectables ; Brain et Voice sont surveillés.";
                    break;
                case PolicyMode.EnforceDlp:
                    lblStatusChip.Text = "DLP ACTIVÉ";
                    lblStatusChip.BackColor = NeonTheme.StatusWarning;
                    lblExplainTitle.Text = "[MODE ACTIF] 2. ENFORCE DLP :";
                    lblExplainTitle.ForeColor = NeonTheme.StatusWarning;
                    lblExplainBody.Text = "Filtrage actif du texte : système, historique, mémoire et outils. Texte multimodal suspect bloqué ; pas d'OCR.";
                    break;
                case PolicyMode.BlockExternal:
                    lblStatusChip.Text = "LOCAL ONLY";
                    lblStatusChip.BackColor = NeonTheme.Rose;
                    lblExplainTitle.Text = "[MODE ACTIF] 3. STRICT LOCAL :";
                    lblExplainTitle.ForeColor = NeonTheme.Rose;
                    lblExplainBody.Text = "Tout appel externe Brain ou Voice est bloqué. Seules les adresses loopback sont autorisées.";
                    break;
                case PolicyMode.AskConfirmation:
                    lblStatusChip.Text = "CONFIRMATION ON";
                    lblStatusChip.BackColor = Color.FromArgb(200, 100, 255);
                    lblExplainTitle.Text = "[MODE ACTIF] 4. ASK CONFIRMATION :";
                    lblExplainTitle.ForeColor = Color.FromArgb(200, 100, 255);
                    lblExplainBody.Text = "Avant chaque connexion externe Brain ou Voice, vérifiez l'hôte réel et le contexte avant d'autoriser.";
                    break;
            }
        }
    }
}
