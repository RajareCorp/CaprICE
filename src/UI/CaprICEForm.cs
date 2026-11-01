using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CaprICE.Hooks;
using CaprICE.Security;
using CaprICE.Services;
using CaprICE.Startup;

namespace CaprICE.UI
{
    /// <summary>
    /// Fenêtre principale « Mission Control ».
    /// Convertie en Single-Page Shell avec navigation par Sidebar et transitions Glitch.
    /// </summary>
    public partial class CaprICEForm : Form
    {
        private static CaprICEForm Instance;

        // Custom Cyber Window Controls
        private Panel pnlTopBar;
        private Button btnClose;
        private Button btnMin;

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

        // Help Drawer Panel
        private Panel pnlHelpDrawer;

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

        // Main View / Inspector Controls
        private ListView lvEvents;
        private RichTextBox rtbInspector;
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

        // Drawers
        private Panel pnlMemoryDrawer;
        private RichTextBox txtMemoryEditor;

        private Panel pnlPromptDrawer;

        // Nouveaux composants pour le Single-Page Workspace avec Glitch
        private Panel pnlSidebar;
        private Panel pnlWorkspace;
        private Panel pnlSecurityView;

        private CyberNavButton btnNavChat;
        private CyberNavButton btnNavPrompts;
        private CyberNavButton btnNavMemory;
        private CyberNavButton btnNavSecurity;
        private CyberNavButton btnNavSettings;
        private CyberNavButton btnNavHelp;
        private Control activeView;

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
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            toolTips = new ToolTip
            {
                BackColor = NeonTheme.Well,
                ForeColor = NeonTheme.Cyan
            };

            // Construction des composants existants (via partial classes)
            BuildTopBar();
            Panel pnlModes = BuildPolicyBar();
            BuildExplanationBanner();
            Panel pnlBottom = BuildBottomBar();

            // Initialisation des panneaux/tiroirs
            InitializeSettingsDrawer();
            InitializeHelpDrawer();
            InitializeMemoryDrawer();
            InitializePromptDrawer();
            InitializeChatDrawer();

            pnlSecurityView = BuildMainView();

            // Construction du nouvel agencement (Sidebar + Workspace)
            BuildSidebarNav();
            BuildWorkspaceContainer();

            // Layout global : TopBar, Policy Bar, Sidebar, Workspace, Bottom
            Panel pnlHeaderContainer = new Panel { Dock = DockStyle.Top, AutoSize = true };
            pnlHeaderContainer.Controls.Add(pnlExplanation);
            pnlHeaderContainer.Controls.Add(pnlModes);
            pnlHeaderContainer.Controls.Add(pnlTopBar);

            this.Controls.Add(pnlWorkspace);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlHeaderContainer);

            // Vue par défaut : Chat
            ShowChatDrawer();

