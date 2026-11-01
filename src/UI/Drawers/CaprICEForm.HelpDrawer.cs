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
        private void InitializeHelpDrawer()
        {
            pnlHelpDrawer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 460,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            Label lblHelpTitle = new Label
            {
                Text = "GUIDE DES OPTIONS // CAPRICE",
                Font = NeonTheme.FontTitle,
                ForeColor = NeonTheme.Rose,
                Dock = DockStyle.Top,
                Height = 36
            };

            Button btnCloseHelp = CyberControls.Button("FERMER LE GUIDE [X]", 170, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnCloseHelp.Dock = DockStyle.Bottom;
            btnCloseHelp.Click += (s, e) => ToggleHelpDrawer();

            TextBox txtHelpContent = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontSubtitle,
                BorderStyle = BorderStyle.None,
                Text =
@"POLITIQUES DE SÉCURITÉ :

1. MONITOR ONLY (Audit Passif)
------------------------------------
- Écoute et enregistre chaque échange en toute transparence.

2. ENFORCE DLP (Prévention Fuite)
------------------------------------
- Scanne chaque prompt avant envoi au cloud et masque automatiquement les données sensibles.

3. STRICT LOCAL (Verrou Cloud)
------------------------------------
- Coupe tout accès aux IA externes. Seul Ollama local passe.

4. ASK CONFIRMATION (Contrôle Manuel)
------------------------------------
- Demande confirmation avec prévisualisation avant envoi."
            };

            pnlHelpDrawer.Controls.Add(txtHelpContent);
            pnlHelpDrawer.Controls.Add(lblHelpTitle);
            pnlHelpDrawer.Controls.Add(btnCloseHelp);
        }

        private void ToggleHelpDrawer()
        {
            helpDrawerVisible = !helpDrawerVisible;
            if (helpDrawerVisible && settingsDrawerVisible) ToggleSettingsDrawer();
            pnlHelpDrawer.Visible = helpDrawerVisible;
            if (helpDrawerVisible) pnlHelpDrawer.BringToFront();
            btnHelpToggle.Text = helpDrawerVisible ? "[X] FERMER" : "? GUIDE";
        }
    }
}
