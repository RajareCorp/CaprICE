using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CaprICE
{
    public class CaprICEForm : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private static CaprICEForm Instance;

        // Custom Cyber Window Controls
        private Panel pnlTopBar;
        private Button btnClose;
        private Button btnMin;
        private Button btnSettingsToggle;
        private Button btnHelpToggle;

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblStatusChip;
        private Label lblStats;

        // Policy Mode Radio Buttons
        private RadioButton rbMonitor;
        private RadioButton rbDlp;
        private RadioButton rbStrictLocal;
        private RadioButton rbAskConfirm;

        // Interactive Explanation Banner
        private Panel pnlExplanation;
        private Label lblExplainTitle;
        private Label lblExplainBody;

        // Settings Drawer Panel
        private Panel pnlSettingsDrawer;
        private bool settingsDrawerVisible = false;

        // Help Drawer Panel
        private Panel pnlHelpDrawer;
        private bool helpDrawerVisible = false;

        // Settings Controls inside Settings Drawer
        private CheckBox cbApiKeys;
        private CheckBox cbPasswords;
        private CheckBox cbPhones;
        private CheckBox cbIps;
        private CheckBox cbAddresses;
        private CheckBox cbUserPaths;
        private CheckBox cbEmails;
        private CheckBox cbCards;
        private CheckBox cbNames;
        private TextBox txtCustomNames;
        private TextBox txtCustomAddresses;
        private Button btnSaveSettings;

        // Main View
        private ListView lvEvents;
        private RichTextBox rtbInspector; // Utilisation de RichTextBox pour un rendu enrichi
        private TextBox txtProxyInput;
        private Button btnClear;
        private Button btnOpenAudit;
        private Button btnSetProxy;

        private ToolTip toolTips;

        private int totalCount = 0;
        private int allowedCount = 0;
        private int blockedCount = 0;
        private int dlpCount = 0;

        private List<InterceptEvent> eventsList = new List<InterceptEvent>();

        private Button btnMemoryToggle;
        private Button btnPromptToggle;

        private Panel pnlMemoryDrawer;
        private bool memoryDrawerVisible = false;
        private TextBox txtMemoryEditor;

        private Panel pnlPromptDrawer;
        private bool promptDrawerVisible = false;
        private TextBox txtPromptEditor;

        public CaprICEForm()
        {
            Instance = this;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            InitializeCyberWindow();
        }

        private void InitializeCyberWindow()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(1280, 840);
            this.MinimumSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = NeonTheme.Ground;
            this.ForeColor = NeonTheme.White;
            this.Font = NeonTheme.FontBody;

            toolTips = new ToolTip
            {
                BackColor = NeonTheme.Well,
                ForeColor = NeonTheme.Cyan
            };

            // Top Header Bar
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 92,
                BackColor = Color.Transparent
            };
            pnlTopBar.MouseDown += OnDragWindow;

            lblTitle = new Label
            {
                Text = "CAPR-ICE // AI SECURITY",
                Font = NeonTheme.FontDisplay,
                ForeColor = NeonTheme.Rose,
                Location = new Point(24, 14),
                AutoSize = true
            };
            lblTitle.MouseDown += OnDragWindow;

            lblSubtitle = new Label
            {
                Text = "SENTINEL INTERCEPTOR // REAL-TIME TRAFFIC & DLP CONTROL // CONTEXT & MEMORY AUDIT",
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Faint,
                Location = new Point(26, 44),
                AutoSize = true
            };
            lblSubtitle.MouseDown += OnDragWindow;

            lblStatusChip = new Label
            {
                Text = "ACTIVE // HOOKED",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.OnInk,
                BackColor = NeonTheme.Cyan,
                Size = new Size(140, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblStats = new Label
            {
                Text = "REQ: 0  |  ALLOWED: 0  |  BLOCKED: 0  |  DLP DETECTED: 0",
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Cyan,
                Location = new Point(26, 66),
                AutoSize = true
            };
            lblStats.MouseDown += OnDragWindow;

            // Header Buttons
            btnSettingsToggle = CreateCyberButton("PARAMÈTRES DLP", 150, 30, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnSettingsToggle.Click += (s, e) => ToggleSettingsDrawer();

            btnHelpToggle = CreateCyberButton("? GUIDE", 90, 30, NeonTheme.Edge, NeonTheme.White);
            btnHelpToggle.Click += (s, e) => ToggleHelpDrawer();

            btnMin = CreateCyberButton("—", 42, 30, NeonTheme.Edge, NeonTheme.White);
            btnMin.Click += (s, e) => { this.WindowState = FormWindowState.Minimized; };

            btnClose = CreateCyberButton("✕ FERMER", 95, 30, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Click += (s, e) => { Application.Exit(); };

            btnMemoryToggle = CreateCyberButton("MÉMOIRE (.MD)", 130, 30, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnMemoryToggle.Click += (s, e) => ToggleMemoryDrawer();

            btnPromptToggle = CreateCyberButton("SYSTEM PROMPT", 130, 30, NeonTheme.DeepEdge, NeonTheme.Rose);
            btnPromptToggle.Click += (s, e) => TogglePromptDrawer();

            pnlTopBar.Controls.Add(btnMemoryToggle);
            pnlTopBar.Controls.Add(btnPromptToggle);

            pnlTopBar.Controls.Add(lblTitle);
            pnlTopBar.Controls.Add(lblSubtitle);
            pnlTopBar.Controls.Add(lblStatusChip);
            pnlTopBar.Controls.Add(lblStats);
            pnlTopBar.Controls.Add(btnSettingsToggle);
            pnlTopBar.Controls.Add(btnHelpToggle);
            pnlTopBar.Controls.Add(btnMin);
            pnlTopBar.Controls.Add(btnClose);

            pnlTopBar.Resize += (s, e) => LayoutHeaderButtons();
            this.Resize += (s, e) => { LayoutHeaderButtons(); NeonTheme.Clip(this, 20, 6); };

            // Policy Selection Bar
            Panel pnlModes = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = NeonTheme.Band
            };

            Label lblModeTag = new Label
            {
                Text = "POLITIQUE :",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Rose,
                Location = new Point(24, 14),
                AutoSize = true
            };

            rbMonitor = CreateRadio("1. Monitor Only", 130, SecurityPolicy.CurrentMode == PolicyMode.MonitorOnly, PolicyMode.MonitorOnly, "Mode passif : surveille et journalise chaque requête sans jamais bloquer.");
            rbDlp = CreateRadio("2. Enforce DLP (Masquage)", 290, SecurityPolicy.CurrentMode == PolicyMode.EnforceDlp, PolicyMode.EnforceDlp, "Pare-feu DLP : détecte et masque automatiquement clés API, noms, téléphones, IP, adresses.");
            rbStrictLocal = CreateRadio("3. Strict Local (Bloquer Cloud)", 510, SecurityPolicy.CurrentMode == PolicyMode.BlockExternal, PolicyMode.BlockExternal, "Verrou local : bloque instantanément toute connexion vers les IA externes.");
            rbAskConfirm = CreateRadio("4. Ask Confirmation", 750, SecurityPolicy.CurrentMode == PolicyMode.AskConfirmation, PolicyMode.AskConfirmation, "Contrôle interactif : demande une validation humaine avant chaque envoi cloud.");

            pnlModes.Controls.Add(lblModeTag);
            pnlModes.Controls.Add(rbMonitor);
            pnlModes.Controls.Add(rbDlp);
            pnlModes.Controls.Add(rbStrictLocal);
            pnlModes.Controls.Add(rbAskConfirm);

            // Explanation Banner
            pnlExplanation = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = NeonTheme.Deep,
                Padding = new Padding(24, 0, 24, 0)
            };

            lblExplainTitle = new Label
            {
                Text = "[MODE ACTIF] 1. MONITOR ONLY :",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Cyan,
                Location = new Point(24, 11),
                AutoSize = true
            };

            lblExplainBody = new Label
            {
                Text = "Audit passif. Tous les prompts, contextes et mémoires envoyés aux modèles sont affichés en toute transparence.",
                Font = NeonTheme.FontBody,
                ForeColor = NeonTheme.White,
                Location = new Point(265, 11),
                AutoSize = true
            };

            pnlExplanation.Controls.Add(lblExplainTitle);
            pnlExplanation.Controls.Add(lblExplainBody);
            UpdateExplanationBanner(SecurityPolicy.CurrentMode);

            // Bottom Actions Panel
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = NeonTheme.Band
            };

            btnClear = CreateCyberButton("VIDER LISTE", 125, 32, NeonTheme.Edge, NeonTheme.White);
            btnClear.Location = new Point(24, 10);
            btnClear.Click += (s, e) =>
            {
                lvEvents.Items.Clear();
                eventsList.Clear();
                rtbInspector.Clear();
            };

            btnOpenAudit = CreateCyberButton("OUVRIR JOURNAL (LOG)", 185, 32, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnOpenAudit.Location = new Point(160, 10);
            btnOpenAudit.Click += (s, e) =>
            {
                if (File.Exists(SecurityPolicy.AuditLogPath))
                    Process.Start("notepad.exe", SecurityPolicy.AuditLogPath);
                else
                    MessageBox.Show("Aucune entrée d'audit pour le moment.", "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            Label lblProxy = new Label
            {
                Text = "PROXY AMONT (OPTIONNEL) :",
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Faint,
                Location = new Point(570, 17),
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            txtProxyInput = new TextBox
            {
                Text = "http://127.0.0.1:8080",
                Font = NeonTheme.FontMono,
                ForeColor = NeonTheme.White,
                BackColor = NeonTheme.Field,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(200, 26),
                Location = new Point(765, 13),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            btnSetProxy = CreateCyberButton("ACTIVER PROXY", 130, 28, NeonTheme.Edge, NeonTheme.White);
            btnSetProxy.Location = new Point(980, 12);
            btnSetProxy.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSetProxy.Click += (s, e) =>
            {
                CaprICEHook.CustomProxyUrl = txtProxyInput.Text.Trim();
                MessageBox.Show("Proxy amont configuré : " + CaprICEHook.CustomProxyUrl, "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            pnlBottom.Controls.Add(btnClear);
            pnlBottom.Controls.Add(btnOpenAudit);
            pnlBottom.Controls.Add(lblProxy);
            pnlBottom.Controls.Add(txtProxyInput);
            pnlBottom.Controls.Add(btnSetProxy);

            // Split Container
            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 340,
                BackColor = NeonTheme.Brim,
                SplitterWidth = 3
            };

            // Events List
            lvEvents = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                BackColor = NeonTheme.Ground,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontBody,
                BorderStyle = BorderStyle.None
            };
            lvEvents.Columns.Add("HEURE", 95);
            lvEvents.Columns.Add("STATUT", 125);
            lvEvents.Columns.Add("CIBLE", 210);
            lvEvents.Columns.Add("TYPE", 110);
            lvEvents.Columns.Add("LATENCE", 90);
            lvEvents.Columns.Add("DÉTAILS / MOTIF DE DÉCISION", 650);

            lvEvents.SelectedIndexChanged += LvEvents_SelectedIndexChanged;

            // Inspector Panel
            Panel pnlInspector = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Field
            };

            Label lblInspHeader = new Label
            {
                Text = "INSPECTEUR // CONTEXTE, MÉMOIRE, PROMPTS ET RÉPONSES DÉTAILLÉS",
                Font = NeonTheme.FontBadge,
                ForeColor = NeonTheme.Rose,
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = NeonTheme.Band,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };

            // Inscription du composant RichTextBox enrichi
            rtbInspector = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = new Font("Consolas", 10F, FontStyle.Regular),
                Padding = new Padding(12),
                WordWrap = true
            };

            pnlInspector.Controls.Add(rtbInspector);
            pnlInspector.Controls.Add(lblInspHeader);

            split.Panel1.Controls.Add(lvEvents);
            split.Panel2.Controls.Add(pnlInspector);

            InitializeSettingsDrawer();
            InitializeHelpDrawer();
            
            InitializeMemoryDrawer();
            InitializePromptDrawer();

            Panel pnlMainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 0, 16, 12),
                BackColor = Color.Transparent
            };
            pnlMainContainer.Controls.Add(split);

            this.Controls.Add(pnlSettingsDrawer);
            this.Controls.Add(pnlHelpDrawer);
            this.Controls.Add(pnlMainContainer);
            this.Controls.Add(pnlExplanation);
            this.Controls.Add(pnlModes);
            this.Controls.Add(pnlTopBar);
            this.Controls.Add(pnlBottom);
            
            this.Controls.Add(pnlMemoryDrawer);
            this.Controls.Add(pnlPromptDrawer);

            LayoutHeaderButtons();
            NeonTheme.Clip(this, 20, 6);
        }

        private void LayoutHeaderButtons()
        {
            if (pnlTopBar == null) return;
            int right = pnlTopBar.ClientSize.Width - 22;

            if (btnClose != null) { btnClose.Location = new Point(right - btnClose.Width, 18); right -= btnClose.Width + 8; }
            if (btnMin != null) { btnMin.Location = new Point(right - btnMin.Width, 18); right -= btnMin.Width + 14; }
            if (btnHelpToggle != null) { btnHelpToggle.Location = new Point(right - btnHelpToggle.Width, 18); right -= btnHelpToggle.Width + 8; }
            if (btnSettingsToggle != null) { btnSettingsToggle.Location = new Point(right - btnSettingsToggle.Width, 18); right -= btnSettingsToggle.Width + 8; }
            if (btnPromptToggle != null) { btnPromptToggle.Location = new Point(right - btnPromptToggle.Width, 18); right -= btnPromptToggle.Width + 8; }
            if (btnMemoryToggle != null) { btnMemoryToggle.Location = new Point(right - btnMemoryToggle.Width, 18); right -= btnMemoryToggle.Width + 14; }
            if (lblStatusChip != null) { lblStatusChip.Location = new Point(right - lblStatusChip.Width, 19); }
        }

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

            cbApiKeys = CreateCheck("Clés API & Tokens secrets", SecurityPolicy.MaskApiKeys, 0);
            cbPasswords = CreateCheck("Mots de passe & Credentials", SecurityPolicy.MaskPasswords, 32);
            cbPhones = CreateCheck("Numéros de téléphone (FR / Intl)", SecurityPolicy.MaskPhoneNumbers, 64);
            cbIps = CreateCheck("Adresses IP (IPv4 & IPv6)", SecurityPolicy.MaskIpAddresses, 96);
            cbAddresses = CreateCheck("Adresses postales & Codes postaux", SecurityPolicy.MaskPostalAddresses, 128);
            cbUserPaths = CreateCheck("Chemins utilisateurs Windows (C:\\Users)", SecurityPolicy.MaskUserPaths, 160);
            cbEmails = CreateCheck("Adresses Email", SecurityPolicy.MaskEmails, 192);
            cbCards = CreateCheck("Numéros de carte bancaire", SecurityPolicy.MaskCreditCards, 224);
            cbNames = CreateCheck("Noms & Prénoms personnalisés", SecurityPolicy.MaskCustomNames, 256);

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

            btnSaveSettings = CreateCyberButton("ENREGISTRER LES PARAMÈTRES", 260, 36, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnSaveSettings.Dock = DockStyle.Bottom;
            btnSaveSettings.Click += (s, e) => SaveSettingsFromUi();

            Button btnCloseSettings = CreateCyberButton("FERMER LE VOLET [X]", 160, 32, NeonTheme.Edge, NeonTheme.Rose);
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

        private CheckBox CreateCheck(string text, bool check, int top)
        {
            return new CheckBox
            {
                Text = text,
                Checked = check,
                Location = new Point(4, top),
                AutoSize = true,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontBody
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

        private void InitializeHelpDrawer()
        {
            pnlHelpDrawer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 460,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            Label lblHelpTitle = new Label
            {
                Text = "GUIDE DES OPTIONS // CAPRICE",
                Font = NeonTheme.FontTitle,
                ForeColor = NeonTheme.Rose,
                Dock = DockStyle.Top,
                Height = 36
            };

            Button btnCloseHelp = CreateCyberButton("FERMER LE GUIDE [X]", 170, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnCloseHelp.Dock = DockStyle.Bottom;
            btnCloseHelp.Click += (s, e) => ToggleHelpDrawer();

            TextBox txtHelpContent = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontSubtitle,
                BorderStyle = BorderStyle.None,
                Text =
@"POLITIQUES DE SÉCURITÉ :

1. MONITOR ONLY (Audit Passif)
------------------------------------
- Écoute et enregistre chaque échange en toute transparence.

2. ENFORCE DLP (Prévention Fuite)
------------------------------------
- Scanne chaque prompt avant envoi au cloud et masque automatiquement les données sensibles.

3. STRICT LOCAL (Verrou Cloud)
------------------------------------
- Coupe tout accès aux IA externes. Seul Ollama local passe.

4. ASK CONFIRMATION (Contrôle Manuel)
------------------------------------
- Demande confirmation avec prévisualisation avant envoi."
            };

            pnlHelpDrawer.Controls.Add(txtHelpContent);
            pnlHelpDrawer.Controls.Add(lblHelpTitle);
            pnlHelpDrawer.Controls.Add(btnCloseHelp);
        }

        private void ToggleHelpDrawer()
        {
            helpDrawerVisible = !helpDrawerVisible;
            if (helpDrawerVisible && settingsDrawerVisible) ToggleSettingsDrawer();
            pnlHelpDrawer.Visible = helpDrawerVisible;
            if (helpDrawerVisible) pnlHelpDrawer.BringToFront();
            btnHelpToggle.Text = helpDrawerVisible ? "[X] FERMER" : "? GUIDE";
        }

        private RadioButton CreateRadio(string text, int x, bool check, PolicyMode mode, string tooltip)
        {
            RadioButton rb = new RadioButton
            {
                Text = text,
                Location = new Point(x, 11),
                AutoSize = true,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontBody,
                Checked = check
            };
            toolTips.SetToolTip(rb, tooltip);

            rb.CheckedChanged += (s, e) =>
            {
                if (rb.Checked)
                {
                    SecurityPolicy.CurrentMode = mode;
                    SecurityPolicy.SaveSettings();
                    UpdateExplanationBanner(mode);
                }
            };

            return rb;
        }

        private void UpdateExplanationBanner(PolicyMode mode)
        {
            switch (mode)
            {
                case PolicyMode.MonitorOnly:
                    lblStatusChip.Text = "AUDIT / MONITOR";
                    lblStatusChip.BackColor = NeonTheme.Cyan;
                    lblExplainTitle.Text = "[MODE ACTIF] 1. MONITOR ONLY :";
                    lblExplainTitle.ForeColor = NeonTheme.Cyan;
                    lblExplainBody.Text = "Audit passif. Tous les prompts, contextes et mémoires envoyés aux modèles sont affichés en toute transparence.";
                    break;
                case PolicyMode.EnforceDlp:
                    lblStatusChip.Text = "DLP ACTIVÉ";
                    lblStatusChip.BackColor = NeonTheme.StatusWarning;
                    lblExplainTitle.Text = "[MODE ACTIF] 2. ENFORCE DLP :";
                    lblExplainTitle.ForeColor = NeonTheme.StatusWarning;
                    lblExplainBody.Text = "Filtrage actif. Les clés API, mots de passe, téléphones, IP, adresses et noms sont masqués avant envoi.";
                    break;
                case PolicyMode.BlockExternal:
                    lblStatusChip.Text = "LOCAL ONLY";
                    lblStatusChip.BackColor = NeonTheme.Rose;
                    lblExplainTitle.Text = "[MODE ACTIF] 3. STRICT LOCAL :";
                    lblExplainTitle.ForeColor = NeonTheme.Rose;
                    lblExplainBody.Text = "Isolation cloud. Tout appel vers OpenAI, Anthropic, DeepSeek, etc. est bloqué net. Seul Ollama local passe.";
                    break;
                case PolicyMode.AskConfirmation:
                    lblStatusChip.Text = "CONFIRMATION ON";
                    lblStatusChip.BackColor = Color.FromArgb(200, 100, 255);
                    lblExplainTitle.Text = "[MODE ACTIF] 4. ASK CONFIRMATION :";
                    lblExplainTitle.ForeColor = Color.FromArgb(200, 100, 255);
                    lblExplainBody.Text = "Validation humaine. Une fenêtre interactive vous demande confirmation avant tout envoi de prompt vers le cloud.";
                    break;
            }
        }

        private Button CreateCyberButton(string text, int width, int height, Color borderColor, Color textColor)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(width, height),
                Font = NeonTheme.FontBadge,
                ForeColor = textColor,
                BackColor = NeonTheme.Field,
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderColor = borderColor;
            btn.FlatAppearance.MouseOverBackColor = NeonTheme.Hover;
            btn.FlatAppearance.MouseDownBackColor = NeonTheme.Pressed;
            return btn;
        }

        private void OnDragWindow(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Application.Exit();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            NeonTheme.Backdrop(e.Graphics, this.Size, 140);
            NeonTheme.Frame(e.Graphics, this.Size, 20, 6, 28, 28);
        }

        // --- NOUVEAU RENDU VISUEL D'INSPECTION (RICH TEXT BOX) ---
        private void LvEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvEvents.SelectedIndices.Count == 0) return;
            int idx = lvEvents.SelectedIndices[0];
            if (idx >= 0 && idx < eventsList.Count)
            {
                InterceptEvent ev = eventsList[idx];

                rtbInspector.Clear();
                rtbInspector.SuspendLayout();

                // 1. En-tête de synthèse / Badges
                Color statusColor = ev.Status == InterceptStatus.Blocked ? NeonTheme.Rose :
                                   (ev.Status == InterceptStatus.DlpMasked ? NeonTheme.StatusWarning : NeonTheme.Cyan);

                AppendStyledText($"EVENT #{ev.Id}", new Font("Consolas", 12F, FontStyle.Bold), NeonTheme.Rose);
                AppendStyledText($"  //  {ev.Timestamp:yyyy-MM-dd HH:mm:ss}  //  ", new Font("Consolas", 10F), NeonTheme.Faint);
                AppendStyledText($"[{ev.Status.ToString().ToUpper()}]\n", new Font("Consolas", 11F, FontStyle.Bold), statusColor);

                AppendStyledText($"CIBLE   : ", new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.Faint);
                AppendStyledText($"{ev.TargetName} ", new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.White);
                AppendStyledText($"({(ev.IsExternal ? "CLOUD EXTERNE" : "LOCAL LOOPBACK")})\n", new Font("Consolas", 9F), ev.IsExternal ? Color.Orange : NeonTheme.Cyan);

                AppendStyledText($"URL     : ", new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.Faint);
                AppendStyledText($"{ev.Url}\n", new Font("Consolas", 9F), NeonTheme.White);

                AppendStyledText($"MOTIF   : ", new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.Faint);
                AppendStyledText($"{ev.Reason} ", new Font("Consolas", 9.5F), NeonTheme.White);
                if (ev.LatencyMs >= 0)
                {
                    AppendStyledText($" (Latence: {ev.LatencyMs} ms)\n", new Font("Consolas", 9F), NeonTheme.Faint);
                }
                else
                {
                    AppendStyledText("\n", new Font("Consolas", 9F), NeonTheme.Faint);
                }

                if (ev.HistoryTurns > 0)
                {
                    AppendStyledText($"CONTEXTE: ", new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.Faint);
                    AppendStyledText($"{ev.HistoryTurns} tour(s) d'historique actif(s)\n", new Font("Consolas", 9.5F), NeonTheme.Cyan);
                }

                // Masquages DLP / Alertes
                if (ev.DlpMatches != null && ev.DlpMatches.Count > 0)
                {
                    AppendStyledText("\n[!] DÉTECTIONS DLP (SENSITIVE DATA):\n", new Font("Consolas", 10F, FontStyle.Bold), NeonTheme.StatusWarning);
                    foreach (var match in ev.DlpMatches)
                    {
                        AppendStyledText($"    • {match}\n", new Font("Consolas", 9.5F), NeonTheme.StatusWarning);
                    }
                }

                // 2. Sections de données
                if (!string.IsNullOrEmpty(ev.RequestPayload))
                {
                    RenderSectionHeader("1. PROMPT UTILISATEUR ACTUEL ENVOYÉ", NeonTheme.Cyan);
                    AppendContentBlock(ev.RequestPayload);
                }

                if (!string.IsNullOrEmpty(ev.ContextMemory))
                {
                    RenderSectionHeader("2. CONTEXTE ET MÉMOIRE UTILISÉE (me.md & HISTORIQUE)", Color.MediumSpringGreen);
                    AppendContentBlock(ev.ContextMemory);
                }

                if (!string.IsNullOrEmpty(ev.ResponsePayload))
                {
                    RenderSectionHeader("3. RÉPONSE DU MODÈLE", Color.Violet);
                    AppendContentBlock(ev.ResponsePayload);
                }

                rtbInspector.SelectionStart = 0;
                rtbInspector.ScrollToCaret();
                rtbInspector.ResumeLayout();
            }
        }

        private void RenderSectionHeader(string title, Color headerColor)
        {
            rtbInspector.AppendText("\n");
            rtbInspector.SelectionColor = headerColor;
            rtbInspector.SelectionFont = new Font("Consolas", 10F, FontStyle.Bold);
            rtbInspector.AppendText($"─── {title} ───────────────────────────────────────\n");
        }

        private void AppendContentBlock(string content)
        {
            rtbInspector.SelectionFont = new Font("Consolas", 9.5F, FontStyle.Regular);
            rtbInspector.SelectionColor = NeonTheme.White;
            
            // Formatage de sécurité pour faire ressortir les éléments masqués [REDACTED]
            if (content.Contains("[REDACTED"))
            {
                string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    if (line.Contains("[REDACTED"))
                    {
                        int index = 0;
                        while (index < line.Length)
                        {
                            int rIndex = line.IndexOf("[REDACTED", index, StringComparison.Ordinal);
                            if (rIndex == -1)
                            {
                                AppendStyledText(line.Substring(index) + "\n", rtbInspector.SelectionFont, NeonTheme.White);
                                break;
                            }

                            if (rIndex > index)
                            {
                                AppendStyledText(line.Substring(index, rIndex - index), rtbInspector.SelectionFont, NeonTheme.White);
                            }

                            int closeIndex = line.IndexOf("]", rIndex, StringComparison.Ordinal);
                            if (closeIndex != -1)
                            {
                                string redactedTag = line.Substring(rIndex, closeIndex - rIndex + 1);
                                AppendStyledText(redactedTag, new Font("Consolas", 9.5F, FontStyle.Bold), NeonTheme.StatusWarning);
                                index = closeIndex + 1;
                            }
                            else
                            {
                                AppendStyledText(line.Substring(rIndex) + "\n", rtbInspector.SelectionFont, NeonTheme.StatusWarning);
                                break;
                            }
                        }
                    }
                    else
                    {
                        AppendStyledText(line + "\n", rtbInspector.SelectionFont, NeonTheme.White);
                    }
                }
            }
            else
            {
                rtbInspector.AppendText(content + "\n");
            }
        }

        private void AppendStyledText(string text, Font font, Color color)
        {
            rtbInspector.SelectionStart = rtbInspector.TextLength;
            rtbInspector.SelectionLength = 0;
            rtbInspector.SelectionFont = font;
            rtbInspector.SelectionColor = color;
            rtbInspector.AppendText(text);
        }

        public static void NotifyEvent(InterceptEvent ev)
        {
            if (Instance == null || Instance.IsDisposed) return;

            if (Instance.InvokeRequired)
            {
                Instance.BeginInvoke(new Action<InterceptEvent>(NotifyEvent), ev);
                return;
            }

            Instance.AddEventInternal(ev);
        }

        private void AddEventInternal(InterceptEvent ev)
        {
            eventsList.Insert(0, ev);
            totalCount++;
            if (ev.Status == InterceptStatus.Blocked) blockedCount++;
            else allowedCount++;

            if (ev.DlpMatches != null && ev.DlpMatches.Count > 0) dlpCount++;

            lblStats.Text = string.Format("REQ: {0}  |  ALLOWED: {1}  |  BLOCKED: {2}  |  DLP DETECTED: {3}",
                totalCount, allowedCount, blockedCount, dlpCount);

            ListViewItem item = new ListViewItem(ev.Timestamp.ToString("HH:mm:ss"));
            item.SubItems.Add(ev.Status.ToString());
            item.SubItems.Add(ev.TargetName);
            item.SubItems.Add(ev.Method);
            item.SubItems.Add(ev.LatencyMs >= 0 ? ev.LatencyMs + " ms" : "-");
            item.SubItems.Add(ev.Reason);

            if (ev.Status == InterceptStatus.Blocked)
            {
                item.ForeColor = NeonTheme.StatusBlocked;
            }
            else if (ev.Status == InterceptStatus.DlpMasked)
            {
                item.ForeColor = NeonTheme.StatusWarning;
            }
            else
            {
                item.ForeColor = ev.IsExternal ? NeonTheme.White : NeonTheme.Cyan;
            }

            lvEvents.Items.Insert(0, item);

            if (lvEvents.Items.Count > 300)
            {
                lvEvents.Items.RemoveAt(lvEvents.Items.Count - 1);
                eventsList.RemoveAt(eventsList.Count - 1);
            }
        }

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

            Button btnSave = CreateCyberButton("SAUVEGARDER LA MÉMOIRE", 240, 36, NeonTheme.DeepEdge, NeonTheme.Cyan);
            btnSave.Dock = DockStyle.Bottom;
            btnSave.Click += (s, e) =>
            {
                MemoryManager.SaveMemory(txtMemoryEditor.Text);
                MessageBox.Show("Mémoire sauvegardée avec succès !", "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ToggleMemoryDrawer();
            };

            Button btnClose = CreateCyberButton("FERMER [X]", 120, 32, NeonTheme.Edge, NeonTheme.Rose);
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
            Button btnFetchOriginal = CreateCyberButton("RÉCUPÉRER PROMPT D'ORIGINE", 230, 34, NeonTheme.Edge, NeonTheme.Cyan);
            btnFetchOriginal.Dock = DockStyle.Bottom;
            btnFetchOriginal.Click += (s, e) =>
            {
                txtPromptEditor.Text = GetOriginalCapriPrompt();
            };

            Button btnSave = CreateCyberButton("APPLIQUER LE HOOK PROMPT", 250, 36, NeonTheme.DeepEdge, NeonTheme.Rose);
            btnSave.Dock = DockStyle.Bottom;
            btnSave.Click += (s, e) =>
            {
                string prompt = txtPromptEditor.Text.Trim();
                CaprICEHook.CustomSystemPrompt = string.IsNullOrEmpty(prompt) ? null : prompt;
                MessageBox.Show("Nouveau System Prompt injecté via Harmony !", "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                TogglePromptDrawer();
            };

            Button btnClose = CreateCyberButton("FERMER [X]", 120, 32, NeonTheme.Edge, NeonTheme.Rose);
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
                    txtPromptEditor.Text = GetOriginalCapriPrompt();
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

        public static bool PromptConfirmation(string title, string content, List<string> warnings)
        {
            if (Instance == null || Instance.IsDisposed) return true;

            bool approved = false;
            Instance.Invoke((Action)delegate
            {
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

                    dlg.MouseDown += (s, e) =>
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            ReleaseCapture();
                            SendMessage(dlg.Handle, 0xA1, 0x2, 0);
                        }
                    };

                    Label lblWarn = new Label
                    {
                        Text = "TRANSMISSION IA EXTERNE // CONFIRMATION REQUISE",
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
                        Text = "Cible : Cloud AI Externe\r\n\r\n--- CONTENU DU PROMPT & CONTEXTE ---\r\n" + content
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

                    approved = (dlg.ShowDialog(Instance) == DialogResult.OK);
                }
            });

            return approved;
        }

        private string GetOriginalCapriPrompt()
        {
            try
            {
                Type brainType = null;

                if (CaprICEHook.CapriAssembly != null)
                {
                    brainType = CaprICEHook.CapriAssembly.GetType("CapriDesktop.Brain");
                }
                else
                {
                    brainType = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(a => a.GetTypes())
                                .FirstOrDefault(t => t.FullName == "CapriDesktop.Brain");
                }

                if (brainType != null)
                {
                    PropertyInfo personaProp = brainType.GetProperty("Persona", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                    if (personaProp != null)
                    {
                        // Désactive temporairement l'override pour lire la vraie valeur d'origine
                        string currentCustom = CaprICEHook.CustomSystemPrompt;
                        CaprICEHook.CustomSystemPrompt = null;

                        string originalPersona = personaProp.GetValue(null) as string;

                        // Restaure la valeur personnalisée
                        CaprICEHook.CustomSystemPrompt = currentCustom;

                        if (!string.IsNullOrEmpty(originalPersona))
                        {
                            // Formatage WinForms : Conversion des sauts de ligne Linux (\n) en Windows (\r\n)
                            originalPersona = originalPersona.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
                            return originalPersona;
                        }
                    }
                }
            }
            catch { }

            return "(Impossible de lire Brain.Persona depuis Capri)";
        }
    }
}