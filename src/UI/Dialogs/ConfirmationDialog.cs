using System.Drawing;
using System.Windows.Forms;

namespace CaprICE.UI
{
    /// <summary>Fenêtre modale demandant d'autoriser ou de bloquer un envoi vers une IA externe.</summary>
    internal static class ConfirmationDialog
    {
        /// <summary>À appeler depuis le thread UI. Retourne true si l'utilisateur autorise l'envoi.</summary>
        public static bool Show(Form owner, string content)
        {
            bool approved = false;

            using (Form dlg = new Form())
            {
                dlg.FormBorderStyle = FormBorderStyle.None;
                dlg.Size = new Size(680, 500);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = NeonTheme.Ground;
                dlg.ForeColor = NeonTheme.White;
                NeonTheme.Clip(dlg, 16, 4);

                dlg.Paint += (s, e) =>
                {
                    NeonTheme.Backdrop(e.Graphics, dlg.Size, 80);
                    NeonTheme.Frame(e.Graphics, dlg.Size, 16, 4, 22, 22);
                };

                dlg.MouseDown += (s, e) => NativeMethods.DragWindow(dlg, e);

                Label lblWarn = new Label
                {
                    Text = "CONNEXION EXTERNE // CONFIRMATION REQUISE",
                    Font = NeonTheme.FontTitle,
                    ForeColor = NeonTheme.Rose,
                    Location = new Point(22, 16),
                    AutoSize = true
                };

                TextBox txtPreview = new TextBox
                {
                    Location = new Point(24, 54),
                    Size = new Size(632, 360),
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BackColor = NeonTheme.Field,
                    ForeColor = NeonTheme.White,
                    Font = NeonTheme.FontMono,
                    BorderStyle = BorderStyle.None,
                    Text = "Vérifiez la cible et le contenu avant d'autoriser.\r\n\r\n" + content
                };

                Button btnAllow = new Button
                {
                    Text = "AUTORISER L'ENVOI",
                    Location = new Point(320, 435),
                    Size = new Size(200, 42),
                    BackColor = NeonTheme.Field,
                    ForeColor = NeonTheme.Cyan,
                    Font = NeonTheme.FontBadge,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.OK
                };
                btnAllow.FlatAppearance.BorderColor = NeonTheme.Cyan;

                Button btnBlock = new Button
                {
                    Text = "BLOQUER",
                    Location = new Point(535, 435),
                    Size = new Size(125, 42),
                    BackColor = NeonTheme.Field,
                    ForeColor = NeonTheme.Rose,
                    Font = NeonTheme.FontBadge,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel
                };
                btnBlock.FlatAppearance.BorderColor = NeonTheme.Rose;

                dlg.Controls.Add(lblWarn);
                dlg.Controls.Add(txtPreview);
                dlg.Controls.Add(btnAllow);
                dlg.Controls.Add(btnBlock);

                approved = (dlg.ShowDialog(owner) == DialogResult.OK);
            }

            return approved;
        }
    }
}
