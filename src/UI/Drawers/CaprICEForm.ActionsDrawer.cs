using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CaprICE.Security;

namespace CaprICE.UI
{
    public partial class CaprICEForm
    {
        private ListBox lstAppLaunchActions;
        private TextBox txtActionKeyword;
        private TextBox txtActionFile;

        private void InitializeActionsDrawer()
        {
            pnlActionsDrawer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            Panel content = new Panel { Dock = DockStyle.Fill, BackColor = NeonTheme.Well };
            Label description = new Label
            {
                Dock = DockStyle.Top,
                Height = 62,
                Text = "Associez un mot exact à un fichier local. Windows l'ouvrira selon son type ; certains fichiers, comme les scripts, peuvent s'exécuter.",
                ForeColor = NeonTheme.Faint,
                Font = NeonTheme.FontBody,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lstAppLaunchActions = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontMono,
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false
            };
            lstAppLaunchActions.SelectedIndexChanged += (s, e) => LoadSelectedAction();

            TableLayoutPanel fields = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 76,
                ColumnCount = 3,
                RowCount = 2,
                BackColor = NeonTheme.Well,
                Padding = new Padding(0, 8, 0, 0)
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            txtActionKeyword = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtActionFile = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontSmall,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true
            };
            Button btnBrowse = CyberControls.Button("PARCOURIR", 94, 28, NeonTheme.Edge, NeonTheme.Cyan);
            btnBrowse.Dock = DockStyle.Fill;
            btnBrowse.Click += BrowseActionExecutable;

            fields.Controls.Add(CreateActionFieldLabel("Mot-clé"), 0, 0);
            fields.Controls.Add(txtActionKeyword, 1, 0);
            fields.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill }, 2, 0);
            fields.Controls.Add(CreateActionFieldLabel("Fichier"), 0, 1);
            fields.Controls.Add(txtActionFile, 1, 1);
            fields.Controls.Add(btnBrowse, 2, 1);

            content.Controls.Add(lstAppLaunchActions);
            content.Controls.Add(fields);
            content.Controls.Add(description);

            Button btnSaveAction = CyberControls.Button("AJOUTER / METTRE À JOUR", 250, 36, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnSaveAction.Click += SaveAppLaunchAction;
            Button btnRemoveAction = CyberControls.Button("SUPPRIMER L'ASSOCIATION", 240, 36, NeonTheme.Edge, NeonTheme.Rose);
            btnRemoveAction.Click += RemoveAppLaunchAction;
            Button btnClose = CyberControls.Button("FERMER", 120, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Click += (s, e) => SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);

            pnlActionsDrawer.Controls.Add(BuildDrawerLayout(
                "ACTIONS LOCALES // MOT-CLÉ → APPLICATION", NeonTheme.Cyan, content,
                btnSaveAction, btnRemoveAction, btnClose));
            RefreshAppLaunchActions();
        }

        private Label CreateActionFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                ForeColor = NeonTheme.Cyan,
                Font = NeonTheme.FontSmall,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private void BrowseActionExecutable(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Choisir le fichier à ouvrir";
                dialog.Filter = "Tous les fichiers (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    txtActionFile.Text = dialog.FileName;
            }
        }

        private void SaveAppLaunchAction(object sender, EventArgs e)
        {
            try
            {
                string keyword = AppLaunchActions.NormalizeCommand(txtActionKeyword.Text);
                AppLaunchActions.Validate(keyword, txtActionFile.Text, true);
                List<AppLaunchAction> updated = new List<AppLaunchAction>(AppLaunchActions.GetAll());
                if (lstAppLaunchActions.SelectedItem is AppLaunchAction selected)
                    updated.Remove(selected);
                updated.RemoveAll(action => string.Equals(action.Keyword, keyword, StringComparison.OrdinalIgnoreCase));
                updated.Add(new AppLaunchAction(keyword, txtActionFile.Text));
                PersistAppLaunchActions(updated);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show("Impossible d'enregistrer l'action : " + ex.Message,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveAppLaunchAction(object sender, EventArgs e)
        {
            if (!(lstAppLaunchActions.SelectedItem is AppLaunchAction selected)) return;
            List<AppLaunchAction> updated = new List<AppLaunchAction>(AppLaunchActions.GetAll());
            updated.Remove(selected);
            PersistAppLaunchActions(updated);
        }

        private void PersistAppLaunchActions(List<AppLaunchAction> updated)
        {
            IList<AppLaunchAction> previous = AppLaunchActions.GetAll();
            AppLaunchActions.ReplaceAll(updated);
            string error;
            if (!SecurityPolicy.SaveSettings(out error))
            {
                AppLaunchActions.ReplaceAll(previous);
                MessageBox.Show("Les associations n'ont pas été sauvegardées : " + error,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            RefreshAppLaunchActions();
            txtActionKeyword.Clear();
            txtActionFile.Clear();
        }

        private void RefreshAppLaunchActions()
        {
            if (lstAppLaunchActions == null) return;
            lstAppLaunchActions.BeginUpdate();
            lstAppLaunchActions.Items.Clear();
            foreach (AppLaunchAction action in AppLaunchActions.GetAll())
                lstAppLaunchActions.Items.Add(action);
            lstAppLaunchActions.EndUpdate();
        }

        private void LoadSelectedAction()
        {
            AppLaunchAction selected = lstAppLaunchActions.SelectedItem as AppLaunchAction;
            if (selected == null) return;
            txtActionKeyword.Text = selected.Keyword;
            txtActionFile.Text = selected.FilePath;
        }

        private void ShowActionsDrawer()
        {
            RefreshAppLaunchActions();
            SwitchWorkspaceView(pnlActionsDrawer, btnNavActions);
        }
    }
}
