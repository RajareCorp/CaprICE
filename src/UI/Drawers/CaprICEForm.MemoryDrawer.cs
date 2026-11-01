using System;
using System.Drawing;
using System.IO;
using System.Security;
using System.Text;
using System.Windows.Forms;
using CaprICE.Services;

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

            txtMemoryEditor = new ReadableTextEditor(NeonTheme.Cyan);

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
            Button btnPurge = CyberControls.Button("PURGER LES DONNÉES LOCALES", 290, 36, NeonTheme.Edge, NeonTheme.Rose);
            btnPurge.Click += PurgeLocalMemory;

            pnlMemoryDrawer.Controls.Add(BuildDrawerLayout(
                "MÉMOIRE CAPRI // NOTE ACTIVE, HISTORIQUE ET TRANSCRIPTIONS", NeonTheme.Cyan,
                txtMemoryEditor, btnSave, btnPurge, btnClose));
        }

        private void PurgeLocalMemory(object sender, System.EventArgs e)
        {
            string vaultNote;
            try
            {
                vaultNote = MemoryPurgeManager.GetVaultNoteToPreserve();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Impossible de vérifier le chemin de la note active : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Accès refusé lors de la vérification de la note active : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Chemin de note active invalide : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show("Chemin de note active non pris en charge : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (SecurityException ex)
            {
                MessageBox.Show("Impossible de vérifier le chemin de la note active : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StringBuilder prompt = new StringBuilder();
            prompt.AppendLine("Cette action supprime les données Capri locales :");
            prompt.AppendLine("• historique du chat");
            prompt.AppendLine("• notes locales me.md / memories.md");
            prompt.AppendLine("• transcriptions et cache de contexte actif");
            if (!string.IsNullOrEmpty(vaultNote))
            {
                prompt.AppendLine();
                prompt.AppendLine("La note active du vault externe sera conservée :");
                prompt.AppendLine(vaultNote);
            }
            prompt.AppendLine();
            prompt.Append("Cette suppression est irréversible. Continuer ?");

            DialogResult answer = MessageBox.Show(this, prompt.ToString(), "PURGE MÉMOIRE // CONFIRMATION",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;

            MemoryPurgeResult result;
            try
            {
                result = MemoryPurgeManager.PurgeLocalData();
            }
            catch (IOException ex)
            {
                MessageBox.Show("La purge a échoué : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Accès refusé pendant la purge : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Chemin invalide pendant la purge : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show("Chemin non pris en charge pendant la purge : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (SecurityException ex)
            {
                MessageBox.Show("La purge a échoué : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                txtMemoryEditor.Text = MemoryManager.ReadMemory();
            }
            catch (IOException ex)
            {
                txtMemoryEditor.Clear();
                MessageBox.Show("Les données locales ont été traitées, mais la note conservée ne peut pas être rechargée : "
                    + ex.Message, "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException ex)
            {
                txtMemoryEditor.Clear();
                MessageBox.Show("Les données locales ont été traitées, mais l'accès à la note conservée est refusé : "
                    + ex.Message, "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            ClearChatDrawerCache();
            if (result.Errors.Count > 0)
            {
                MessageBox.Show("Purge partielle. Certaines données n'ont pas pu être supprimées :\r\n\r\n"
                    + string.Join("\r\n", result.Errors.ToArray()),
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string summary = "Données locales supprimées : " + result.DeletedPaths.Count + " fichier(s).";
            if (!string.IsNullOrEmpty(result.PreservedVaultNote))
                summary += "\r\n\r\nNote du vault préservée :\r\n" + result.PreservedVaultNote
                    + "\r\n\r\nElle reste active et pourra être relue lors des prochaines requêtes.";
            MessageBox.Show(summary, "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
