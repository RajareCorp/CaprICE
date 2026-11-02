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
                MessageBox.Show("Unable to read memory: " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Access denied while reading memory: " + ex.Message,
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

            Button btnSave = CyberControls.Button("SAVE MEMORY", 240, 36, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnSave.Click += (s, e) =>
            {
                try
                {
                    MemoryManager.SaveMemory(txtMemoryEditor.Text);
                    MessageBox.Show("Memory saved successfully.", "CaprICE",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Unable to save memory: " + ex.Message,
                        "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Access denied while saving memory: " + ex.Message,
                        "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            Button btnClose = CyberControls.Button("CLOSE", 120, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Click += (s, e) => SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);
            Button btnPurge = CyberControls.Button("PURGE LOCAL DATA", 290, 36, NeonTheme.Edge, NeonTheme.Rose);
            btnPurge.Click += PurgeLocalMemory;

            pnlMemoryDrawer.Controls.Add(BuildDrawerLayout(
                "CAPRI MEMORY // ACTIVE NOTE, HISTORY & TRANSCRIPTS", NeonTheme.Cyan,
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
                MessageBox.Show("Unable to verify the active note path: " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Access denied while verifying the active note path: " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Invalid active note path: " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show("Unsupported active note path: " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (SecurityException ex)
            {
                MessageBox.Show("Unable to verify the active note path: " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StringBuilder prompt = new StringBuilder();
            prompt.AppendLine("This action deletes local Capri data:");
            prompt.AppendLine("• chat history");
            prompt.AppendLine("• local me.md / memories.md notes");
            prompt.AppendLine("• transcripts and active context cache");
            if (!string.IsNullOrEmpty(vaultNote))
            {
                prompt.AppendLine();
                prompt.AppendLine("The active note in the external vault will be preserved:");
                prompt.AppendLine(vaultNote);
            }
            prompt.AppendLine();
            prompt.Append("This deletion is permanent. Continue?");

            DialogResult answer = MessageBox.Show(this, prompt.ToString(), "MEMORY PURGE // CONFIRMATION",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;

            MemoryPurgeResult result;
            try
            {
                result = MemoryPurgeManager.PurgeLocalData();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Purge failed: " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Access denied during purge: " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Invalid path during purge: " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show("Unsupported path during purge: " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (SecurityException ex)
            {
                MessageBox.Show("Purge failed: " + ex.Message,
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
                MessageBox.Show("Local data was processed, but the preserved note could not be reloaded: "
                    + ex.Message, "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException ex)
            {
                txtMemoryEditor.Clear();
                MessageBox.Show("Local data was processed, but access to the preserved note was denied: "
                    + ex.Message, "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            ClearChatDrawerCache();
            if (result.Errors.Count > 0)
            {
                MessageBox.Show("Partial purge. Some data could not be deleted:\r\n\r\n"
                    + string.Join("\r\n", result.Errors.ToArray()),
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string summary = "Local data deleted: " + result.DeletedPaths.Count + " file(s).";
            if (!string.IsNullOrEmpty(result.PreservedVaultNote))
                summary += "\r\n\r\nPreserved vault note:\r\n" + result.PreservedVaultNote
                    + "\r\n\r\nIt remains active and may be read again during future requests.";
            MessageBox.Show(summary, "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
