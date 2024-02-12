using System;
using System.Runtime.InteropServices;

namespace KeePassRDP
{
    internal static class NativeMethods
    {
        [DllImport("User32.dll", EntryPoint = "GetWindowTextLengthW", CharSet = CharSet.Unicode)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("User32.dll", EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

        [DllImport("User32.dll", EntryPoint = "FindWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);

        [DllImport("User32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("User32.dll", EntryPoint = "GetLastActivePopup", SetLastError = true)]
        public static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        public const uint BM_CLICK = 0x00F5;

        public const uint WM_LBUTTONDOWN = 0x0201;

        public const uint WM_LBUTTONUP = 0x0202;

        public const uint WM_CLOSE = 0x10;

        [DllImport("User32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
    }
}
