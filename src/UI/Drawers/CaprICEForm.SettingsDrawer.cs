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
        private void InitializeSettingsDrawer()
        {
            pnlSettingsDrawer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 480,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            Label lblSetTitle = new Label
            {
                Text = "PARAMÈTRES DE CONFIDENTIALITÉ & DLP",
                Font = NeonTheme.FontTitle,
                ForeColor = NeonTheme.Rose,
                Dock = DockStyle.Top,
                Height = 36
            };

            Panel pnlContent = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            Label lblDesc = new Label
            {
                Text = "Cochez les éléments à masquer automatiquement dans les requêtes cloud :",
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Faint,
                Dock = DockStyle.Top,
                Height = 28
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

            Panel pnlChecks = new Panel { Dock = DockStyle.Top, Height = 295 };
            pnlChecks.Controls.AddRange(new Control[] {
                cbApiKeys, cbPasswords, cbPhones, cbIps, cbAddresses, cbUserPaths, cbEmails, cbCards, cbNames
            });

            Label lblCustomNames = new Label
            {
                Text = "NOMS & PRÉNOMS À MASQUER (séparés par virgules) :",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Cyan,
                Dock = DockStyle.Top,
                Height = 26
            };

            txtCustomNames = new TextBox
            {
                Text = SecurityPolicy.CustomNames,
                Font = NeonTheme.FontSubtitle,
                ForeColor = NeonTheme.White,
                BackColor = NeonTheme.Field,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Top,
                Height = 28
            };

            Label lblCustomAddrs = new Label
            {
                Text = "ADRESSES PRIVÉES / VILLES (séparées par virgules) :",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Cyan,
                Dock = DockStyle.Top,
                Height = 26
            };

            txtCustomAddresses = new TextBox
            {
                Text = SecurityPolicy.CustomAddresses,
                Font = NeonTheme.FontSubtitle,
                ForeColor = NeonTheme.White,
                BackColor = NeonTheme.Field,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Top,
                Height = 28
            };

            btnSaveSettings = CyberControls.Button("ENREGISTRER LES PARAMÈTRES", 260, 36, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnSaveSettings.Dock = DockStyle.Bottom;
            btnSaveSettings.Click += (s, e) => SaveSettingsFromUi();

            Button btnCloseSettings = CyberControls.Button("FERMER LE VOLET [X]", 160, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnCloseSettings.Dock = DockStyle.Bottom;
            btnCloseSettings.Click += (s, e) => ToggleSettingsDrawer();

            pnlContent.Controls.Add(txtCustomAddresses);
            pnlContent.Controls.Add(lblCustomAddrs);
            pnlContent.Controls.Add(txtCustomNames);
            pnlContent.Controls.Add(lblCustomNames);
            pnlContent.Controls.Add(pnlChecks);
            pnlContent.Controls.Add(lblDesc);

            pnlSettingsDrawer.Controls.Add(pnlContent);
            pnlSettingsDrawer.Controls.Add(btnSaveSettings);
            pnlSettingsDrawer.Controls.Add(btnCloseSettings);
            pnlSettingsDrawer.Controls.Add(lblSetTitle);
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

            SecurityPolicy.SaveSettings();
            MessageBox.Show("Paramètres enregistrés !", "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ToggleSettingsDrawer();
        }

        private void ToggleSettingsDrawer()
        {
            settingsDrawerVisible = !settingsDrawerVisible;
            if (settingsDrawerVisible && helpDrawerVisible) ToggleHelpDrawer();
            pnlSettingsDrawer.Visible = settingsDrawerVisible;
            if (settingsDrawerVisible) pnlSettingsDrawer.BringToFront();
            btnSettingsToggle.Text = settingsDrawerVisible ? "[X] FERMER" : "PARAMÈTRES DLP";
        }
    }
}
