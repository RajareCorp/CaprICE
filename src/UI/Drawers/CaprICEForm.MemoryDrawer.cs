using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CaprICE.UI
{
    public partial class CaprICEForm
    {
        private void ShowMemoryDrawer()
        {
            try
            {
                txtMemoryEditor.Text = MemoryManager.ReadMemory();
                SwitchWorkspaceView(pnlMemoryDrawer, btnNavMemory);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Impossible de lire la mémoire : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Accès refusé lors de la lecture de la mémoire : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeMemoryDrawer()
        {
            pnlMemoryDrawer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            txtMemoryEditor = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontMono,
                BorderStyle = BorderStyle.None
            };

            Button btnSave = CyberControls.Button("SAUVEGARDER LA MÉMOIRE", 240, 36, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnSave.Click += (s, e) =>
            {
                try
                {
                    MemoryManager.SaveMemory(txtMemoryEditor.Text);
                    MessageBox.Show("Mémoire sauvegardée avec succès !", "CaprICE",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Impossible de sauvegarder la mémoire : " + ex.Message,
                        "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Accès refusé lors de la sauvegarde de la mémoire : " + ex.Message,
                        "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            Button btnClose = CyberControls.Button("FERMER", 120, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Click += (s, e) => SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);

            pnlMemoryDrawer.Controls.Add(BuildDrawerLayout(
                "ÉDITEUR DE MÉMOIRE (memories.md / me.md)", NeonTheme.Cyan, txtMemoryEditor, btnSave, btnClose));
        }
    }
}
