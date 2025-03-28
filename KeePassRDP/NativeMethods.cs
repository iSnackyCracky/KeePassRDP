/*
 *  Copyright (C) 2018 - 2025 iSnackyCracky, NETertainer
 *
 *  This file is part of KeePassRDP.
 *
 *  KeePassRDP is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  KeePassRDP is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with KeePassRDP.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.AccessControl;
using System.Text;

namespace KeePassRDP
{
    internal static class NativeMethods
    {
        [DllImport("User32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = false)]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        [DllImport("User32.dll", EntryPoint = "GetWindowTextLengthW", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("User32.dll", EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int GetWindowText(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

        [DllImport("User32.dll", EntryPoint = "FindWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);

        [DllImport("User32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("User32.dll", EntryPoint = "SetWindowPos", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public static readonly IntPtr HWND_TOP = IntPtr.Zero;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOREDRAW = 0x0008;
        public const uint SWP_NOACTIVATE = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        [DllImport("User32.dll", EntryPoint = "SetWindowDisplayAffinity", SetLastError = false)]
        public static extern uint SetWindowDisplayAffinity(IntPtr hwnd, uint dwAffinity);

        public const int WDA_EXCLUDEFROMCAPTURE = 1;

        [DllImport("User32.dll", EntryPoint = "GetLastActivePopup", SetLastError = true)]
        public static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        [DllImport("User32.dll", EntryPoint = "ShowWindowAsync", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("User32.dll", EntryPoint = "SetForegroundWindow", SetLastError = false)]
        public static extern int SetForegroundWindow(IntPtr handle);

        [DllImport("User32.dll", EntryPoint = "GetForegroundWindow", SetLastError = false)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll", EntryPoint = "GetDesktopWindow", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("User32.dll", EntryPoint = "GetDC", SetLastError = false)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll", EntryPoint = "ReleaseDC", SetLastError = false)]
        public static extern int ReleaseDC([In] IntPtr hWnd, [In] IntPtr hDC);

        public const int BM_CLICK = 0x00F5;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_CLOSE = 0x0010;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_NCPAINT = 0x0085;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_DWMCOMPOSITIONCHANGED = 0x031E;
        public const int WM_USER = 0x0400;
        public const int SC_RESTORE = 0xF120;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_CHILD = 0x40000000;
        public const int WS_POPUP = unchecked((int)0x80000000);
        public const int WS_EX_NOPARENTNOTIFY = 0x00000004;
        public const int WS_EX_TOPMOST = 0x00000008;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_APPWINDOW = 0x00040000;
        public const int WS_EX_NOINHERITLAYOUT = 0x00100000;
        public const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int CS_PARENTDC = 0x0080;
        /*public const int CS_CLASSDC = 0x0040;
        public const int CS_OWNDC = 0x0020;*/

        public static readonly IntPtr HWND_BROADCAST = (IntPtr)0xffff;
        public static readonly IntPtr HwndMessage = new IntPtr(-3);

        [DllImport("User32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("User32.dll", EntryPoint = "PostMessageW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll", EntryPoint = "RegisterWindowMessageW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern int RegisterWindowMessage(string lpString);

        [DllImport("User32.dll", EntryPoint = "ReleaseCapture", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReleaseCapture();

        [DllImport("User32.dll", EntryPoint = "PaintDesktop", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PaintDesktop(IntPtr hdc);

        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("User32.dll", EntryPoint = "EnumDesktopWindows", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDesktopWindows([In] IntPtr hDesktop, [In] EnumWindowsProc lpfn, [In] IntPtr lParam);

        [DllImport("User32.dll", EntryPoint = "CreateDesktopW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, [MarshalAs(UnmanagedType.U4)] DesktopAccess dwDesiredAccess, IntPtr lpsa);

        [DllImport("User32.dll", EntryPoint = "OpenDesktopW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenDesktop([In] string lpszDesktop, [In] int dwFlags, [In] bool fInherit, [In][MarshalAs(UnmanagedType.U4)] DesktopAccess dwDesiredAccess);

        [DllImport("User32.dll", EntryPoint = "OpenInputDesktop", SetLastError = true)]
        public static extern IntPtr OpenInputDesktop([In] int dwFlags, [In] bool fInherit, [MarshalAs(UnmanagedType.U4)] DesktopAccess dwDesiredAccess);

        [Flags]
        public enum DesktopAccess : uint
        {
            DesktopReadObjects = 0x0001,
            DesktopCreateWindow = 0x0002,
            DesktopCreateMenu = 0x0004,
            DesktopHookControl = 0x0008,
            DesktopJournalRecord = 0x0010,
            DesktopJournalPlayback = 0x0020,
            DesktopEnumerate = 0x0040,
            DesktopWriteObjects = 0x0080,
            DesktopSwitchDesktop = 0x0100,
            GenericAll = DesktopReadObjects | DesktopCreateWindow | DesktopCreateMenu |
                         DesktopHookControl | DesktopJournalRecord | DesktopJournalPlayback |
                         DesktopEnumerate | DesktopWriteObjects | DesktopSwitchDesktop
        }

        [DllImport("User32.dll", EntryPoint = "SwitchDesktop", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SwitchDesktop(IntPtr hDesktop);

        [DllImport("User32.dll", EntryPoint = "CloseDesktop", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseDesktop(IntPtr handle);

        [DllImport("User32.dll", EntryPoint = "SetThreadDesktop", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("User32.dll", EntryPoint = "GetThreadDesktop", SetLastError = true)]
        public static extern IntPtr GetThreadDesktop(uint dwThreadId);

        [DllImport("User32.dll", EntryPoint = "GetUserObjectInformationW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetUserObjectInformation([In] IntPtr hObj, [In] int nIndex, [Out, Optional] IntPtr pvInfo, [In] int nLength, [In, Out, Optional] ref int lpnLengthNeeded);

        [DllImport("User32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = false)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern uint GetWindowThreadProcessId([In] IntPtr hWnd, [Out, Optional] out uint lpdwProcessId);

        [DllImport("Kernel32.dll", EntryPoint = "GetCurrentThreadId", SetLastError = false)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern uint GetCurrentThreadId();

        [Flags]
        public enum WINEVENT_FLAGS : uint
        {
            WINEVENT_OUTOFCONTEXT = 0,
            WINEVENT_SKIPOWNTHREAD = 1,
            WINEVENT_SKIPOWNPROCESS = 2,
            WINEVENT_INCONTEXT = 4
        }

        public delegate void WinEventDelegate(IntPtr hWinEventHook, [MarshalAs(UnmanagedType.U4)] WINEVENT_EVENTS eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("User32.dll", SetLastError = false, EntryPoint = "SetWinEventHook")]
        public static extern IntPtr SetWinEventHook([MarshalAs(UnmanagedType.U4)] WINEVENT_EVENTS eventMin, [MarshalAs(UnmanagedType.U4)] WINEVENT_EVENTS eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, [MarshalAs(UnmanagedType.U4)] WINEVENT_FLAGS dwFlags);

        [DllImport("User32.dll", SetLastError = false, EntryPoint = "UnhookWinEvent")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        public enum WINEVENT_EVENTS : uint
        {
            EVENT_SYSTEM_FOREGROUND = 0x0003,
            EVENT_SYSTEM_MINIMIZESTART = 0x0016,
            EVENT_SYSTEM_MINIMIZEEND = 0x0017,
            EVENT_SYSTEM_DESKTOPSWITCH = 0x0020,
            EVENT_OBJECT_FOCUS = 0x8005
        }

        [DllImport("User32.dll", SetLastError = true)]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowCompositionAttributeData
        {
            public DWMWINDOWATTRIBUTE Attribute;
            public IntPtr Data;  //Will point to an AccentPolicy struct, where Attribute will be DWMWINDOWATTRIBUTE.AccentPolicy
            public int SizeOfData;
        }

        public enum DWMACCENTSTATE
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_ENABLE_HOSTBACKDROP = 5,
            ACCENT_INVALID_STATE = 6
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AccentPolicy
        {
            public DWMACCENTSTATE AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [Flags]
        public enum DWM_BB : int
        {
            DWM_BB_ENABLE = 1,
            DWM_BB_BLURREGION = 2,
            DWM_BB_TRANSITIONONMAXIMIZED = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_BLURBEHIND
        {
            [MarshalAs(UnmanagedType.U4)]
            public DWM_BB dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        public enum DWMNCRENDERINGPOLICY : int
        {
            DWMNCRP_USEWINDOWSTYLE,
            DWMNCRP_DISABLED,
            DWMNCRP_ENABLED,
            DWMNCRP_LAST
        }

        public enum DWMWINDOWATTRIBUTE : int
        {
            /*DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY = 2,
            DWMWA_TRANSITIONS_FORCEDISABLED = 3,
            DWMWA_ALLOW_NCPAINT = 4,
            DWMWA_CAPTION_BUTTON_BOUNDS = 5,
            DWMWA_NONCLIENT_RTL_LAYOUT = 6,
            DWMWA_FORCE_ICONIC_REPRESENTATION = 7,
            DWMWA_FLIP3D_POLICY = 8,
            DWMWA_EXTENDED_FRAME_BOUNDS = 9,
            DWMWA_HAS_ICONIC_BITMAP = 10,
            DWMWA_DISALLOW_PEEK = 11,
            DWMWA_EXCLUDED_FROM_PEEK = 12,
            DWMWA_CLOAK = 13,
            DWMWA_CLOAKED = 14,
            DWMWA_FREEZE_REPRESENTATION = 15,
            DWMWA_PASSIVE_UPDATE_MODE,
            DWMWA_USE_HOSTBACKDROPBRUSH,
            DWMWA_ACCENT_POLICY = 19, // undocumented
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_WINDOW_CORNER_PREFERENCE = 33,
            DWMWA_BORDER_COLOR,
            DWMWA_CAPTION_COLOR,
            DWMWA_TEXT_COLOR,
            DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,
            DWMWA_SYSTEMBACKDROP_TYPE,
            DWMWA_LAST*/

            WCA_UNDEFINED = 0,
            WCA_NCRENDERING_ENABLED = 1,
            WCA_NCRENDERING_POLICY = 2,
            WCA_TRANSITIONS_FORCEDISABLED = 3,
            WCA_ALLOW_NCPAINT = 4,
            WCA_CAPTION_BUTTON_BOUNDS = 5,
            WCA_NONCLIENT_RTL_LAYOUT = 6,
            WCA_FORCE_ICONIC_REPRESENTATION = 7,
            WCA_EXTENDED_FRAME_BOUNDS = 8,
            WCA_HAS_ICONIC_BITMAP = 9,
            WCA_THEME_ATTRIBUTES = 10,
            WCA_NCRENDERING_EXILED = 11,
            WCA_NCADORNMENTINFO = 12,
            WCA_EXCLUDED_FROM_LIVEPREVIEW = 13,
            WCA_VIDEO_OVERLAY_ACTIVE = 14,
            WCA_FORCE_ACTIVEWINDOW_APPEARANCE = 15,
            WCA_DISALLOW_PEEK = 16,
            WCA_CLOAK = 17,
            WCA_CLOAKED = 18,
            WCA_ACCENT_POLICY = 19,
            WCA_FREEZE_REPRESENTATION = 20,
            WCA_EVER_UNCLOAKED = 21,
            WCA_VISUAL_OWNER = 22,
            WCA_HOLOGRAPHIC = 23,
            WCA_EXCLUDED_FROM_DDA = 24,
            WCA_PASSIVEUPDATEMODE = 25,
            WCA_LAST = 26
        }

        [DllImport("DwmApi.dll", SetLastError = false, EntryPoint = "DwmEnableBlurBehindWindow")]
        public static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurBehind);

        [DllImport("DwmApi.dll", SetLastError = false, EntryPoint = "DwmExtendFrameIntoClientArea")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("DwmApi.dll", SetLastError = false, EntryPoint = "DwmSetWindowAttribute")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, [MarshalAs(UnmanagedType.I4)] DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);

        [DllImport("DwmApi.dll", SetLastError = false, EntryPoint = "DwmIsCompositionEnabled")]
        public static extern int DwmIsCompositionEnabled([MarshalAs(UnmanagedType.Bool)] ref bool pfEnabled);

        [DllImport("DwmApi.dll", SetLastError = false, EntryPoint = "DwmGetColorizationColor")]
        public static extern int DwmGetColorizationColor([Out] out uint pcrColorization, [Out][MarshalAs(UnmanagedType.Bool)] out bool pfOpaqueBlend);

        [SecurityCritical]
        [DllImport("uxtheme.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsThemeActive();

        /*public const uint CREDUI_NO_ERROR = 0;
        public const uint CREDUI_ERROR_INVALID_ACCOUNT_NAME = 1315;
        public const uint CREDUI_ERROR_INSUFFICIENT_BUFFER = 122;
        public const uint CREDUI_ERROR_INVALID_PARAMETER = 87;*/
        public const int CREDUI_MAX_USERNAME_LENGTH = 513;
        public const int CREDUI_MAX_DOMAIN_TARGET_LENGTH = 337;

        [DllImport("credui.dll", EntryPoint = "CredUIParseUserNameW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint CredUIParseUserName([In] string userName, [Out] StringBuilder user, [In] int userBufferSize, [Out] StringBuilder domain, [In] int domainBufferSize);

        [Flags]
        public enum ProcessAccessRights : uint
        {
            PROCESS_TERMINATE = 0x00000001, //  Required to terminate a process using TerminateProcess.
            PROCESS_CREATE_THREAD = 0x00000002, //  Required to create a thread.
            PROCESS_VM_OPERATION = 0x00000008, //   Required to perform an operation on the address space of a process (see VirtualProtectEx and WriteProcessMemory).
            PROCESS_VM_READ = 0x00000010, //    Required to read memory in a process using ReadProcessMemory.
            PROCESS_VM_WRITE = 0x00000020, //   Required to write to memory in a process using WriteProcessMemory.
            PROCESS_DUP_HANDLE = 0x00000040, // Required to duplicate a handle using DuplicateHandle.
            PROCESS_CREATE_PROCESS = 0x00000080, //  Required to create a process.
            PROCESS_SET_QUOTA = 0x00000100, //  Required to set memory limits using SetProcessWorkingSetSize.
            PROCESS_SET_INFORMATION = 0x00000200, //    Required to set certain information about a process, such as its priority class (see SetPriorityClass).
            PROCESS_QUERY_INFORMATION = 0x00000400, //  Required to retrieve certain information about a process, such as its token, exit code, and priority class (see OpenProcessToken, GetExitCodeProcess, GetPriorityClass, and IsProcessInJob).
            PROCESS_SUSPEND_RESUME = 0x00000800, // Required to suspend or resume a process.
            PROCESS_QUERY_LIMITED_INFORMATION = 0x00001000, //  Required to retrieve certain information about a process (see QueryFullProcessImageName). A handle that has the PROCESS_QUERY_INFORMATION access right is automatically granted PROCESS_QUERY_LIMITED_INFORMATION. Windows Server 2003 and Windows XP/2000:  This access right is not supported.
            DELETE = 0x00010000, // Required to delete the object.
            READ_CONTROL = 0x00020000, //   Required to read information in the security descriptor for the object, not including the information in the SACL. To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right. For more information, see SACL Access Right.
            WRITE_DAC = 0x00040000, //  Required to modify the DACL in the security descriptor for the object.
            WRITE_OWNER = 0x00080000, //    Required to change the owner in the security descriptor for the object.
            SYNCHRONIZE = 0x00100000, //    The right to use the object for synchronization. This enables a thread to wait until the object is in the signaled state.
            STANDARD_RIGHTS_REQUIRED = DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER,
            PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF
        }

        private const uint DACL_SECURITY_INFORMATION = 4;

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Advapi32.dll", EntryPoint = "GetKernelObjectSecurity", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetKernelObjectSecurity(IntPtr Handle, uint securityInformation, [Out] byte[] pSecurityDescriptor, uint nLength, out uint lpnLengthNeeded);

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Advapi32.dll", EntryPoint = "SetKernelObjectSecurity", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetKernelObjectSecurity(IntPtr Handle, uint securityInformation, [In] byte[] pSecurityDescriptor);

        public static RawSecurityDescriptor GetProcessSecurityDescriptor(IntPtr processHandle)
        {
            var psd = new byte[0];
            uint lengthNeeded;
            if (!GetKernelObjectSecurity(processHandle, DACL_SECURITY_INFORMATION, psd, 0, out lengthNeeded) && lengthNeeded > 0)
#pragma warning disable IDE0059
                if (!GetKernelObjectSecurity(processHandle, DACL_SECURITY_INFORMATION, psd = new byte[lengthNeeded], lengthNeeded, out lengthNeeded))
#pragma warning restore IDE0059
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            return new RawSecurityDescriptor(psd, 0);
        }

        public static void SetProcessSecurityDescriptor(IntPtr processHandle, RawSecurityDescriptor dacl)
        {
            var rawsd = new byte[dacl.BinaryLength];
            dacl.GetBinaryForm(rawsd, 0);
            if (!SetKernelObjectSecurity(processHandle, DACL_SECURITY_INFORMATION, rawsd))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /*[DllImport("Gdi32.dll", EntryPoint = "CreateRectRgn", SetLastError = true)]
        public static extern IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);

        [DllImport("Gdi32.dll", EntryPoint = "SelectClipRgn", SetLastError = true)]
        public static extern IntPtr SelectClipRgn(IntPtr hdc, IntPtr hrgn);*/
    }
}
