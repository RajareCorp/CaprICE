using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CaprICE.UI
{
    /// <summary>Appels Win32 utilisés pour déplacer les fenêtres sans bordure.</summary>
    internal static class NativeMethods
    {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int ResizeBorder = 8;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetProcessDPIAware();

        /// <summary>Lance le déplacement de la fenêtre en glissant avec le bouton gauche.</summary>
        public static void DragWindow(Form window, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(window.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        public static int ResizeHitTest(Form window, IntPtr screenPoint)
        {
            if (window.WindowState != FormWindowState.Normal) return 0;

            long packed = screenPoint.ToInt64();
            Point client = window.PointToClient(new Point(
                unchecked((short)(packed & 0xFFFF)),
                unchecked((short)((packed >> 16) & 0xFFFF))));

            bool left = client.X >= 0 && client.X < ResizeBorder;
            bool right = client.X < window.ClientSize.Width && client.X >= window.ClientSize.Width - ResizeBorder;
            bool top = client.Y >= 0 && client.Y < ResizeBorder;
            bool bottom = client.Y < window.ClientSize.Height && client.Y >= window.ClientSize.Height - ResizeBorder;

            if (top && left) return HTTOPLEFT;
            if (top && right) return HTTOPRIGHT;
            if (bottom && left) return HTBOTTOMLEFT;
            if (bottom && right) return HTBOTTOMRIGHT;
            if (left) return HTLEFT;
            if (right) return HTRIGHT;
            if (top) return HTTOP;
            if (bottom) return HTBOTTOM;
            return 0;
        }
    }
}
