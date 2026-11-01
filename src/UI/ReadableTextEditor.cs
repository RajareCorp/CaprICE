using System;
using System.Drawing;
using System.Windows.Forms;

namespace CaprICE.UI
{
    internal sealed class ReadableTextEditor : RichTextBox
    {
        private readonly Color headingColor;
        private readonly Font boldFont;
        private readonly Timer formattingTimer;
        private bool formatting;

        public ReadableTextEditor(Color headingColor)
        {
            this.headingColor = headingColor;
            boldFont = new Font(NeonTheme.FontMono, FontStyle.Bold);
            formattingTimer = new Timer { Interval = 250 };
            formattingTimer.Tick += (s, e) =>
            {
                formattingTimer.Stop();
                ApplyReadableFormatting();
            };
            Multiline = true;
            WordWrap = true;
            AcceptsTab = true;
            DetectUrls = false;
            HideSelection = false;
            ScrollBars = RichTextBoxScrollBars.Vertical;
            BackColor = NeonTheme.Field;
            ForeColor = NeonTheme.White;
            Font = NeonTheme.FontMono;
            BorderStyle = BorderStyle.None;
            SelectionIndent = 10;
            SelectionRightIndent = 10;
            Padding = new Padding(8);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            formattingTimer.Stop();
            formattingTimer.Start();
        }

        private void ApplyReadableFormatting()
        {
            if (formatting || IsDisposed) return;
            formatting = true;

            int selectionStart = SelectionStart;
            int selectionLength = SelectionLength;
            try
            {
                SelectAll();
                SelectionColor = NeonTheme.White;
                SelectionFont = NeonTheme.FontMono;

                bool inCodeBlock = false;
                string[] lines = Lines;
                for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    string line = lines[lineIndex];
                    int offset = GetFirstCharIndexFromLine(lineIndex);
                    if (offset < 0) break;
                    string trimmed = line.TrimStart();
                    int prefixOffset = line.Length - trimmed.Length;
                    if (trimmed.StartsWith("```", StringComparison.Ordinal))
                    {
                        inCodeBlock = !inCodeBlock;
                        Select(offset, line.Length);
                        SelectionColor = NeonTheme.Faint;
                        SelectionFont = boldFont;
                    }
                    else if (inCodeBlock)
                    {
                        Select(offset, line.Length);
                        SelectionColor = NeonTheme.Cyan;
                    }
                    else if (trimmed.StartsWith("#", StringComparison.Ordinal)
                        || trimmed.StartsWith("==", StringComparison.Ordinal))
                    {
                        Select(offset, line.Length);
                        SelectionColor = headingColor;
                        SelectionFont = boldFont;
                    }
                    else if (trimmed.StartsWith(">", StringComparison.Ordinal))
                    {
                        Select(offset, line.Length);
                        SelectionColor = NeonTheme.Cyan;
                    }
                    else if (IsListLine(trimmed))
                    {
                        Select(offset + prefixOffset, 1);
                        SelectionColor = NeonTheme.Yellow;
                        SelectionFont = boldFont;
                    }
                }

                Select(Math.Min(selectionStart, TextLength), Math.Min(selectionLength, Math.Max(0, TextLength - selectionStart)));
                ScrollToCaret();
            }
            finally
            {
                formatting = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                formattingTimer.Stop();
                formattingTimer.Dispose();
                boldFont.Dispose();
            }
            base.Dispose(disposing);
        }

        private static bool IsListLine(string line)
        {
            if (line.Length < 2) return false;
            if ((line[0] == '-' || line[0] == '*' || line[0] == '+') && char.IsWhiteSpace(line[1]))
                return true;

            int index = 0;
            while (index < line.Length && char.IsDigit(line[index])) index++;
            return index > 0 && index < line.Length
                && (line[index] == '.' || line[index] == ')')
                && index + 1 < line.Length && char.IsWhiteSpace(line[index + 1]);
        }
    }
}
