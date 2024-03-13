using System;
using System.Runtime.InteropServices;
using System.Text;

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

        /*public const uint CREDUI_NO_ERROR = 0;
        public const uint CREDUI_ERROR_INVALID_ACCOUNT_NAME = 1315;
        public const uint CREDUI_ERROR_INSUFFICIENT_BUFFER = 122;
        public const uint CREDUI_ERROR_INVALID_PARAMETER = 87;*/
        public const int CREDUI_MAX_USERNAME_LENGTH = 513;
        public const int CREDUI_MAX_DOMAIN_TARGET_LENGTH = 337;

        [DllImport("credui.dll", EntryPoint = "CredUIParseUserNameW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint CredUIParseUserName([In] string userName, [Out] StringBuilder user, [In] int userBufferSize, [Out] StringBuilder domain, [In] int domainBufferSize);
    }
}
