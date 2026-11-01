using System;
using System.Drawing;
using System.Windows.Forms;
using CaprICE.Hooks;
using CaprICE.Services;

namespace CaprICE.UI
{
    public partial class CaprICEForm
    {
        private Panel pnlChatDrawer;
        private Panel pnlMessageList;
        private SavedChatData currentChatData;
        private bool chatDrawerVisible = false;

        private void InitializeChatDrawer()
        {
            pnlChatDrawer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 560,
                BackColor = NeonTheme.Well,
                Visible = false,
                Padding = new Padding(20)
            };

            Label lblTitle = new Label
            {
                Text = "GESTIONNAIRE DE MÉMOIRE (chat.json)",
                Font = NeonTheme.FontTitle,
                ForeColor = NeonTheme.Yellow,
                Dock = DockStyle.Top,
                Height = 36
            };

            Label lblSub = new Label
            {
                Text = "Supprimez sélectivement les messages pour altérer la mémoire vivante de Capri.",
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Faint,
                Dock = DockStyle.Top,
                Height = 24
            };

            pnlMessageList = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = NeonTheme.Well,
                Padding = new Padding(0, 10, 10, 10)
            };

            Button btnClearAll = CyberControls.Button("PURGER TOUTE LA MÉMOIRE", 240, 36, NeonTheme.Edge, NeonTheme.Rose);
            btnClearAll.Dock = DockStyle.Bottom;
            btnClearAll.Click += (s, e) =>
            {
                if (MessageBox.Show("Voulez-vous vraiment effacer tout l'historique ?", "CaprICE Memory Guard", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    currentChatData.Messages.Clear();
                    if (currentChatData.Seen != null) currentChatData.Seen.Clear();
                    SaveAndRefresh();
                }
            };

            Button btnClose = CyberControls.Button("FERMER [X]", 120, 32, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Dock = DockStyle.Bottom;
            btnClose.Click += (s, e) => ToggleChatDrawer();

            pnlChatDrawer.Controls.Add(pnlMessageList);
            pnlChatDrawer.Controls.Add(lblSub);
            pnlChatDrawer.Controls.Add(lblTitle);
            pnlChatDrawer.Controls.Add(btnClearAll);
            pnlChatDrawer.Controls.Add(btnClose);

            this.Controls.Add(pnlChatDrawer);
        }

        private void RenderMessageCards()
        {
            pnlMessageList.Controls.Clear();
            if (currentChatData == null) return;

            pnlMessageList.SuspendLayout();

            int topOffset = 0;

            // Rendu des messages de conversation (MESSAGES)
            if (currentChatData.Messages != null)
            {
                for (int i = 0; i < currentChatData.Messages.Count; i++)
                {
                    int index = i;
                    var msg = currentChatData.Messages[i];

                    Panel card = CreateMessageCard(
                        role: msg.Role?.ToUpper() ?? "MSG",
                        content: msg.Content,
                        headerColor: msg.Role == "user" ? NeonTheme.Cyan : NeonTheme.Rose,
                        onDelete: () =>
                        {
                            currentChatData.Messages.RemoveAt(index);
                            SaveAndRefresh();
                        }
                    );

                    card.Top = topOffset;
                    pnlMessageList.Controls.Add(card);
                    topOffset += card.Height + 10;
                }
            }

            // Rendu des captures visuelles (SEEN)
            if (currentChatData.Seen != null && currentChatData.Seen.Count > 0)
            {
                Label lblSeenHeader = new Label
                {
                    Text = "--- CAPTURES D'ÉCRAN / MÉMOIRE VISUELLE ---",
                    Font = NeonTheme.FontSmall,
                    ForeColor = NeonTheme.Yellow,
                    Top = topOffset + 10,
                    Height = 24,
                    Width = pnlMessageList.Width - 25
                };
                pnlMessageList.Controls.Add(lblSeenHeader);
                topOffset += 34;

                for (int i = 0; i < currentChatData.Seen.Count; i++)
                {
                    int index = i;
                    var sighting = currentChatData.Seen[i];

                    string displayText = "[VU] " + sighting.Saw + (string.IsNullOrEmpty(sighting.Said) ? "" : "\r\n[RÉPONSE] " + sighting.Said);

                    Panel card = CreateMessageCard(
                        role: "SIGHTING",
                        content: displayText,
                        headerColor: NeonTheme.Yellow,
                        onDelete: () =>
                        {
                            currentChatData.Seen.RemoveAt(index);
                            SaveAndRefresh();
                        }
                    );

                    card.Top = topOffset;
                    pnlMessageList.Controls.Add(card);
                    topOffset += card.Height + 10;
                }
            }

            pnlMessageList.ResumeLayout();
        }

        private Panel CreateMessageCard(string role, string content, Color headerColor, Action onDelete)
        {
            Panel card = new Panel
            {
                Width = pnlMessageList.Width - 25,
                BackColor = NeonTheme.Field,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.None
            };

            Label lblRole = new Label
            {
                Text = role,
                Font = NeonTheme.FontTitle,
                ForeColor = headerColor,
                Dock = DockStyle.Top,
                Height = 22
            };

            TextBox txtContent = new TextBox
            {
                Text = content,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = new Font("Consolas", 9.5F, FontStyle.Regular),
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill
            };

            Button btnDelete = CyberControls.Button("SUPPRIMER [X]", 130, 26, NeonTheme.Edge, NeonTheme.Rose);
            btnDelete.Dock = DockStyle.Bottom;
            btnDelete.Click += (s, e) => onDelete();

            // Calcul dynamique de la hauteur en fonction du texte
            int textHeight = Math.Max(40, Math.Min(120, TextRenderer.MeasureText(content, txtContent.Font, new Size(card.Width - 20, 0), TextFormatFlags.WordBreak).Height + 10));
            card.Height = textHeight + 60;

            card.Controls.Add(txtContent);
            card.Controls.Add(lblRole);
            card.Controls.Add(btnDelete);

            return card;
        }

        private void SaveAndRefresh()
        {
            if (ChatHistoryManager.SaveData(currentChatData, out string err))
            {
                CaprICEHook.ClearBrainHistory(); // Purge du cache mémoire
                RenderMessageCards();
            }
            else
            {
                MessageBox.Show("Erreur de sauvegarde : " + err, "CaprICE Memory", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToggleChatDrawer()
        {
            chatDrawerVisible = !chatDrawerVisible;
            if (chatDrawerVisible)
            {
                if (settingsDrawerVisible) ToggleSettingsDrawer();
                if (helpDrawerVisible) ToggleHelpDrawer();
                if (memoryDrawerVisible) ToggleMemoryDrawer();
                if (promptDrawerVisible) TogglePromptDrawer();

                currentChatData = ChatHistoryManager.LoadData();
                RenderMessageCards();

                pnlChatDrawer.BringToFront();
            }
            pnlChatDrawer.Visible = chatDrawerVisible;
        }
    }
}