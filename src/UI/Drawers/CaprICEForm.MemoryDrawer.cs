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
        // --- TIROIR 1 : Éditeur de Memory.md ---
        private void InitializeMemoryDrawer()
        {
            pnlMemoryDrawer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 500,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            Label lblTitle = new Label
            {
                Text = "ÉDITEUR DE MÉMOIRE (memories.md / me.md)",
                Font = NeonTheme.FontTitle,
                ForeColor = NeonTheme.Cyan,
                Dock = DockStyle.Top,
                Height = 36
            };

            txtMemoryEditor = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = new Font("Consolas", 10F, FontStyle.Regular),
                BorderStyle = BorderStyle.None
            };

            Button btnSave = CyberControls.Button("SAUVEGARDER LA MÉMOIRE", 240, 36, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnSave.Dock = DockStyle.Bottom;
            btnSave.Click += (s, e) =>
            {
                MemoryManager.SaveMemory(txtMemoryEditor.Text);
                MessageBox.Show("Mémoire sauvegardée avec succès !", "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ToggleMemoryDrawer();
            };

            Button btnClose = CyberControls.Button("FERMER [X]", 120, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Dock = DockStyle.Bottom;
            btnClose.Click += (s, e) => ToggleMemoryDrawer();

            pnlMemoryDrawer.Controls.Add(txtMemoryEditor);
            pnlMemoryDrawer.Controls.Add(lblTitle);
            pnlMemoryDrawer.Controls.Add(btnSave);
            pnlMemoryDrawer.Controls.Add(btnClose);
        }

        private void ToggleMemoryDrawer()
        {
            memoryDrawerVisible = !memoryDrawerVisible;
            if (memoryDrawerVisible)
            {
                if (settingsDrawerVisible) ToggleSettingsDrawer();
                if (helpDrawerVisible) ToggleHelpDrawer();
                if (promptDrawerVisible) TogglePromptDrawer();

                txtMemoryEditor.Text = MemoryManager.ReadMemory();
                pnlMemoryDrawer.BringToFront();
            }
            pnlMemoryDrawer.Visible = memoryDrawerVisible;
            btnMemoryToggle.Text = memoryDrawerVisible ? "[X] FERMER" : "MÉMOIRE (.MD)";
        }
    }
}
