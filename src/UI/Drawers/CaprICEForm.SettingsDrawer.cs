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

            Panel content = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = NeonTheme.Well };
            TableLayoutPanel columns = new TableLayoutPanel
            {
                Location = Point.Empty,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                BackColor = NeonTheme.Well,
                Padding = new Padding(0, 4, 8, 8)
            };
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Panel rules = BuildSectionPanel("DLP // RÈGLES DE MASQUAGE");
            Label ruleIntro = new Label
            {
                Text = "Contexte textuel inspecté ; pas d'OCR des images. Une détection dans le texte multimodal bloque l'envoi.",
                Dock = DockStyle.Top,
                Height = 42,
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Faint,
                TextAlign = ContentAlignment.MiddleLeft
            };

            cbApiKeys = CreateRuleCheck("Clés API & tokens secrets", SecurityPolicy.MaskApiKeys);
            cbPasswords = CreateRuleCheck("Mots de passe & identifiants", SecurityPolicy.MaskPasswords);
            cbPhones = CreateRuleCheck("Numéros de téléphone", SecurityPolicy.MaskPhoneNumbers);
            cbIps = CreateRuleCheck("Adresses IP (IPv4 / IPv6)", SecurityPolicy.MaskIpAddresses);
            cbAddresses = CreateRuleCheck("Adresses postales & codes postaux", SecurityPolicy.MaskPostalAddresses);
            cbUserPaths = CreateRuleCheck("Chemins utilisateurs Windows", SecurityPolicy.MaskUserPaths);
            cbEmails = CreateRuleCheck("Adresses e-mail", SecurityPolicy.MaskEmails);
            cbCards = CreateRuleCheck("Numéros de carte bancaire", SecurityPolicy.MaskCreditCards);
            cbNames = CreateRuleCheck("Noms personnalisés", SecurityPolicy.MaskCustomNames);
            FlowLayoutPanel checks = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                BackColor = NeonTheme.Well,
                Padding = new Padding(0, 4, 0, 4)
            };
            checks.Controls.AddRange(new Control[] {
                cbApiKeys, cbPasswords, cbPhones, cbIps, cbAddresses,
                cbUserPaths, cbEmails, cbCards, cbNames
            });
            rules.Controls.Add(checks);
            rules.Controls.Add(ruleIntro);
            SetSectionOrder(rules, ruleIntro, checks);

            Panel custom = BuildSectionPanel("DONNÉES PERSONNELLES");
            Label lblCustomNames = CreateSettingsSectionLabel("NOMS & PRÉNOMS À MASQUER");
            txtCustomNames = CreateSettingsTextBox(SecurityPolicy.CustomNames, 72);
            Label lblCustomAddrs = CreateSettingsSectionLabel("ADRESSES PRIVÉES À MASQUER");
            txtCustomAddresses = CreateSettingsTextBox(SecurityPolicy.CustomAddresses, 100);
            Label customHint = new Label
            {
                Text = "Sépare les entrées par une virgule. Ces valeurs ne sont pas incluses dans le journal DLP.",
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Faint,
                Dock = DockStyle.Top,
                Height = 48,
                TextAlign = ContentAlignment.MiddleLeft
            };
            custom.Controls.Add(customHint);
            custom.Controls.Add(txtCustomAddresses);
            custom.Controls.Add(lblCustomAddrs);
            custom.Controls.Add(txtCustomNames);
            custom.Controls.Add(lblCustomNames);

            Label networkHint = new Label
            {
                Text = "Les modes de la barre supérieure contrôlent les URL réelles de Brain et Voice. Les fournisseurs personnalisés sont classés à partir de leur hôte ; une destination externe inconnue reste externe.",
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Cyan,
                BackColor = NeonTheme.Deep,
                Dock = DockStyle.Top,
                Height = 112,
                Padding = new Padding(10),
                TextAlign = ContentAlignment.MiddleLeft
            };
            custom.Controls.Add(networkHint);
            SetSectionOrder(custom, networkHint, lblCustomNames, txtCustomNames,
                lblCustomAddrs, txtCustomAddresses, customHint);

            columns.Controls.Add(rules, 0, 0);
            columns.Controls.Add(custom, 1, 0);
            content.Controls.Add(columns);
            content.SizeChanged += (s, e) => columns.Width = content.ClientSize.Width;

            btnSaveSettings = CyberControls.Button("ENREGISTRER", 180, 36, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnSaveSettings.Click += (s, e) => SaveSettingsFromUi();
            Button btnCloseSettings = CyberControls.Button("FERMER", 140, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnCloseSettings.Click += (s, e) => SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);

            pnlSettingsDrawer.Controls.Add(BuildDrawerLayout(
                "PARAMÈTRES DE CONFIDENTIALITÉ // MAINFRAME", NeonTheme.Rose,
                content, btnSaveSettings, btnCloseSettings));
        }

        private Panel BuildSectionPanel(string title)
        {
            Panel section = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = NeonTheme.Band,
                Padding = new Padding(12),
                Margin = new Padding(6)
            };
            Label heading = CreateSettingsSectionLabel(title);
            heading.Height = 34;
            heading.Dock = DockStyle.Top;
            section.Tag = heading;
            section.Controls.Add(heading);
            return section;
        }

        private void SetSectionOrder(Panel section, params Control[] orderedContent)
        {
            Control heading = section.Tag as Control;
            if (heading != null) section.Controls.SetChildIndex(heading, 0);
            for (int i = 0; i < orderedContent.Length; i++)
                section.Controls.SetChildIndex(orderedContent[i], i + 1);
        }

        private Label CreateSettingsSectionLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Rose,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private CheckBox CreateRuleCheck(string text, bool isChecked)
        {
            CheckBox check = CyberControls.Check(text, isChecked, 0);
            check.AutoSize = false;
            check.Width = 340;
            check.Height = 30;
            check.Margin = new Padding(2);
            return check;
        }

        private TextBox CreateSettingsTextBox(string text, int height)
        {
            return new TextBox
            {
                Text = text,
                Font = NeonTheme.FontSubtitle,
                ForeColor = NeonTheme.White,
                BackColor = NeonTheme.Field,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Top,
                Height = height,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
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
