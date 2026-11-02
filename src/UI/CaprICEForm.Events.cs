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
        private InspectorRenderer inspector;

        private void LvEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvEvents.SelectedIndices.Count == 0) return;
            int idx = lvEvents.SelectedIndices[0];
            if (idx >= 0 && idx < eventsList.Count)
            {
                inspector.Render(eventsList[idx]);
            }
        }

        private Panel BuildMainView()
        {
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
            lvEvents.Columns.Add("TIME", 95);
            lvEvents.Columns.Add("STATUS", 125);
            lvEvents.Columns.Add("TARGET", 210);
            lvEvents.Columns.Add("TYPE", 110);
            lvEvents.Columns.Add("LATENCY", 90);
            lvEvents.Columns.Add("DETAILS / DECISION REASON", 650);

            lvEvents.SelectedIndexChanged += LvEvents_SelectedIndexChanged;

            // Inspector Panel
            Panel pnlInspector = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Field
            };

            Label lblInspHeader = new Label
            {
                Text = "INSPECTOR // DETAILED CONTEXT, MEMORY, PROMPTS & RESPONSES",
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

            inspector = new InspectorRenderer(rtbInspector);
            pnlInspector.Controls.Add(rtbInspector);
            pnlInspector.Controls.Add(lblInspHeader);

            split.Panel1.Controls.Add(lvEvents);
            split.Panel2.Controls.Add(pnlInspector);

            Panel pnlMainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 0, 16, 12),
                BackColor = Color.Transparent
            };
            pnlMainContainer.Controls.Add(split);
            return pnlMainContainer;
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
    }
}
