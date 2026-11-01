using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private List<TranscriptData> currentTranscripts = new List<TranscriptData>();

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
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

            TableLayoutPanel header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = NeonTheme.Well,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104F));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

            Label lblTitle = new Label
            {
                Text = "CHAT & TRANSCRIPTIONS",
                Font = NeonTheme.FontTitle,
                ForeColor = NeonTheme.Yellow,
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblSub = new Label
            {
                Text = "Historique, captures et journaux locaux de Capri.",
                Font = NeonTheme.FontSmall,
                ForeColor = NeonTheme.Faint,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            pnlMessageList = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = NeonTheme.Well,
                Padding = new Padding(0, 10, 10, 10),
                Margin = Padding.Empty
            };
            pnlMessageList.SizeChanged += (s, e) => RenderMessageCards();

            Button btnClose = CyberControls.Button("FERMER", 96, 30, NeonTheme.Edge, NeonTheme.Rose);
            btnClose.Dock = DockStyle.Fill;
            btnClose.Margin = new Padding(4, 2, 0, 2);
            btnClose.Click += (s, e) => SwitchWorkspaceView(pnlSecurityView, btnNavSecurity);

            TableLayoutPanel footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = NeonTheme.Well,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Button btnClearAll = CyberControls.Button("EFFACER LE CHAT", 240, 38, NeonTheme.Edge, NeonTheme.Rose);
            btnClearAll.Dock = DockStyle.Fill;
            btnClearAll.Margin = new Padding(0, 4, 4, 4);
            btnClearAll.Click += (s, e) =>
            {
                if (MessageBox.Show("Effacer tout l'historique de chat et les captures visuelles ?",
                    "CaprICE // Chat", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    currentChatData = ChatHistoryManager.LoadData();
                    currentChatData.Messages.Clear();
                    currentChatData.Seen.Clear();
                    SaveAndRefresh(true);
                }
            };

            Button btnClearTranscripts = CyberControls.Button("EFFACER LES TRANSCRIPTIONS", 250, 38, NeonTheme.Edge, NeonTheme.Yellow);
            btnClearTranscripts.Dock = DockStyle.Fill;
            btnClearTranscripts.Margin = new Padding(4, 4, 0, 4);
            btnClearTranscripts.Click += (s, e) =>
            {
                if (MessageBox.Show("Supprimer les journaux quotidiens et unfolded.txt ?",
                    "CaprICE // Transcriptions", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

                try
                {
                    TranscriptManager.DeleteAll();
                    currentTranscripts = TranscriptManager.LoadAll();
                    RenderMessageCards();
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Impossible de supprimer les transcriptions : " + ex.Message,
                        "CaprICE // Transcriptions", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Accès refusé lors de la suppression des transcriptions : " + ex.Message,
                        "CaprICE // Transcriptions", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            header.Controls.Add(lblTitle, 0, 0);
            header.Controls.Add(btnClose, 1, 0);
            header.Controls.Add(lblSub, 0, 1);
            header.SetColumnSpan(lblSub, 2);
            footer.Controls.Add(btnClearAll, 0, 0);
            footer.Controls.Add(btnClearTranscripts, 1, 0);

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(pnlMessageList, 0, 1);
            layout.Controls.Add(footer, 0, 2);
            pnlChatDrawer.Controls.Add(layout);

            this.Controls.Add(pnlChatDrawer);
        }

        private void RenderMessageCards()
        {
            if (pnlMessageList == null || pnlMessageList.IsDisposed) return;

            pnlMessageList.SuspendLayout();
            try
            {
                while (pnlMessageList.Controls.Count > 0)
                {
                    Control oldControl = pnlMessageList.Controls[0];
                    pnlMessageList.Controls.RemoveAt(0);
                    oldControl.Dispose();
                }
                if (currentChatData == null) return;

                int topOffset = 0;

                if (currentChatData.Messages != null)
                {
                    for (int i = 0; i < currentChatData.Messages.Count; i++)
                    {
                        var msg = currentChatData.Messages[i];
                        if (msg == null) continue;

                        Panel card = CreateMessageCard(
                            role: msg.Role?.ToUpper() ?? "MSG",
                            content: msg.Content,
                            headerColor: msg.Role == "user" ? NeonTheme.Cyan : NeonTheme.Rose,
                            onDelete: () => DeleteChatMessage(msg)
                        );

                        card.Top = topOffset;
                        pnlMessageList.Controls.Add(card);
                        topOffset += card.Height + 10;
                    }
                }

                if (currentChatData.Seen != null && currentChatData.Seen.Count > 0)
                {
                    AddSectionHeader("--- CAPTURES D'ÉCRAN / MÉMOIRE VISUELLE ---", NeonTheme.Yellow, ref topOffset);

                    for (int i = 0; i < currentChatData.Seen.Count; i++)
                    {
                        var sighting = currentChatData.Seen[i];
                        if (sighting == null) continue;

                        string displayText = "[VU] " + sighting.Saw + (string.IsNullOrEmpty(sighting.Said) ? "" : "\r\n[RÉPONSE] " + sighting.Said);

                        Panel card = CreateMessageCard(
                            role: "SIGHTING",
                            content: displayText,
                            headerColor: NeonTheme.Yellow,
                            onDelete: () => DeleteSighting(sighting)
                        );

                        card.Top = topOffset;
                        pnlMessageList.Controls.Add(card);
                        topOffset += card.Height + 10;
                    }
                }

                if (currentTranscripts != null && currentTranscripts.Count > 0)
                {
                    AddSectionHeader("--- TRANSCRIPTIONS LOCALES ---", NeonTheme.Cyan, ref topOffset);

                    foreach (TranscriptData transcript in currentTranscripts)
                    {
                        TranscriptData selectedTranscript = transcript;
                        Panel card = CreateMessageCard(
                            role: selectedTranscript.Name,
                            content: selectedTranscript.Content,
                            headerColor: NeonTheme.Cyan,
                            onDelete: () => DeleteTranscript(selectedTranscript.Name)
                        );

                        card.Top = topOffset;
                        pnlMessageList.Controls.Add(card);
                        topOffset += card.Height + 10;
                    }
                }
            }
            finally
            {
                pnlMessageList.ResumeLayout();
            }
        }

        private void AddSectionHeader(string text, Color color, ref int topOffset)
        {
            Label header = new Label
            {
                Text = text,
                Font = NeonTheme.FontSmall,
                ForeColor = color,
                Top = topOffset + 10,
                Height = 24,
                Width = Math.Max(100, pnlMessageList.ClientSize.Width - 25)
            };
            pnlMessageList.Controls.Add(header);
            topOffset += 34;
        }

        private Panel CreateMessageCard(string role, string content, Color headerColor, Action onDelete)
        {
            string displayContent = content ?? string.Empty;
            int cardWidth = Math.Max(220, pnlMessageList.ClientSize.Width - 25);
            int textWidth = Math.Max(120, cardWidth - 42);
            int textHeight = Math.Max(40, Math.Min(220,
                TextRenderer.MeasureText(displayContent, NeonTheme.FontMono, new Size(textWidth, 0),
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height + 10));

            Panel card = new Panel
            {
                Width = cardWidth,
                BackColor = NeonTheme.Field,
                BorderStyle = BorderStyle.None
            };

            TableLayoutPanel cardLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = NeonTheme.Field,
                Padding = new Padding(10),
                Margin = Padding.Empty
            };
            cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            Label lblRole = new Label
            {
                Text = role ?? "MSG",
                Font = NeonTheme.FontSmall,
                ForeColor = headerColor,
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            TextBox txtContent = new TextBox
            {
                Text = displayContent,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = NeonTheme.Field,
                ForeColor = NeonTheme.White,
                Font = NeonTheme.FontMono,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill
            };

            Button btnDelete = CyberControls.Button("SUPPRIMER", 130, 28, NeonTheme.Edge, NeonTheme.Rose);
            btnDelete.Dock = DockStyle.Fill;
            btnDelete.Margin = Padding.Empty;
            btnDelete.Click += (s, e) => onDelete();

            card.Height = textHeight + 76;
            cardLayout.Controls.Add(lblRole, 0, 0);
            cardLayout.Controls.Add(txtContent, 0, 1);
            cardLayout.Controls.Add(btnDelete, 0, 2);
            card.Controls.Add(cardLayout);

            return card;
        }

        private void DeleteChatMessage(ChatMessageData selected)
        {
            currentChatData = ChatHistoryManager.LoadData();
            int index = currentChatData.Messages.FindIndex(message =>
                message != null && message.Role == selected.Role && message.Content == selected.Content);
            if (index < 0)
            {
                RenderMessageCards();
                MessageBox.Show("Ce message n'existe plus dans l'historique actuel. La liste a été actualisée.",
                    "CaprICE // Chat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            currentChatData.Messages.RemoveAt(index);
            SaveAndRefresh(true);
        }

        private void DeleteSighting(SightingData selected)
        {
            currentChatData = ChatHistoryManager.LoadData();
            int index = currentChatData.Seen.FindIndex(sighting =>
                sighting != null && sighting.At == selected.At && sighting.Saw == selected.Saw
                && sighting.Said == selected.Said && sighting.Asked == selected.Asked);
            if (index < 0)
            {
                RenderMessageCards();
                MessageBox.Show("Cette capture n'existe plus dans l'historique actuel. La liste a été actualisée.",
                    "CaprICE // Chat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            currentChatData.Seen.RemoveAt(index);
            SaveAndRefresh(false);
        }

        private void DeleteTranscript(string fileName)
        {
            try
            {
                TranscriptManager.Delete(fileName);
                currentTranscripts = TranscriptManager.LoadAll();
                RenderMessageCards();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Impossible de supprimer la transcription : " + ex.Message,
                    "CaprICE // Transcriptions", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Accès refusé lors de la suppression de la transcription : " + ex.Message,
                    "CaprICE // Transcriptions", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveAndRefresh(bool clearBrainHistory)
        {
            if (ChatHistoryManager.SaveData(currentChatData, out string err))
            {
                if (clearBrainHistory && !CaprICEHook.ClearBrainHistory())
                {
                    MessageBox.Show("L'historique est enregistré, mais l'historique actif de Capri n'a pas pu être vidé.",
                        "CaprICE // Chat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                RenderMessageCards();
            }
            else
            {
                MessageBox.Show("Erreur de sauvegarde : " + err, "CaprICE // Chat",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowChatDrawer()
        {
            try
            {
                currentChatData = ChatHistoryManager.LoadData();
                currentTranscripts = TranscriptManager.LoadAll();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Impossible de lire les transcriptions : " + ex.Message,
                    "CaprICE // Transcriptions", MessageBoxButtons.OK, MessageBoxIcon.Error);
                currentTranscripts = new List<TranscriptData>();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Accès refusé lors de la lecture des transcriptions : " + ex.Message,
                    "CaprICE // Transcriptions", MessageBoxButtons.OK, MessageBoxIcon.Error);
                currentTranscripts = new List<TranscriptData>();
            }

            RenderMessageCards();
            SwitchWorkspaceView(pnlChatDrawer, btnNavChat);
        }
    }
}