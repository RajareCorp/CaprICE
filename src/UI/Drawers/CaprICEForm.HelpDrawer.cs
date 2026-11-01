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

            Button btnClose = CyberControls.Button("FERMER", 140, 34, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Click += (s, e) => SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);

            pnlHelpDrawer.Controls.Add(BuildDrawerLayout(
                "GUIDE DES OPTIONS // CAPRICE", NeonTheme.Rose, txtHelpContent, btnClose));
        }
    }
}
