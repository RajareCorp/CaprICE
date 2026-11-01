using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CaprICE.UI
{
    /// <summary>Appels Win32 utilisés pour déplacer les fenêtres sans bordure.</summary>
    internal static class NativeMethods
    {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        /// <summary>Lance le déplacement de la fenêtre en glissant avec le bouton gauche.</summary>
        public static void DragWindow(Form window, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(window.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
    }
}
