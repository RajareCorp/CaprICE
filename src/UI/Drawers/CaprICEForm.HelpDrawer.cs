using System.Drawing;
using System.Windows.Forms;

namespace CaprICE.UI
{
    public partial class CaprICEForm
    {
        private void InitializeHelpDrawer()
        {
            pnlHelpDrawer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            TextBox txtHelpContent = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontSubtitle,
                BorderStyle = BorderStyle.None,
                Text =
@"SECURITY POLICIES:

1. MONITOR ONLY (Passive Audit)
------------------------------------
- Monitors and logs every exchange.

2. ENFORCE DLP (Data Loss Prevention)
------------------------------------
- Scans each prompt before sending it to the cloud and automatically masks sensitive data.

3. STRICT LOCAL (Cloud Lockdown)
------------------------------------
- Blocks all non-loopback connections; local loopback services remain allowed.

4. ASK CONFIRMATION (Manual Control)
------------------------------------
- Requests confirmation and shows a preview before sending."
            };

            Button btnClose = CyberControls.Button("CLOSE", 140, 34, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Click += (s, e) => SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);

            pnlHelpDrawer.Controls.Add(BuildDrawerLayout(
                "OPTIONS GUIDE // CAPRICE", NeonTheme.Rose, txtHelpContent, btnClose));
        }
    }
}
