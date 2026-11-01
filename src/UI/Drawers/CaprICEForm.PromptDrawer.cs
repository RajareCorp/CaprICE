using System;
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

        // --- TIROIR 2 : Éditeur du Prompt Initial (Persona Hook) ---
        private void InitializePromptDrawer()
        {
            pnlPromptDrawer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 560,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            Label lblTitle = new Label
            {
                Text = "HOOK SYSTEM PROMPT (MODULAIRE)",
                Font = NeonTheme.FontTitle,
                ForeColor = NeonTheme.Rose,
                Dock = DockStyle.Top,
                Height = 36
            };

            // Zone défilante pour héberger les 3 sections
            Panel pnlContentScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            // Section 1: DefaultPersonality
            Label lblPerso = CreateSectionLabel("1. PERSONALITY (Personnage, Ton, Lore)");
            txtPersonality = CreateCodeTextBox(120);

            // Section 2: Safety
            Label lblSafety = CreateSectionLabel("2. SAFETY (Sécurité, Limites, Comportement)");
            txtSafety = CreateCodeTextBox(90);

            // Section 3: Machinery
            Label lblMachinery = CreateSectionLabel("3. MACHINERY (Contraintes de Format & Voice)");
            txtMachinery = CreateCodeTextBox(90);

            // Empilement des composants de haut en bas
            pnlContentScroll.Controls.Add(txtMachinery);
            pnlContentScroll.Controls.Add(lblMachinery);
            pnlContentScroll.Controls.Add(txtSafety);
            pnlContentScroll.Controls.Add(lblSafety);
            pnlContentScroll.Controls.Add(txtPersonality);
            pnlContentScroll.Controls.Add(lblPerso);

            Button btnFetchOriginal = CyberControls.Button("RECHARGER PROMPTS ORIGINAUX", 240, 34, NeonTheme.Edge, NeonTheme.Cyan);
            btnFetchOriginal.Dock = DockStyle.Bottom;
            btnFetchOriginal.Click += (s, e) => LoadOriginalsIntoFields();

            Button btnSave = CyberControls.Button("APPLIQUER LES HOOKS PROMPT", 250, 36, NeonTheme.DeepEdge, NeonTheme.Rose);
            btnSave.Dock = DockStyle.Bottom;
            btnSave.Click += (s, e) =>
            {
                CaprICEHook.CustomPersonality = string.IsNullOrWhiteSpace(txtPersonality.Text) ? null : txtPersonality.Text.Trim();
                CaprICEHook.CustomSafety = string.IsNullOrWhiteSpace(txtSafety.Text) ? null : txtSafety.Text.Trim();
                CaprICEHook.CustomMachinery = string.IsNullOrWhiteSpace(txtMachinery.Text) ? null : txtMachinery.Text.Trim();

                CaprICEHook.ClearBrainHistory();
                MessageBox.Show("Nouveau System Prompt modulaire appliqué avec succès !", "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                TogglePromptDrawer();
            };

            Button btnClose = CyberControls.Button("FERMER [X]", 120, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Dock = DockStyle.Bottom;
            btnClose.Click += (s, e) => TogglePromptDrawer();

            pnlPromptDrawer.Controls.Add(pnlContentScroll);
            pnlPromptDrawer.Controls.Add(lblTitle);
            pnlPromptDrawer.Controls.Add(btnFetchOriginal);
            pnlPromptDrawer.Controls.Add(btnSave);
            pnlPromptDrawer.Controls.Add(btnClose);
        }

        private Label CreateSectionLabel(string title)
        {
            return new Label
            {
                Text = title,
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Cyan,
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.BottomLeft
            };
        }

        private TextBox CreateCodeTextBox(int height)
        {
            return new TextBox
            {
                Dock = DockStyle.Top,
                Height = height,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = new Font("Consolas", 10F, FontStyle.Regular),
                BorderStyle = BorderStyle.None
            };
        }

        private void LoadOriginalsIntoFields()
        {
            txtPersonality.Text = PersonaReader.GetDefaultPersonality();
            txtSafety.Text = PersonaReader.GetSafety();
            txtMachinery.Text = PersonaReader.GetMachinery();
        }

        private void TogglePromptDrawer()
        {
            promptDrawerVisible = !promptDrawerVisible;
            if (promptDrawerVisible)
            {
                if (settingsDrawerVisible) ToggleSettingsDrawer();
                if (helpDrawerVisible) ToggleHelpDrawer();

                txtPersonality.Text = CaprICEHook.CustomPersonality ?? PersonaReader.GetDefaultPersonality();
                txtSafety.Text = CaprICEHook.CustomSafety ?? PersonaReader.GetSafety();
                txtMachinery.Text = CaprICEHook.CustomMachinery ?? PersonaReader.GetMachinery();

                pnlPromptDrawer.BringToFront();
            }
            pnlPromptDrawer.Visible = promptDrawerVisible;
        }
    }
}