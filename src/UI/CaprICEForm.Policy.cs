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
                Text = "POLICY:",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Rose,
                Location = new Point(24, 14),
                AutoSize = true
            };

            rbMonitor = CreateRadio("1. Monitor Only", 130, SecurityPolicy.CurrentMode == PolicyMode.MonitorOnly, PolicyMode.MonitorOnly, "Passive mode: monitors and logs every request without blocking it.");
            rbDlp = CreateRadio("2. Enforce DLP (Masking)", 290, SecurityPolicy.CurrentMode == PolicyMode.EnforceDlp, PolicyMode.EnforceDlp, "DLP protection: detects and automatically masks API keys, names, phone numbers, IP addresses, and street addresses.");
            rbStrictLocal = CreateRadio("3. Strict Local (Block External)", 510, SecurityPolicy.CurrentMode == PolicyMode.BlockExternal, PolicyMode.BlockExternal, "Blocks every non-loopback URL, including voice connections.");
            rbAskConfirm = CreateRadio("4. Ask for Confirmation", 750, SecurityPolicy.CurrentMode == PolicyMode.AskConfirmation, PolicyMode.AskConfirmation, "Requests approval before each external Brain or Voice connection and shows the actual URL.");

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
                Text = "[ACTIVE MODE] 1. MONITOR ONLY:",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Cyan,
                Location = new Point(24, 11),
                AutoSize = true
            };

            lblExplainBody = new Label
            {
                Text = "Passive audit. All prompts, context, and memory sent to models are visible for inspection.",
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
                    lblExplainTitle.Text = "[ACTIVE MODE] 1. MONITOR ONLY:";
                    lblExplainTitle.ForeColor = NeonTheme.Cyan;
                    lblExplainBody.Text = "Passive audit. Outgoing prompts and context can be inspected; Brain and Voice are monitored.";
                    break;
                case PolicyMode.EnforceDlp:
                    lblStatusChip.Text = "DLP ENABLED";
                    lblStatusChip.BackColor = NeonTheme.StatusWarning;
                    lblExplainTitle.Text = "[ACTIVE MODE] 2. ENFORCE DLP:";
                    lblExplainTitle.ForeColor = NeonTheme.StatusWarning;
                    lblExplainBody.Text = "Filters system text, history, memory, and tools. Suspicious multimodal text is blocked; images are not OCR-scanned.";
                    break;
                case PolicyMode.BlockExternal:
                    lblStatusChip.Text = "LOCAL ONLY";
                    lblStatusChip.BackColor = NeonTheme.Rose;
                    lblExplainTitle.Text = "[ACTIVE MODE] 3. STRICT LOCAL:";
                    lblExplainTitle.ForeColor = NeonTheme.Rose;
                    lblExplainBody.Text = "All external Brain and Voice requests are blocked. Only loopback addresses are allowed.";
                    break;
                case PolicyMode.AskConfirmation:
                    lblStatusChip.Text = "CONFIRMATION ON";
                    lblStatusChip.BackColor = Color.FromArgb(200, 100, 255);
                    lblExplainTitle.Text = "[ACTIVE MODE] 4. ASK CONFIRMATION:";
                    lblExplainTitle.ForeColor = Color.FromArgb(200, 100, 255);
                    lblExplainBody.Text = "Review the actual host and outgoing context before allowing each external Brain or Voice connection.";
                    break;
            }
        }
    }
}
