using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ChromiumBackground
{
    public static class FramelessWindowExtension
    {
        public static void FramelessOnSourceInitialized(this Window self, EventArgs e)
        {
            System.IntPtr handle = (new WindowInteropHelper(self)).Handle;
            HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
        }

        private const int WM_CREATE = 0x0001;
        private const int WM_NCCALCSIZE = 0x0083;
        private const uint SWP_FRAMECHANGED = 0x0020;


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int _Left;
            public int _Top;
            public int _Right;
            public int _Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        private static System.IntPtr WindowProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            if (msg == WM_CREATE)
            {
                RECT rcClient;
                GetWindowRect(hwnd, out rcClient);

                // Inform the application of the frame change.
                SetWindowPos(hwnd,
                             IntPtr.Zero,
                             rcClient._Left, rcClient._Top,
                             rcClient._Right - rcClient._Left, rcClient._Top - rcClient._Bottom,
                             SWP_FRAMECHANGED);
            }
            else if (msg == WM_NCCALCSIZE && wParam != IntPtr.Zero)
            {
                handled = true;
            }

            return (System.IntPtr)0;
        }
    }
}