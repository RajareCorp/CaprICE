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
        private Panel BuildBottomBar()
        {
            // Bottom Actions Panel
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = NeonTheme.Band
            };

            btnClear = CyberControls.Button("CLEAR LIST", 125, 32, NeonTheme.Edge, NeonTheme.White);
            btnClear.Location = new Point(24, 10);
            btnClear.Click += (s, e) =>
            {
                lvEvents.Items.Clear();
                eventsList.Clear();
                rtbInspector.Clear();
            };

            btnOpenAudit = CyberControls.Button("OPEN AUDIT LOG", 185, 32, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnOpenAudit.Location = new Point(160, 10);
            btnOpenAudit.Click += (s, e) =>
            {
                if (File.Exists(SecurityPolicy.AuditLogPath))
                    Process.Start("notepad.exe", SecurityPolicy.AuditLogPath);
                else
                    MessageBox.Show("There are no audit entries yet.", "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            Label lblProxy = new Label
            {
                Text = "UPSTREAM PROXY (OPTIONAL):",
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Faint,
                Location = new Point(570, 17),
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            txtProxyInput = new TextBox
            {
                Text = "http://127.0.0.1:8080",
                Font = NeonTheme.FontMono,
                ForeColor = NeonTheme.White,
                BackColor = NeonTheme.Field,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(200, 26),
                Location = new Point(765, 13),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            btnSetProxy = CyberControls.Button("SET PROXY", 130, 28, NeonTheme.Edge, NeonTheme.White);
            btnSetProxy.Location = new Point(980, 12);
            btnSetProxy.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSetProxy.Click += (s, e) =>
            {
                CaprICEHook.CustomProxyUrl = txtProxyInput.Text.Trim();
                MessageBox.Show("Upstream proxy configured: " + CaprICEHook.CustomProxyUrl, "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            pnlBottom.Controls.Add(btnClear);
            pnlBottom.Controls.Add(btnOpenAudit);
            pnlBottom.Controls.Add(lblProxy);
            pnlBottom.Controls.Add(txtProxyInput);
            pnlBottom.Controls.Add(btnSetProxy);
            return pnlBottom;
        }
    }
}
