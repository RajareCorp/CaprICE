using System.Drawing;
using System.Windows.Forms;
using CaprICE.Security;

namespace CaprICE.UI
{
    public partial class CaprICEForm
    {
        private void InitializeSettingsDrawer()
        {
            pnlSettingsDrawer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            Panel pnlContent = new Panel
            {
                AutoScroll = true,
                BackColor = NeonTheme.Well
            };

            TableLayoutPanel content = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 6,
                Height = 447,
                BackColor = NeonTheme.Well,
                Location = Point.Empty
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 295F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

            Label lblDesc = new Label
            {
                Text = "Cochez les éléments à masquer automatiquement dans les requêtes cloud :",
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Faint,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            cbApiKeys = CyberControls.Check("Clés API & Tokens secrets", SecurityPolicy.MaskApiKeys, 0);
            cbPasswords = CyberControls.Check("Mots de passe & Credentials", SecurityPolicy.MaskPasswords, 32);
            cbPhones = CyberControls.Check("Numéros de téléphone (FR / Intl)", SecurityPolicy.MaskPhoneNumbers, 64);
            cbIps = CyberControls.Check("Adresses IP (IPv4 & IPv6)", SecurityPolicy.MaskIpAddresses, 96);
            cbAddresses = CyberControls.Check("Adresses postales & Codes postaux", SecurityPolicy.MaskPostalAddresses, 128);
            cbUserPaths = CyberControls.Check("Chemins utilisateurs Windows (C:\\Users)", SecurityPolicy.MaskUserPaths, 160);
            cbEmails = CyberControls.Check("Adresses Email", SecurityPolicy.MaskEmails, 192);
            cbCards = CyberControls.Check("Numéros de carte bancaire", SecurityPolicy.MaskCreditCards, 224);
            cbNames = CyberControls.Check("Noms & Prénoms personnalisés", SecurityPolicy.MaskCustomNames, 256);

            Panel pnlChecks = new Panel { Dock = DockStyle.Fill, Height = 295, BackColor = NeonTheme.Well };
            pnlChecks.Controls.AddRange(new Control[] {
                cbApiKeys, cbPasswords, cbPhones, cbIps, cbAddresses, cbUserPaths, cbEmails, cbCards, cbNames
            });

            Label lblCustomNames = new Label
            {
                Text = "NOMS & PRÉNOMS À MASQUER (séparés par virgules) :",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Cyan,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtCustomNames = CreateSettingsTextBox(SecurityPolicy.CustomNames);

            Label lblCustomAddrs = new Label
            {
                Text = "ADRESSES PRIVÉES / VILLES (séparées par virgules) :",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Cyan,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtCustomAddresses = CreateSettingsTextBox(SecurityPolicy.CustomAddresses);

            content.Controls.Add(lblDesc, 0, 0);
            content.Controls.Add(pnlChecks, 0, 1);
            content.Controls.Add(lblCustomNames, 0, 2);
            content.Controls.Add(txtCustomNames, 0, 3);
            content.Controls.Add(lblCustomAddrs, 0, 4);
            content.Controls.Add(txtCustomAddresses, 0, 5);
            pnlContent.Controls.Add(content);
            pnlContent.SizeChanged += (s, e) => content.Width = pnlContent.ClientSize.Width;

            btnSaveSettings = CyberControls.Button("ENREGISTRER LES PARAMÈTRES", 260, 36, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnSaveSettings.Click += (s, e) => SaveSettingsFromUi();

            Button btnCloseSettings = CyberControls.Button("FERMER", 160, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnCloseSettings.Click += (s, e) => SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);

            pnlSettingsDrawer.Controls.Add(BuildDrawerLayout(
                "PARAMÈTRES DE CONFIDENTIALITÉ & DLP", NeonTheme.Rose, pnlContent,
                btnSaveSettings, btnCloseSettings));
        }

        private TextBox CreateSettingsTextBox(string text)
        {
            return new TextBox
            {
                Text = text,
                Font = NeonTheme.FontSubtitle,
                ForeColor = NeonTheme.White,
                BackColor = NeonTheme.Field,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill
            };
        }

        private void SaveSettingsFromUi()
        {
            SecurityPolicy.MaskApiKeys = cbApiKeys.Checked;
            SecurityPolicy.MaskPasswords = cbPasswords.Checked;
            SecurityPolicy.MaskPhoneNumbers = cbPhones.Checked;
            SecurityPolicy.MaskIpAddresses = cbIps.Checked;
            SecurityPolicy.MaskPostalAddresses = cbAddresses.Checked;
            SecurityPolicy.MaskUserPaths = cbUserPaths.Checked;
            SecurityPolicy.MaskEmails = cbEmails.Checked;
            SecurityPolicy.MaskCreditCards = cbCards.Checked;
            SecurityPolicy.MaskCustomNames = cbNames.Checked;

            SecurityPolicy.CustomNames = txtCustomNames.Text.Trim();
            SecurityPolicy.CustomAddresses = txtCustomAddresses.Text.Trim();

            if (!SecurityPolicy.SaveSettings(out string error))
            {
                MessageBox.Show("Impossible d'enregistrer les paramètres : " + error,
                    "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Paramètres enregistrés !", "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);
        }
    }
}
