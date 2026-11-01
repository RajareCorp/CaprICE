using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CaprICE.UI
{
    // The visual identity of Capri: dark cyberpunk ground, wine glow, rose highlights,
    // cyan accents, Bahnschrift typography, and clipped hexagonal cyber corners.
    public static class NeonTheme
    {
        public static readonly Color Ground = Color.FromArgb(10, 10, 16);
        public static readonly Color Glow = Color.FromArgb(54, 14, 26);
        public static readonly Color Band = Color.FromArgb(16, 12, 20);
        public static readonly Color Rule = Color.FromArgb(44, 122, 134);
        public static readonly Color Rose = Color.FromArgb(255, 93, 102);
        public static readonly Color White = Color.FromArgb(236, 236, 241);
        public static readonly Color Faint = Color.FromArgb(146, 130, 140);
        public static readonly Color Field = Color.FromArgb(17, 16, 28);
        public static readonly Color Hover = Color.FromArgb(30, 20, 36);
        public static readonly Color Pressed = Color.FromArgb(46, 22, 38);
        public static readonly Color Edge = Color.FromArgb(92, 24, 36);
        public static readonly Color Hot = Color.FromArgb(232, 66, 70);
        public static readonly Color Cyan = Color.FromArgb(94, 240, 246);
        public static readonly Color Yellow = Color.FromArgb(255, 185, 0);
        public static readonly Color Deep = Color.FromArgb(9, 30, 33);
        public static readonly Color DeepEdge = Color.FromArgb(40, 120, 132);
        public static readonly Color OnInk = Color.FromArgb(6, 14, 18);
        public static readonly Color Dash = Color.FromArgb(78, 28, 40);
        public static readonly Color Brim = Color.FromArgb(120, 30, 44);
        public static readonly Color Well = Color.FromArgb(12, 11, 20);
        public static readonly Color WellEdge = Color.FromArgb(72, 28, 40);

        // Status Colors
        public static readonly Color StatusAllowed = Color.FromArgb(94, 240, 246);  // Cyan
        public static readonly Color StatusBlocked = Color.FromArgb(255, 93, 102);  // Rose
        public static readonly Color StatusWarning = Color.FromArgb(255, 185, 0);   // Amber

        // Enlarged Fonts for high comfort and readability
        public static Font FontDisplay = CreateFont("Bahnschrift SemiBold Condensed", "Segoe UI Semibold", 16.0f, FontStyle.Bold);
        public static Font FontTitle = CreateFont("Bahnschrift SemiBold SemiConden", "Segoe UI", 14.0f, FontStyle.Bold);
        public static Font FontSubtitle = CreateFont("Bahnschrift SemiCondensed", "Segoe UI", 12.0f, FontStyle.Regular);
        public static Font FontBody = CreateFont("Bahnschrift SemiCondensed", "Segoe UI", 11.5f, FontStyle.Regular);
        public static Font FontMono = CreateFont("Consolas", "ShareTechMono-Regular", 11.0f, FontStyle.Regular);
        public static Font FontSmall = CreateFont("Bahnschrift Condensed", "Segoe UI", 10.0f, FontStyle.Regular);
        public static Font FontBadge = CreateFont("Bahnschrift SemiBold SemiConden", "Segoe UI", 10.5f, FontStyle.Bold);

        private static Font CreateFont(string family1, string family2, float size, FontStyle style)
        {
            try
            {
                using (FontFamily f1 = new FontFamily(family1))
                    return new Font(f1, size, style);
            }
            catch
            {
                try
                {
                    using (FontFamily f2 = new FontFamily(family2))
                        return new Font(f2, size, style);
                }
                catch
                {
                    return new Font(FontFamily.GenericSansSerif, size, style);
                }
            }
        }

        // Polygon with corners clipped (top-left big, top-right small, bottom-right big, bottom-left small)
        public static GraphicsPath Cut(RectangleF box, float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(new[]
            {
                new PointF(box.Left + topLeft, box.Top),
                new PointF(box.Right - topRight, box.Top),
                new PointF(box.Right, box.Top + topRight),
                new PointF(box.Right, box.Bottom - bottomRight),
                new PointF(box.Right - bottomRight, box.Bottom),
                new PointF(box.Left + bottomLeft, box.Bottom),
                new PointF(box.Left, box.Bottom - bottomLeft),
                new PointF(box.Left, box.Top + topLeft)
            });
            return path;
        }

        // Cuts a window to Capri's signature clipped corners
        public static void Clip(Control window, float big, float small)
        {
            Region old = window.Region;
            using (GraphicsPath hull = Cut(new RectangleF(0, 0, window.Width, window.Height), big, small, big, small))
                window.Region = new Region(hull);
            if (old != null) old.Dispose();
        }

        // Draws the cyber frame edge around the window with hot red corner highlights
        public static void Frame(Graphics graphics, Size size, float big, float small, float along, float down)
        {
            SmoothingMode was = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF box = new RectangleF(0.5f, 0.5f, size.Width - 2, size.Height - 2);

            using (GraphicsPath edge = Cut(box, big, small, big, small))
            using (Pen line = new Pen(Brim, 1.8f))
            {
                graphics.DrawPath(line, edge);
            }

            RectangleF hot = RectangleF.Inflate(box, -0.6f, -0.6f);
            using (Pen stroke = new Pen(Hot, 2.8f))
            {
                // Top-left big corner accent
                graphics.DrawLines(stroke, new[]
                {
                    new PointF(hot.Left, hot.Top + big + down),
                    new PointF(hot.Left, hot.Top + big),
                    new PointF(hot.Left + big, hot.Top),
                    new PointF(hot.Left + big + along, hot.Top)
                });
                // Bottom-right big corner accent
                graphics.DrawLines(stroke, new[]
                {
                    new PointF(hot.Right, hot.Bottom - big - down),
                    new PointF(hot.Right, hot.Bottom - big),
                    new PointF(hot.Right - big, hot.Bottom),
                    new PointF(hot.Right - big - along, hot.Bottom)
                });
            }

            graphics.SmoothingMode = was;
        }

        // Wine glow backdrop fading down from the top
        public static void Backdrop(Graphics graphics, Size size, float reach)
        {
            using (Brush ground = new SolidBrush(Ground))
                graphics.FillRectangle(ground, 0, 0, size.Width, size.Height);

            float glow = Math.Min(size.Height, reach);
            if (glow < 1) return;

            using (LinearGradientBrush wine = new LinearGradientBrush(
                new RectangleF(0, -1, size.Width, glow + 2),
                Glow,
                Ground,
                LinearGradientMode.Vertical))
            {
                graphics.FillRectangle(wine, 0, 0, size.Width, glow);
            }
        }
    }
}
