using System.Drawing;
using System.Windows.Forms;

namespace CaprICE.UI
{
    /// <summary>Fabriques de contrôles WinForms au style néon CaprICE.</summary>
    internal static class CyberControls
    {
        public static Button Button(string text, int width, int height, Color borderColor, Color textColor)
        {
            Button btn = new System.Windows.Forms.Button
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

        public static CheckBox Check(string text, bool check, int top)
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
    }
}
