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
    /// <summary>
    /// Fenêtre principale « Mission Control ». Classe partielle, répartie par zone :
    /// Header, Policy, Footer, Events, Inspector et un fichier par tiroir (Drawers/).
    /// </summary>
    public partial class CaprICEForm : Form
    {
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

            BuildTopBar();
            Panel pnlModes = BuildPolicyBar();
            BuildExplanationBanner();
            Panel pnlBottom = BuildBottomBar();
            Panel pnlMainContainer = BuildMainView();

            InitializeSettingsDrawer();
            InitializeHelpDrawer();
            InitializeMemoryDrawer();
            InitializePromptDrawer();

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

        private void OnDragWindow(object sender, MouseEventArgs e)
        {
            NativeMethods.DragWindow(this, e);
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

        /// <summary>Demande de confirmation manuelle (appelée depuis un thread de Capri).</summary>
        public static bool PromptConfirmation(string title, string content, List<string> warnings)
        {
            if (Instance == null || Instance.IsDisposed) return true;

            bool approved = false;
            Instance.Invoke((Action)delegate
            {
                approved = ConfirmationDialog.Show(Instance, content);
            });

            return approved;
        }
    }
}
