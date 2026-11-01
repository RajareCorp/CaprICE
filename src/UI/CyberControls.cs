using System.Drawing;
using System.Windows.Forms;

namespace CaprICE.UI
{
    internal static class CyberControls
    {
        public static Button Button(string text, int width, int height, Color borderColor, Color textColor)
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

    /// <summary>
    /// Bouton de navigation latérale au style Cyberpunk avec indicateur d'activation Néon.
    /// </summary>
    public class CyberNavButton : Button
    {
        private bool isActive = false;
        public bool IsActive
        {
            get => isActive;
            set { isActive = value; Invalidate(); }
        }

        public CyberNavButton(string text)
        {
            this.Text = text;
            this.Height = 48;
            this.Dock = DockStyle.Top;
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Font = NeonTheme.FontTitle;
            this.ForeColor = NeonTheme.White;
            this.BackColor = Color.Transparent;
            this.TextAlign = ContentAlignment.MiddleLeft;
            this.Padding = new Padding(20, 0, 0, 0);
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            Graphics g = pevent.Graphics;

            if (isActive)
            {
                using (SolidBrush activeBg = new SolidBrush(Color.FromArgb(40, NeonTheme.Cyan)))
                    g.FillRectangle(activeBg, 0, 0, Width, Height);

                using (SolidBrush bar = new SolidBrush(NeonTheme.Cyan))
                    g.FillRectangle(bar, 0, 0, 4, Height);

                this.ForeColor = NeonTheme.Cyan;
            }
            else
            {
                this.ForeColor = NeonTheme.White;
            }
        }
    }

}