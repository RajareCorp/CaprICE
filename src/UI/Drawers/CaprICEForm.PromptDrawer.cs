using System.Drawing;
using System.Windows.Forms;
using CaprICE.Hooks;
using CaprICE.Services;

namespace CaprICE.UI
{
    public partial class CaprICEForm
    {
        private TextBox txtPersonality;
        private TextBox txtSafety;
        private TextBox txtMachinery;

        private void InitializePromptDrawer()
        {
            pnlPromptDrawer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            Panel pnlContentScroll = new Panel
            {
                AutoScroll = true,
                BackColor = NeonTheme.Well
            };

            TableLayoutPanel content = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 6,
                Height = 378,
                BackColor = NeonTheme.Well,
                Location = Point.Empty
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));

            Label lblPersonality = CreateSectionLabel("1. PERSONALITY (Personnage, Ton, Lore)");
            txtPersonality = CreateCodeTextBox();
            Label lblSafety = CreateSectionLabel("2. SAFETY (Sécurité, Limites, Comportement)");
            txtSafety = CreateCodeTextBox();
            Label lblMachinery = CreateSectionLabel("3. MACHINERY (Contraintes de Format & Voice)");
            txtMachinery = CreateCodeTextBox();

            content.Controls.Add(lblPersonality, 0, 0);
            content.Controls.Add(txtPersonality, 0, 1);
            content.Controls.Add(lblSafety, 0, 2);
            content.Controls.Add(txtSafety, 0, 3);
            content.Controls.Add(lblMachinery, 0, 4);
            content.Controls.Add(txtMachinery, 0, 5);
            pnlContentScroll.Controls.Add(content);
            pnlContentScroll.SizeChanged += (s, e) => content.Width = pnlContentScroll.ClientSize.Width;

            Button btnFetchOriginal = CyberControls.Button("RECHARGER PROMPTS ORIGINAUX", 240, 34, NeonTheme.Edge, NeonTheme.Cyan);
            btnFetchOriginal.Click += (s, e) => LoadOriginalsIntoFields();

            Button btnSave = CyberControls.Button("APPLIQUER LES HOOKS PROMPT", 250, 36, NeonTheme.DeepEdge, NeonTheme.Rose);
            btnSave.Click += (s, e) =>
            {
                CaprICEHook.CustomPersonality = string.IsNullOrWhiteSpace(txtPersonality.Text) ? null : txtPersonality.Text.Trim();
                CaprICEHook.CustomSafety = string.IsNullOrWhiteSpace(txtSafety.Text) ? null : txtSafety.Text.Trim();
                CaprICEHook.CustomMachinery = string.IsNullOrWhiteSpace(txtMachinery.Text) ? null : txtMachinery.Text.Trim();

                bool historyCleared = CaprICEHook.ClearBrainHistory();
                MessageBox.Show(historyCleared
                        ? "Nouveau System Prompt modulaire appliqué."
                        : "Le prompt a été appliqué, mais l'historique actif de Capri n'a pas pu être vidé.",
                    "CaprICE", MessageBoxButtons.OK,
                    historyCleared ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);
            };

            Button btnClose = CyberControls.Button("FERMER", 120, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Click += (s, e) => SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);

            pnlPromptDrawer.Controls.Add(BuildDrawerLayout(
                "HOOK SYSTEM PROMPT (MODULAIRE)", NeonTheme.Rose, pnlContentScroll,
                btnFetchOriginal, btnSave, btnClose));
        }

        private Label CreateSectionLabel(string title)
        {
            return new Label
            {
                Text = title,
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Cyan,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };
        }

        private TextBox CreateCodeTextBox()
        {
            return new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontMono,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill
            };
        }

        private void LoadOriginalsIntoFields()
        {
            txtPersonality.Text = PersonaReader.GetDefaultPersonality();
            txtSafety.Text = PersonaReader.GetSafety();
            txtMachinery.Text = PersonaReader.GetMachinery();
        }

        private void ShowPromptDrawer()
        {
            txtPersonality.Text = CaprICEHook.CustomPersonality ?? PersonaReader.GetDefaultPersonality();
            txtSafety.Text = CaprICEHook.CustomSafety ?? PersonaReader.GetSafety();
            txtMachinery.Text = CaprICEHook.CustomMachinery ?? PersonaReader.GetMachinery();
            SwitchWorkspaceView(pnlPromptDrawer, btnNavPrompts);
        }
    }
}
