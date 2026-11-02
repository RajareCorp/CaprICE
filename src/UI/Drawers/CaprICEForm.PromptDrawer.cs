using System.Drawing;
using System.Windows.Forms;
using CaprICE.Hooks;
using CaprICE.Security;
using CaprICE.Services;

namespace CaprICE.UI
{
    public partial class CaprICEForm
    {
        private RichTextBox txtPersonality;
        private RichTextBox txtSafety;
        private RichTextBox txtMachinery;

        private void InitializePromptDrawer()
        {
            pnlPromptDrawer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            Panel pnlContentScroll = new Panel
            {
                AutoScroll = true,
                BackColor = NeonTheme.Well
            };

            TableLayoutPanel content = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 6,
                Height = 588,
                BackColor = NeonTheme.Well,
                Location = Point.Empty
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 190F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 145F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 145F));

            Label lblPersonality = CreateSectionLabel("1. PERSONALITY (Character, Tone, Lore)");
            txtPersonality = CreateCodeTextBox(NeonTheme.Rose);
            Label lblSafety = CreateSectionLabel("2. SAFETY (Security, Limits, Behavior)");
            txtSafety = CreateCodeTextBox(NeonTheme.StatusWarning);
            Label lblMachinery = CreateSectionLabel("3. MACHINERY (Format & Voice Constraints)");
            txtMachinery = CreateCodeTextBox(NeonTheme.Cyan);

            content.Controls.Add(lblPersonality, 0, 0);
            content.Controls.Add(txtPersonality, 0, 1);
            content.Controls.Add(lblSafety, 0, 2);
            content.Controls.Add(txtSafety, 0, 3);
            content.Controls.Add(lblMachinery, 0, 4);
            content.Controls.Add(txtMachinery, 0, 5);
            pnlContentScroll.Controls.Add(content);
            pnlContentScroll.SizeChanged += (s, e) => content.Width = pnlContentScroll.ClientSize.Width;

            Button btnFetchOriginal = CyberControls.Button("LOAD ORIGINAL PROMPTS", 240, 34, NeonTheme.Edge, NeonTheme.Cyan);
            btnFetchOriginal.Click += (s, e) => LoadOriginalsIntoFields();

            Button btnSave = CyberControls.Button("APPLY PROMPT HOOKS", 250, 36, NeonTheme.DeepEdge, NeonTheme.Rose);
            btnSave.Click += (s, e) =>
            {
                string previousPersonality = CaprICEHook.CustomPersonality;
                string previousSafety = CaprICEHook.CustomSafety;
                string previousMachinery = CaprICEHook.CustomMachinery;
                CaprICEHook.CustomPersonality = string.IsNullOrWhiteSpace(txtPersonality.Text) ? null : txtPersonality.Text;
                CaprICEHook.CustomSafety = string.IsNullOrWhiteSpace(txtSafety.Text) ? null : txtSafety.Text;
                CaprICEHook.CustomMachinery = string.IsNullOrWhiteSpace(txtMachinery.Text) ? null : txtMachinery.Text;

                if (!SecurityPolicy.SaveSettings(out string error))
                {
                    CaprICEHook.CustomPersonality = previousPersonality;
                    CaprICEHook.CustomSafety = previousSafety;
                    CaprICEHook.CustomMachinery = previousMachinery;
                    MessageBox.Show("The prompt could not be saved: " + error,
                        "CaprICE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool historyCleared = CaprICEHook.ClearBrainHistory();
                MessageBox.Show(historyCleared
                        ? "Custom prompt saved and applied."
                        : "Prompt saved and applied, but Capri's active history could not be cleared.",
                    "CaprICE", MessageBoxButtons.OK,
                    historyCleared ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);
            };

            Button btnClose = CyberControls.Button("CLOSE", 120, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Click += (s, e) => SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);

            pnlPromptDrawer.Controls.Add(BuildDrawerLayout(
                "HOOK SYSTEM PROMPT (MODULAIRE)", NeonTheme.Rose, pnlContentScroll,
                btnFetchOriginal, btnSave, btnClose));
        }

        private Label CreateSectionLabel(string title)
        {
            return new Label
            {
                Text = title,
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Cyan,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };
        }

        private RichTextBox CreateCodeTextBox(Color accent)
        {
            ReadableTextEditor editor = new ReadableTextEditor(accent) { Dock = DockStyle.Fill };
            return editor;
        }

        private void LoadOriginalsIntoFields()
        {
            txtPersonality.Text = PersonaReader.GetDefaultPersonality();
            txtSafety.Text = PersonaReader.GetSafety();
            txtMachinery.Text = PersonaReader.GetMachinery();
        }

        private void ShowPromptDrawer()
        {
            txtPersonality.Text = CaprICEHook.CustomPersonality ?? PersonaReader.GetDefaultPersonality();
            txtSafety.Text = CaprICEHook.CustomSafety ?? PersonaReader.GetSafety();
            txtMachinery.Text = CaprICEHook.CustomMachinery ?? PersonaReader.GetMachinery();
            SwitchWorkspaceView(pnlPromptDrawer, btnNavPrompts);
        }
    }
}