            LayoutHeaderButtons();
            NeonTheme.Clip(this, 20, 6);
        }

        private void BuildSidebarNav()
        {
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = NeonTheme.Band,
                Padding = new Padding(0, 10, 0, 10)
            };

            btnNavChat = new CyberNavButton("💬  CHAT & AI") { IsActive = true };
            btnNavPrompts = new CyberNavButton("📝  PROMPTS");
            btnNavMemory = new CyberNavButton("🧠  MEMORY");
            btnNavSecurity = new CyberNavButton("🛡️  SECURITY");
            btnNavSettings = new CyberNavButton("⚙️  SETTINGS");
            btnNavHelp = new CyberNavButton("❔  GUIDE");

            btnNavChat.Click += (s, e) => ShowChatDrawer();
            btnNavPrompts.Click += (s, e) => ShowPromptDrawer();
            btnNavMemory.Click += (s, e) => ShowMemoryDrawer();
            btnNavSecurity.Click += (s, e) => SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);
            btnNavSettings.Click += (s, e) => SwitchWorkspaceView(pnlSettingsDrawer, btnNavSettings);
            btnNavHelp.Click += (s, e) => SwitchWorkspaceView(pnlHelpDrawer, btnNavHelp);

            pnlSidebar.Controls.Add(btnNavHelp);
            pnlSidebar.Controls.Add(btnNavSettings);
            pnlSidebar.Controls.Add(btnNavSecurity);
            pnlSidebar.Controls.Add(btnNavMemory);
            pnlSidebar.Controls.Add(btnNavPrompts);
            pnlSidebar.Controls.Add(btnNavChat);
        }

        private void BuildWorkspaceContainer()
        {
            pnlWorkspace = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Ground,
                Padding = new Padding(5)
            };
        }

        private Control BuildDrawerLayout(string title, Color titleColor, Control content, params Control[] actions)
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = NeonTheme.Well,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, actions.Length * 42F));

            Label header = new Label
            {
                Text = title,
                Font = NeonTheme.FontTitle,
                ForeColor = titleColor,
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            TableLayoutPanel footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = actions.Length,
                BackColor = NeonTheme.Well,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < actions.Length; i++)
            {
                footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
                actions[i].Dock = DockStyle.Fill;
                actions[i].Margin = new Padding(0, 3, 0, 3);
                footer.Controls.Add(actions[i], 0, i);
            }

            content.Dock = DockStyle.Fill;
            content.Margin = Padding.Empty;
            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(content, 0, 1);
            layout.Controls.Add(footer, 0, 2);
            return layout;
        }

        /// <summary>
        /// Permute la vue active dans le Workspace en appliquant un effet de Glitch RGB.
        /// </summary>
        private void SwitchWorkspaceView(Control targetView, CyberNavButton targetButton)
        {
            if (targetView == null || activeView == targetView) return;

            GlitchOverlay previousGlitch = null;
            foreach (Control control in pnlWorkspace.Controls)
            {
                if (control is GlitchOverlay)
                {
                    previousGlitch = (GlitchOverlay)control;
                    break;
                }
            }
            if (previousGlitch != null)
            {
                pnlWorkspace.Controls.Remove(previousGlitch);
                previousGlitch.Dispose();
            }

            if (activeView != null)
            {
                GlitchOverlay glitch = new GlitchOverlay(pnlWorkspace);
                pnlWorkspace.Controls.Remove(activeView);

                foreach (Control ctrl in pnlSidebar.Controls)
                {
                    if (ctrl is CyberNavButton navBtn)
                        navBtn.IsActive = (navBtn == targetButton);
                }

                targetView.Visible = true;
                targetView.Dock = DockStyle.Fill;
                pnlWorkspace.Controls.Add(targetView);
                activeView = targetView;

                glitch.Dock = DockStyle.Fill;
                pnlWorkspace.Controls.Add(glitch);
                glitch.BringToFront();
                return;
            }

            foreach (Control ctrl in pnlSidebar.Controls)
            {
                if (ctrl is CyberNavButton navBtn)
                    navBtn.IsActive = (navBtn == targetButton);
            }

            targetView.Visible = true;
            targetView.Dock = DockStyle.Fill;
            pnlWorkspace.Controls.Add(targetView);
            activeView = targetView;
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

        public static bool PromptConfirmation(string title, string content, List<string> warnings)
        {
            if (Instance == null || Instance.IsDisposed || !Instance.IsHandleCreated) return false;

            bool approved = false;
            Instance.Invoke((Action)delegate
            {
                approved = ConfirmationDialog.Show(Instance, content);
            });

            return approved;
        }

        /// <summary>
        /// Arrête proprement Capri et quitte l'application.
        /// </summary>
        private void ShutdownAndExit()
        {
            try
            {
                CapriLauncher.Stop();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erreur lors de l'arrêt de Capri : " + ex.Message);
            }
            finally
            {
                Application.Exit();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            CapriLauncher.Stop();
        }

    }
}