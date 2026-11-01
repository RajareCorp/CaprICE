using System;
using System.Drawing;
using System.Drawing.Imaging;
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

    /// <summary>
    /// Overlay temporaire effectuant un effet de décomposition Glitch RGB lors du swap de vue.
    /// </summary>
    public class GlitchOverlay : Control
    {
        private Bitmap snapshot;
        private Timer glitchTimer;
        private int frameCount = 0;
        private Random rnd = new Random();

        public GlitchOverlay(Control target)
        {
            this.Size = target.Size;
            this.Location = new Point(0, 0);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            // Capture un snapshot de la vue cible
            snapshot = new Bitmap(target.Width, target.Height);
            target.DrawToBitmap(snapshot, new Rectangle(0, 0, target.Width, target.Height));

            glitchTimer = new Timer { Interval = 25 };
            glitchTimer.Tick += (s, e) =>
            {
                frameCount++;
                Invalidate();
                if (frameCount > 8) // L'effet dure environ 200ms
                {
                    glitchTimer.Stop();
                    if (this.Parent != null)
                        this.Parent.Controls.Remove(this);
                    this.Dispose();
                }
            };
            glitchTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (snapshot == null) return;

            Graphics g = e.Graphics;
            int shiftX = rnd.Next(-12, 12);

            // Rendu canal Cyan (Décalé)
            using (ImageAttributes attrCyan = new ImageAttributes())
            {
                ColorMatrix cmCyan = new ColorMatrix(new float[][] {
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 0.7f, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
                attrCyan.SetColorMatrix(cmCyan);
                g.DrawImage(snapshot, new Rectangle(shiftX, 0, Width, Height), 0, 0, Width, Height, GraphicsUnit.Pixel, attrCyan);
            }

            // Rendu canal Red/Rose (Décalé inverse)
            using (ImageAttributes attrRed = new ImageAttributes())
            {
                ColorMatrix cmRed = new ColorMatrix(new float[][] {
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0.7f, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
                attrRed.SetColorMatrix(cmRed);
                g.DrawImage(snapshot, new Rectangle(-shiftX, 0, Width, Height), 0, 0, Width, Height, GraphicsUnit.Pixel, attrRed);
            }

            // Tranches horizontales décalées aléatoirement (Slice Glitch)
            for (int i = 0; i < 3; i++)
            {
                int sliceY = rnd.Next(0, Height - 20);
                int sliceH = rnd.Next(5, 25);
                int sliceOffset = rnd.Next(-25, 25);
                Rectangle srcRect = new Rectangle(0, sliceY, Width, sliceH);
                Rectangle destRect = new Rectangle(sliceOffset, sliceY, Width, sliceH);
                g.DrawImage(snapshot, destRect, srcRect, GraphicsUnit.Pixel);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (glitchTimer != null)
                {
                    glitchTimer.Stop();
                    glitchTimer.Dispose();
                    glitchTimer = null;
                }

                if (snapshot != null)
                {
                    snapshot.Dispose();
                    snapshot = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}