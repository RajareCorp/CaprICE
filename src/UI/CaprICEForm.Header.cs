using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CaprICE.Security;

namespace CaprICE.UI
{
    public partial class CaprICEForm
    {
        private void BuildTopBar()
        {
            // Barre de titre Cyber
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

            // Boutons Système de la fenêtre
            btnMin = CyberControls.Button("—", 42, 30, NeonTheme.Edge, NeonTheme.White);
            btnMin.Click += (s, e) => { this.WindowState = FormWindowState.Minimized; };

            btnClose = CyberControls.Button("✕ CLOSE", 95, 30, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Click += (s, e) => { ShutdownAndExit(); };

            pnlTopBar.Controls.Add(lblTitle);
            pnlTopBar.Controls.Add(lblSubtitle);
            pnlTopBar.Controls.Add(lblStatusChip);
            pnlTopBar.Controls.Add(lblStats);
            pnlTopBar.Controls.Add(btnMin);
            pnlTopBar.Controls.Add(btnClose);

            pnlTopBar.Resize += (s, e) => LayoutHeaderButtons();
            this.Resize += (s, e) => { LayoutHeaderButtons(); NeonTheme.Clip(this, 20, 6); };
        }

        private void LayoutHeaderButtons()
        {
            if (pnlTopBar == null) return;
            int right = pnlTopBar.ClientSize.Width - 22;

            if (btnClose != null) { btnClose.Location = new Point(right - btnClose.Width, 18); right -= btnClose.Width + 8; }
            if (btnMin != null) { btnMin.Location = new Point(right - btnMin.Width, 18); right -= btnMin.Width + 14; }
            if (lblStatusChip != null) { lblStatusChip.Location = new Point(right - lblStatusChip.Width, 19); }
        }
    }
}