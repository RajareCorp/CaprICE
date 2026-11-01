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
        // --- TIROIR 2 : Éditeur du Prompt Initial (Persona Hook) ---
        private void InitializePromptDrawer()
        {
            pnlPromptDrawer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 540,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            Label lblTitle = new Label
            {
                Text = "HOOK SYSTEM PROMPT (Capri.Persona)",
                Font = NeonTheme.FontTitle,
                ForeColor = NeonTheme.Rose,
                Dock = DockStyle.Top,
                Height = 36
            };

            Label lblSub = new Label
            {
                Text = "Le texte ci-dessous remplace dynamiquement le prompt système de Capri.",
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Faint,
                Dock = DockStyle.Top,
                Height = 24
            };

            txtPromptEditor = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = new Font("Consolas", 10F, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10)
            };

            // Bouton pour recharger le prompt original extrait de Capri
            Button btnFetchOriginal = CyberControls.Button("RÉCUPÉRER PROMPT D'ORIGINE", 230, 34, NeonTheme.Edge, NeonTheme.Cyan);
            btnFetchOriginal.Dock = DockStyle.Bottom;
            btnFetchOriginal.Click += (s, e) =>
            {
                txtPromptEditor.Text = PersonaReader.GetOriginalCapriPrompt();
            };

            Button btnSave = CyberControls.Button("APPLIQUER LE HOOK PROMPT", 250, 36, NeonTheme.DeepEdge, NeonTheme.Rose);
            btnSave.Dock = DockStyle.Bottom;
            btnSave.Click += (s, e) =>
            {
                string prompt = txtPromptEditor.Text.Trim();
                CaprICEHook.CustomSystemPrompt = string.IsNullOrEmpty(prompt) ? null : prompt;
                MessageBox.Show("Nouveau System Prompt injecté via Harmony !", "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                TogglePromptDrawer();
            };

            Button btnClose = CyberControls.Button("FERMER [X]", 120, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Dock = DockStyle.Bottom;
            btnClose.Click += (s, e) => TogglePromptDrawer();

            pnlPromptDrawer.Controls.Add(txtPromptEditor);
            pnlPromptDrawer.Controls.Add(lblSub);
            pnlPromptDrawer.Controls.Add(lblTitle);
            pnlPromptDrawer.Controls.Add(btnFetchOriginal);
            pnlPromptDrawer.Controls.Add(btnSave);
            pnlPromptDrawer.Controls.Add(btnClose);
        }

        private void TogglePromptDrawer()
        {
            promptDrawerVisible = !promptDrawerVisible;
            if (promptDrawerVisible)
            {
                if (settingsDrawerVisible) ToggleSettingsDrawer();
                if (helpDrawerVisible) ToggleHelpDrawer();
                if (memoryDrawerVisible) ToggleMemoryDrawer();

                // Si aucun prompt personnalisé n'est encore défini, on extrait le prompt original de Capri
                if (string.IsNullOrEmpty(CaprICEHook.CustomSystemPrompt))
                {
                    txtPromptEditor.Text = PersonaReader.GetOriginalCapriPrompt();
                }
                else
                {
                    txtPromptEditor.Text = CaprICEHook.CustomSystemPrompt;
                }

                pnlPromptDrawer.BringToFront();
            }
            pnlPromptDrawer.Visible = promptDrawerVisible;
            btnPromptToggle.Text = promptDrawerVisible ? "[X] FERMER" : "SYSTEM PROMPT";
        }
    }
}
