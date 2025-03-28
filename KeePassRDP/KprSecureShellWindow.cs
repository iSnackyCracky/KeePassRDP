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

using KeePass;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace KeePassRDP
{
    internal class KprSecureShellWindow : NativeWindow, IDisposable
    {
        [DllImport("User32.dll", EntryPoint = "PostMessageW", SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage([In, Optional] HandleRef hWnd, [In] uint Msg, [In] int wParam, [In] int lParam);

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Timer _timer;
        private readonly List<KprSecureDesktopAppBar> _appBars;
        private readonly ManualResetEventSlim _appBarsReady;

        private readonly DesktopWindow _desktopWindow;
        private readonly EmptyWindow _emptyWindow;
        private readonly KprSecureDesktopToolBar _toolBar;
        private readonly KprSecureLockScreen _lockScreen;

        private int _shellHookMessage;
        private int _taskBarCreatedMessage;
        private MINIMIZEDMETRICS? _mmOrg;

        internal DesktopWindow Desktop { get { return _desktopWindow; } }
        internal EmptyWindow Empty { get { return _emptyWindow; } }
        internal KprSecureDesktopToolBar ToolBar { get { return _toolBar; } }

        public KprSecureShellWindow(ToolStripRenderer tsr)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _timer = new Timer(Timer_Tick, null, TimeSpan.FromSeconds(60 - DateTime.Now.Second), TimeSpan.FromSeconds(60));

            _appBars = new List<KprSecureDesktopAppBar>();
            _appBarsReady = new ManualResetEventSlim(false);

            _desktopWindow = new DesktopWindow();
            _emptyWindow = new EmptyWindow();
            _toolBar = new KprSecureDesktopToolBar(tsr, true);
            _toolBar.VisibleChanged += ToolBar_VisibleChanged;
            _toolBar.FormClosed += ToolBar_FormClosed;
            _toolBar.MinimizeStart += Minimize;
            _toolBar.MinimizeEnd += Restore;
            _lockScreen = new KprSecureLockScreen();

            _shellHookMessage = 0;
            _taskBarCreatedMessage = 0;
            _mmOrg = null;
        }

        private void ToolBar_FormClosed(object sender, FormClosedEventArgs e)
        {
            HideAppBars(true);
        }

        private void ToolBar_VisibleChanged(object sender, EventArgs e)
        {
            if (_toolBar.Visible)
                ShowAppBars();
            else
                HideAppBars();
        }

        private void CreateHandle()
        {
            CreateHandle(new CreateParams
            {
                ClassName = "#32769",
                Style = 0,
                ExStyle = NativeMethods.WS_EX_NOREDIRECTIONBITMAP | NativeMethods.WS_EX_NOINHERITLAYOUT | NativeMethods.WS_EX_NOPARENTNOTIFY | NativeMethods.WS_EX_NOACTIVATE,
                ClassStyle = NativeMethods.CS_PARENTDC,
                Caption = string.Empty,
                Parent = NativeMethods.HwndMessage,
                Width = 0,
                Height = 0,
                X = 0,
                Y = 0
            });

            /*var hwnd = NativeMethods.GetDesktopWindow();
            NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_BOTTOM, rect.X, rect.Y, rect.Width, rect.Height, NativeMethods.SWP_NOACTIVATE);*/
            /*var hwnd = Handle;
            var hDC = NativeMethods.GetDC(hwnd);
            try
            {
                if (KeePass.Program.MainForm.InvokeRequired)
                    KeePass.Program.MainForm.Invoke(new Action(() => NativeMethods.PaintDesktop(hDC)));
                else
                    NativeMethods.PaintDesktop(hDC);
            }
            finally
            {
                NativeMethods.ReleaseDC(hwnd, hDC);
            }*/
        }

        public void Lock()
        {
            _lockScreen.Show(_emptyWindow);
            _lockScreen.Lock();

            foreach (var appBar in _appBars)
                appBar.IsLocked = true;

            _desktopWindow.SendToBack();
            _toolBar.Hide();

            Application.DoEvents();
        }

        public void Unlock()
        {
            _lockScreen.Unlock();

            _desktopWindow.BringToFront();
            _toolBar.Show();

            Application.DoEvents();
        }

        public void ShowAppBars()
        {
            if (Handle == IntPtr.Zero)
                CreateHandle();

            try
            {
                if (!_appBarsReady.IsSet)
                    if (!_appBarsReady.Wait(TimeSpan.FromSeconds(1), _cancellationTokenSource.Token))
                        return;
            }
            catch { }

            foreach (var appBar in _appBars.Where(x => !x.Visible))
                appBar.Show(this);

            Timer_Tick(null);

            var threadId = NativeMethods.GetCurrentThreadId();
            Task.Factory.StartNew(() =>
            {
                var list = new List<IntPtr>();
                NativeMethods.EnumDesktopWindows(NativeMethods.GetThreadDesktop(threadId), (hWnd, lParam) =>
                {
                    list.Add(hWnd);
                    return true;
                }, IntPtr.Zero);

                Parallel.ForEach(_appBars, new ParallelOptions
                {
                    MaxDegreeOfParallelism = _appBars.Count,
                    CancellationToken = _cancellationTokenSource.Token,
                    TaskScheduler = TaskScheduler.Default
                }, appBar =>
                {
                    foreach (var hwnd in list)
                        appBar.AddOrUpdateWindow(hwnd);
                    if (appBar.IsLocked)
                    {
                        appBar.IsLocked = false;
                        if (appBar.IsFullscreen)
                            appBar.SendToBack();
                    }
                });
            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        public void HideAppBars(bool close = false)
        {
            foreach (var appBar in _appBars)
            {
                if (appBar.Visible)
                    appBar.Hide();
                if (close)
                    appBar.Close();
            }
        }

        public void BringToFront()
        {
            foreach (var appBar in _appBars.Where(x => x.Visible && !x.IsFullscreen))
                appBar.BringToFront();
        }

        public void SendToBack()
        {
            foreach (var appBar in _appBars.Where(x => x.Visible))
                appBar.SendToBack();
        }

        internal void RefreshAppBars(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!_appBarsReady.IsSet)
                        if (!_appBarsReady.Wait(TimeSpan.FromSeconds(1), _cancellationTokenSource.Token))
                            return;
                }
                catch { }

                if (Screen.AllScreens.Length != _appBars.Count)
                {
                    _appBarsReady.Reset();
                    foreach (var appBar in _appBars)
                    {
                        try
                        {
                            if (appBar.Visible)
                                appBar.Hide();
                            appBar.Close();
                            appBar.Dispose();
                        }
                        catch { }
                    }

                    _appBars.Clear();
                    _appBars.AddRange(Screen.AllScreens.Select(x => new KprSecureDesktopAppBar(x)));
                    _appBarsReady.Set();

                    if (SecureDesktop.Instance.IsActive)
                        SecureDesktop.Instance.Run(_ => SecureDesktop.CompletedTask, true);
                }
                else
                    foreach (var appBar in _appBars)
                        appBar.UpdateAppBar();
            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void Timer_Tick(object state)
        {
            var now = DateTime.Now;

            try
            {
                foreach (var appBar in _appBars.Where(x => x.Visible))
                    appBar.UpdateDateTime(now);
            }
            catch { }
        }

        private const int SPI_SETMINIMIZEDMETRICS = 0x002C;

        [DllImport("KeePassRDP.unmanaged.dll", EntryPoint = "KprSetCbtHwnd", SetLastError = false)]
        private static extern int KprSetCbtHwnd([In] IntPtr hwnd);

        private void RegisterEvents()
        {
            SystemEvents.DisplaySettingsChanged += RefreshAppBars;
            //KprSetCbtHwnd(Handle);

            if (_shellHookMessage == 0)
            {
                ShellDDEInit(true);
                SetTaskmanWindow(Handle);
                //SetProgmanWindow(Handle);
                SetShellWindow(Handle);

                if (_taskBarCreatedMessage == 0)
                    _taskBarCreatedMessage = NativeMethods.RegisterWindowMessage("TaskbarCreated");

                if (RegisterShellHookWindow(Handle))
                {
                    _shellHookMessage = NativeMethods.RegisterWindowMessage("SHELLHOOK");

                    var mm = new MINIMIZEDMETRICS
                    {
                        cbSize = Marshal.SizeOf(typeof(MINIMIZEDMETRICS))
                    };

                    var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(mm));

                    try
                    {
                        const int SPI_GETMINIMIZEDMETRICS = 0x002B;
                        const int ARW_HIDE = 8;

                        Marshal.StructureToPtr(mm, ptr, false);
                        if (!SystemParametersInfo(SPI_GETMINIMIZEDMETRICS, mm.cbSize, ptr, 0))
                            throw new Win32Exception(Marshal.GetLastWin32Error());

                        mm = (MINIMIZEDMETRICS)Marshal.PtrToStructure(ptr, typeof(MINIMIZEDMETRICS));

                        if ((mm.iArrange & ARW_HIDE) == 0)
                        {
                            _mmOrg = mm;

                            mm = new MINIMIZEDMETRICS
                            {
                                cbSize = Marshal.SizeOf(typeof(MINIMIZEDMETRICS)),
                                iArrange = mm.iArrange | ARW_HIDE,
                                iHorzGap = mm.iHorzGap,
                                iVertGap = mm.iVertGap,
                                iWidth = mm.iWidth
                            };

                            Marshal.StructureToPtr(mm, ptr, false);
                            if (!SystemParametersInfo(SPI_SETMINIMIZEDMETRICS, mm.cbSize, ptr, 0))
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }
        }

        private void UnregisterEvents()
        {
            SystemEvents.DisplaySettingsChanged -= RefreshAppBars;
            //KprSetCbtHwnd(IntPtr.Zero);

            if (_shellHookMessage != 0)
            {
                DeregisterShellHookWindow(Handle);

                if (_mmOrg.HasValue)
                {
                    var mm = _mmOrg.Value;
                    var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(mm));
                    try
                    {
                        Marshal.StructureToPtr(mm, ptr, false);
                        SystemParametersInfo(SPI_SETMINIMIZEDMETRICS, mm.cbSize, ptr, 0);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                        _mmOrg = null;
                    }
                }

                _shellHookMessage = 0;
            }

            SetShellWindow(IntPtr.Zero);
            //SetProgmanWindow(IntPtr.Zero);
            SetTaskmanWindow(IntPtr.Zero);
            ShellDDEInit(false);
        }

        protected override void OnHandleChange()
        {
            base.OnHandleChange();

            if (Handle == IntPtr.Zero)
                return;

            RegisterEvents();

            if (!_appBarsReady.IsSet)
            {
                _appBars.AddRange(Screen.AllScreens.Select(x => new KprSecureDesktopAppBar(x)));
                _appBarsReady.Set();

                NativeMethods.PostMessage(NativeMethods.HWND_BROADCAST, _taskBarCreatedMessage, 0, 0);
            }
        }

        public void Minimize(IntPtr hwnd)
        {
            foreach (var appBar in _appBars)
                appBar.Minimize(hwnd);
        }

        public void Restore(IntPtr hwnd)
        {
            foreach (var appBar in _appBars)
                appBar.Restore(hwnd);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == _shellHookMessage)
            {
                if (!_appBarsReady.IsSet)
                    return;

                switch ((ShellEvents)m.WParam.ToInt32())
                {
                    case ShellEvents.HSHELL_REDRAW:
                    case ShellEvents.HSHELL_WINDOWREPLACING:
                    case ShellEvents.HSHELL_WINDOWCREATED:
                        foreach (var appBar in _appBars)
                            appBar.AddOrUpdateWindow(m.LParam);
                        break;
                    case ShellEvents.HSHELL_ENDTASK:
                    case ShellEvents.HSHELL_WINDOWREPLACED:
                    case ShellEvents.HSHELL_WINDOWDESTROYED:
                        foreach (var appBar in _appBars)
                            appBar.RemoveWindow(m.LParam);
                        break;
                    case ShellEvents.HSHELL_RUDEAPPACTIVATED:
                    case ShellEvents.HSHELL_WINDOWACTIVATED:
                        foreach (var appBar in _appBars)
                            appBar.SetActive(m.LParam);
                        break;
                    case ShellEvents.HSHELL_FLASH:
                        foreach (var appBar in _appBars)
                            appBar.SetFlashing(m.LParam);
                        break;
                    case ShellEvents.HSHELL_GETMINRECT:
                        var info = (SHELLHOOKINFO)Marshal.PtrToStructure(m.LParam, typeof(SHELLHOOKINFO));
                        var hwnd = info.hWnd;
                        var screen = Screen.FromHandle(hwnd);
                        var aB = _appBars.FirstOrDefault(x => x.Screen.Equals(screen));
                        if (aB != null)
                        {
                            var sr = aB.GetMinRect(hwnd);

                            info.rc = new RECT
                            {
                                left = sr.Left,
                                top = sr.Top,
                                right = sr.Right,
                                bottom = sr.Bottom
                            };

                            /*info.rc = new SRECT
                            {
                                left = Convert.ToInt16(sr.Left),
                                top = Convert.ToInt16(sr.Top),
                                right = Convert.ToInt16(sr.Right),
                                bottom = Convert.ToInt16(sr.Bottom)
                            };*/

                            Marshal.StructureToPtr(info, m.LParam, false);

                            m.Result = new IntPtr(1);
                        }
                        break;
                    case ShellEvents.HSHELL_ENTERFULLSCREEN:
                        foreach (var appBar in _appBars)
                            appBar.EnterFullscreen(m.LParam);
                        break;
                    case ShellEvents.HSHELL_EXITFULLSCREEN:
                        foreach (var appBar in _appBars)
                            appBar.ExitFullscreen(m.LParam);
                        break;
                    case ShellEvents.HSHELL_MONITORCHANGED:
                        foreach (var appBar in _appBars)
                            appBar.AddOrUpdateWindow(m.LParam);
                        break;
                }
            }

            /*const int SW_MINIMIZE = 6;

            switch (m.Msg)
            {
                case NativeMethods.WM_USER + 1:
                    if (m.LParam.ToInt32() == SW_MINIMIZE)
                    {
                        var hwnd = m.WParam;
                        var screen = Screen.FromHandle(hwnd);
                        var aB = _appBars.FirstOrDefault(x => x.Screen.Equals(screen));
                        if (aB != null)
                        {
                            var sr = aB.GetMinRect(hwnd);

                            var placement = new WINDOWPLACEMENT
                            {
                                length = Marshal.SizeOf(typeof(WINDOWPLACEMENT))
                            };
                            if (GetWindowPlacement(hwnd, ref placement))
                            {
                                const int WPF_SETMINPOSITION = 0x0001;
                                placement.flags |= WPF_SETMINPOSITION;
                                placement.minPosition = new POINT
                                {
                                    x = sr.X,
                                    y = sr.Y
                                };
                                SetWindowPlacement(hwnd, ref placement);
                            }
                        }
                    }
                    break;
            }*/

            base.WndProc(ref m);
        }

        private bool GetInvokeRequired(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return false;

            uint pid;
            var hwndThread = NativeMethods.GetWindowThreadProcessId(hWnd, out pid);
            var currentThread = NativeMethods.GetCurrentThreadId();

            return hwndThread != currentThread;
        }

        private void DestroyWindow(bool destroyHwnd, IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                hWnd = Handle;

            if (GetInvokeRequired(hWnd))
            {
                PostMessage(new HandleRef(this, hWnd), NativeMethods.WM_CLOSE, 0, 0);
                return;
            }

            if (destroyHwnd)
                lock (this)
                    base.DestroyHandle();
        }

        public override void DestroyHandle()
        {
            UnregisterEvents();

            DestroyWindow(false, IntPtr.Zero);
            base.DestroyHandle();
        }

        public void Dispose()
        {
            DestroyHandle();

            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();

            _timer.Dispose();

            _appBarsReady.Dispose();

            foreach (var appBar in _appBars)
            {
                if (appBar.Visible)
                    appBar.Hide();
                appBar.Dispose();
            }

            _appBars.Clear();

            _lockScreen.Dispose();
            _toolBar.MinimizeStart -= Minimize;
            _toolBar.MinimizeEnd -= Restore;
            _toolBar.VisibleChanged -= ToolBar_VisibleChanged;
            _toolBar.FormClosed -= ToolBar_FormClosed;
            _toolBar.Dispose();
            _emptyWindow.Dispose();
            _desktopWindow.Dispose();
            _cancellationTokenSource.Dispose();
        }

        internal class EmptyWindow : NativeWindow, IDisposable
        {
            public EmptyWindow()
            {
                CreateHandle(new CreateParams
                {
                    ClassName = "Message",
                    Style = 0,
                    ExStyle = NativeMethods.WS_EX_NOREDIRECTIONBITMAP | NativeMethods.WS_EX_NOINHERITLAYOUT | NativeMethods.WS_EX_NOPARENTNOTIFY | NativeMethods.WS_EX_NOACTIVATE,
                    ClassStyle = NativeMethods.CS_PARENTDC,
                    Caption = string.Empty,
                    Parent = NativeMethods.HwndMessage,
                    Height = 0,
                    Width = 0,
                    X = 0,
                    Y = 0
                });
            }

            public void Dispose()
            {
                DestroyHandle();
            }
        }

        internal class DesktopWindow : NativeWindow, IDisposable
        {
            private readonly ManualResetEventSlim _bgLock;

            public DesktopWindow()
            {
                _bgLock = new ManualResetEventSlim(false);

                var rect = SystemInformation.VirtualScreen;

                CreateHandle(new CreateParams
                {
                    ClassName = "#32769",
                    Style = NativeMethods.WS_VISIBLE | NativeMethods.WS_POPUP,
                    ExStyle = NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOINHERITLAYOUT | NativeMethods.WS_EX_NOPARENTNOTIFY,
                    ClassStyle = NativeMethods.CS_PARENTDC,
                    Caption = string.Empty,
                    Parent = NativeMethods.GetDesktopWindow(),
                    Height = rect.Height,
                    Width = rect.Width,
                    X = rect.X,
                    Y = rect.Y
                });
            }

            public void BringToFront()
            {
                NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOP, 0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOREDRAW | NativeMethods.SWP_NOACTIVATE);
                Application.DoEvents();
            }

            public void SendToBack()
            {
                NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_BOTTOM, 0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOREDRAW | NativeMethods.SWP_NOACTIVATE);
                Application.DoEvents();
            }

            public void RefreshBackground()
            {
                if (_bgLock.IsSet)
                    return;

                _bgLock.Set();

                var rect = SystemInformation.VirtualScreen;

                var e = Graphics.FromHwnd(Handle);

                e.SmoothingMode = SmoothingMode.None;
                e.PixelOffsetMode = PixelOffsetMode.None;
                e.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.CompositingQuality = CompositingQuality.HighSpeed;

                var b = new Bitmap(rect.Width, rect.Height, e);
                var g = Graphics.FromImage(b);

                g.SetClip(rect, CombineMode.Replace);

                /*g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.CompositingQuality = CompositingQuality.HighQuality;*/

                var c = BufferedGraphicsManager.Current;
                var d = c.Allocate(e, rect);

                var h = g.GetHdc();

                var finish = new Action<IAsyncResult>((result) =>
                {
                    if (result != null)
                        Program.MainForm.EndInvoke(result);

                    using (g)
                    {
                        g.ReleaseHdc(h);
                        using (var brush = new SolidBrush(Color.FromArgb(192, Color.Black)))
                            g.FillRectangle(brush, rect);
                    }

                    using (b)
                        d.Graphics.DrawImage(b, -rect.Left, -rect.Top);

                    using (c)
                    using (d)
                        d.Render(e);

                    using (e)
                        e.Flush();

                    _bgLock.Reset();
                });

                if (Program.MainForm.InvokeRequired)
                    Task.Factory.FromAsync(
                        Program.MainForm.BeginInvoke(new Action(() => NativeMethods.PaintDesktop(h))),
                        finish,
                        TaskCreationOptions.AttachedToParent,
                        TaskScheduler.FromCurrentSynchronizationContext());
                else
                    finish(null);
            }

            public void Dispose()
            {
                DestroyHandle();
                _bgLock.Dispose();
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                const int WM_ACTIVATE = 0x0006;
                /*const int WM_SETFOCUS = 0x0007;
                const int WM_KILLFOCUS = 0x0008;
                const int WM_SHOWWINDOW = 0x0018;
                const int WM_CHILDACTIVATE = 0x0022;
                const int WM_NCACTIVATE = 0x0086;*/
                const int WM_ACTIVATEAPP = 0x001C;
                const int WM_ERASEBKGND = 0x0014;
                switch (m.Msg)
                {
                    /*case WM_SETFOCUS:
                    case WM_KILLFOCUS:
                    case WM_SHOWWINDOW:
                    case WM_CHILDACTIVATE:
                    case WM_NCACTIVATE:
                    case WM_ACTIVATEAPP:*/
                    //case WM_NCACTIVATE:
                    case WM_ACTIVATE:
                    case WM_ACTIVATEAPP:
                        SendToBack();
                        break;
                    case NativeMethods.WM_WINDOWPOSCHANGING:
                        var winpos = (NativeMethods.WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.WINDOWPOS));
                        winpos.hwndInsertAfter = NativeMethods.HWND_BOTTOM;
                        Marshal.StructureToPtr(winpos, m.LParam, false);
                        break;
                    case WM_ERASEBKGND:
                        RefreshBackground();
                        m.Result = (IntPtr)1;
                        break;
                }
            }
        }

        private enum ShellEvents : int
        {
            HSHELL_WINDOWCREATED = 1,
            HSHELL_WINDOWDESTROYED = 2,
            HSHELL_ACTIVATESHELLWINDOW = 3,
            HSHELL_WINDOWACTIVATED = 4,
            HSHELL_GETMINRECT = 5,
            HSHELL_REDRAW = 6,
            HSHELL_TASKMAN = 7,
            HSHELL_LANGUAGE = 8,
            HSHELL_SYSMENU = 9,
            HSHELL_ENDTASK = 10,
            HSHELL_ACCESSIBILITYSTATE = 11,
            HSHELL_APPCOMMAND = 12,
            HSHELL_WINDOWREPLACED = 13,
            HSHELL_WINDOWREPLACING = 14,
            HSHELL_MONITORCHANGED = 16,
            HSHELL_ENTERFULLSCREEN = 53, // undocumented
            HSHELL_EXITFULLSCREEN = 54, // undocumented
            HSHELL_HIGHBIT = 0x8000,
            HSHELL_FLASH = HSHELL_REDRAW | HSHELL_HIGHBIT,
            HSHELL_RUDEAPPACTIVATED = HSHELL_WINDOWACTIVATED | HSHELL_HIGHBIT
        }

        [DllImport("User32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "DeregisterShellHookWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeregisterShellHookWindow(IntPtr hWnd);

        [DllImport("User32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "RegisterShellHookWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterShellHookWindow(IntPtr hWnd);

        /*[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "#181")]
        private static extern bool RegisterShellHook(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] ShellHookType flags);

        private enum ShellHookType : uint
        {
            RSH_UNREGISTER = 0,
            RSH_REGISTER = 1,
            RSH_REGISTER_PROGMAN = 2,
            RSH_REGISTER_TASKMAN = 3
        }*/

        [DllImport("User32.dll", SetLastError = false, ExactSpelling = true, EntryPoint = "SetTaskmanWindow")]
        private static extern void SetTaskmanWindow(IntPtr hwnd);

        [DllImport("User32.dll", SetLastError = false, ExactSpelling = true, EntryPoint = "SetProgmanWindow")]
        private static extern void SetProgmanWindow(IntPtr hwnd);

        [DllImport("User32.dll", SetLastError = false, ExactSpelling = true, EntryPoint = "SetShellWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetShellWindow(IntPtr hwnd);

        [DllImport("User32.dll", SetLastError = true, EntryPoint = "SystemParametersInfoW")]
        private static extern bool SystemParametersInfo(int uiAction, int uiParam, IntPtr pvParam, int fWinIni);

        [DllImport("Shell32.dll", SetLastError = false, EntryPoint = "#188")]
        private static extern void ShellDDEInit([In][MarshalAs(UnmanagedType.Bool)] bool init);

        [StructLayout(LayoutKind.Sequential)]
        private struct MINIMIZEDMETRICS
        {
            public int cbSize;
            public int iWidth;
            public int iHorzGap;
            public int iVertGap;
            public int iArrange;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SHELLHOOKINFO
        {
            public IntPtr hWnd;
            public RECT rc;
        }

        /*[StructLayout(LayoutKind.Sequential)]
        private struct SRECT
        {
            public short left;
            public short top;
            public short right;
            public short bottom;
        }*/

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}