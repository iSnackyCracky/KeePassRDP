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
using KeePass.UI;
using KeePassLib.Utility;
using KeePassRDP.Extensions;
using KeePassRDP.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace KeePassRDP
{
    public partial class KprSecureDesktopToolBar : Form, IMessageFilter
    {
        private static readonly List<KprSecureDesktopToolBar> _instances = new List<KprSecureDesktopToolBar>();
        private static readonly object _lock = new object();

        private static readonly Lazy<Image> _smallIcon = new Lazy<Image>(() =>
        {
            using (var icon = IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, UIUtil.GetSmallIconSize().Height))
                return icon.ToBitmap();
        });

        private static readonly Lazy<Image> _smallIconSecure = new Lazy<Image>(() =>
        {
            //var bmp = (Bitmap)_smallIcon.Value.Clone();
            using (var icon = IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, UIUtil.GetSmallIconSize().Height))
            {
                var bmp = icon.ToBitmap();
                var widthScaled = bmp.Width * 0.67f;
                var heightScaled = bmp.Height * 0.67f;
                using (var shield = SystemIcons.Shield)
                using (var bmpShield = shield.ToBitmap())
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.DrawImage(
                        bmpShield,
                        new RectangleF(bmp.Width - widthScaled + 1, bmp.Height - heightScaled, widthScaled, heightScaled),
                        new RectangleF(0, 0, bmpShield.Width, bmpShield.Height),
                        GraphicsUnit.Pixel);
                }
                return bmp;
            }
        });

        private static readonly Lazy<Image> _largeIconSecure = new Lazy<Image>(() =>
        {
            using (var icon = IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, UIUtil.GetIconSize().Height))
            {
                var bmp = icon.ToBitmap();
                var widthScaled = bmp.Width * 0.67f;
                var heightScaled = bmp.Height * 0.67f;
                using (var shield = SystemIcons.Shield)
                using (var bmpShield = shield.ToBitmap())
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.DrawImage(
                        bmpShield,
                        new RectangleF(bmp.Width - widthScaled + 1, bmp.Height - heightScaled, widthScaled, heightScaled),
                        new RectangleF(0, 0, bmpShield.Width, bmpShield.Height),
                        GraphicsUnit.Pixel);
                }
                return bmp;
            }
        });

        private static volatile bool _setAutoSwitch = false;
        private static volatile bool _tryAutoSwitch = false;
        //private readonly LowLevelKeyboardProc _proc;

        private readonly bool _secure;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ManualResetEventSlim _dropDownLock;
        private readonly HashSet<Control> _controls;
        private readonly Timer _tooltipTimer;
        private NativeMethods.WinEventDelegate _eventCallback;
        //private IntPtr _hookId;
        private IntPtr _powerHandle;
        private IntPtr _switchHook;
        private IntPtr _foregroundHook;
        private IntPtr _focusHook;
        private IntPtr _minimizeHook;
        private Point? _lastTooltipMousePosition;
        private volatile bool _isResizing;

        private bool _aeroEnabled;

        public delegate void MinimizeEvent(IntPtr hWnd);

        public event MinimizeEvent MinimizeStart;
        public event MinimizeEvent MinimizeEnd;

        public KprSecureDesktopToolBar(ToolStripRenderer tsr, bool secure = false)
        {
            lock (_lock)
                _instances.Add(this);

            _secure = secure;
            _cancellationTokenSource = new CancellationTokenSource();
            _dropDownLock = new ManualResetEventSlim(false);
            _tooltipTimer = new Timer
            {
                Interval = 500,
                Enabled = false
            };
            if (secure)
            {
                //_proc = new LowLevelKeyboardProc(HookCallback);
            }
            _eventCallback = null;
            //_hookId = IntPtr.Zero;
            _powerHandle = IntPtr.Zero;
            _switchHook = IntPtr.Zero;
            _foregroundHook = IntPtr.Zero;
            _focusHook = IntPtr.Zero;
            _minimizeHook = IntPtr.Zero;
            _lastTooltipMousePosition = null;
            _isResizing = false;

            InitializeComponent();

            SuspendLayout();

            Visible = false;
            TopLevel = true;

            toolStrip1.Renderer = tsr;
            toolStrip1.ImageList = kprImageList;
            toolStripButton1.ImageKey = "SwitchSourceOrTarget";

            toolStripLabel1.Control.SetStyle(ControlStyles.SupportsTransparentBackColor);
            toolStripLabel1.Control.BackColor = Color.Transparent;

            var label = new Label
            {
                ImageAlign = ContentAlignment.MiddleCenter,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                AutoSize = false,
                BackColor = Color.Transparent,
                Location = new Point(4, -1)
            };

            if (secure)
            {
                label.Image = _smallIconSecure.Value;
                toolStripButton1.ToolTipText = KprResourceManager.Instance["Switch to default desktop"];

                var tsddb = new KprToolStripDropDownButton
                {
                    DropDown = new ToolStripDropDown
                    {
                        AutoSize = true,
                        Dock = DockStyle.Fill,
                        LayoutStyle = ToolStripLayoutStyle.Table,
                        ShowItemToolTips = false,
                        DropShadowEnabled = false,
                        AutoClose = true,
                        Renderer = toolStrip1.Renderer,
                        Margin = Padding.Empty,
                        Padding = new Padding(1),
                        DefaultDropDownDirection = ToolStripDropDownDirection.BelowRight
                    },
                    ShowDropDownArrow = false,
                    Padding = new Padding(2, 0, 2, 0),
                    Margin = new Padding(1, 1, 0, 2),
                    DropDownDirection = ToolStripDropDownDirection.BelowRight,
                    ImageKey = "ListMembers",
                    DisplayStyle = ToolStripItemDisplayStyle.Image,
                    ToolTipText = KprResourceManager.Instance["Running processes"],
                    AutoSize = true
                };
                var tls = tsddb.DropDown.LayoutSettings as TableLayoutSettings;
                tls.ColumnCount = 1;
                tls.RowCount = 1;
                tls.ColumnStyles.Clear();
                tls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                tls.RowStyles.Clear();
                tls.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tls.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
                tsddb.DropDown.GetType().GetProperty("WorkingAreaConstrained", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(tsddb.DropDown, false, null);
                tsddb.DropDown.Closing += ToolStripDropDown_Closing;
                tsddb.DropDown.Opening += ToolStripDropDown_Opening;
                tsddb.MouseEnter += ToolStripItem_MouseEnter;
                tsddb.MouseLeave += ToolStripItem_MouseLeave;
                tsddb.MouseMove += ToolStripItem_MouseMove;
                tsddb.Click += ToolStripDropDownButton_Click;
                toolStrip1.Items.Add(tsddb);
            }
            else
            {
                label.Image = _smallIcon.Value;
                toolStripButton1.ToolTipText = KprResourceManager.Instance["Switch to secure desktop"];
            }

            label.Size = label.Image.Size;
            toolStripLabel1.AutoSize = false;
            toolStripLabel1.Width += label.Image.Width;
            toolStripLabel1.Control.Controls.Add(label);
            toolStripLabel1.Control.Cursor = Cursors.Arrow;

            _controls = new HashSet<Control>
            {
                toolStripLabel1.Control,
                label
            };

            ResumeLayout(false);

            Application.AddMessageFilter(this);
        }

        ~KprSecureDesktopToolBar()
        {
            lock (_lock)
                _instances.Remove(this);

            Application.RemoveMessageFilter(this);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;

                var cp = base.CreateParams;

                cp.ExStyle |= NativeMethods.WS_EX_TOOLWINDOW;
                cp.ExStyle &= ~NativeMethods.WS_EX_APPWINDOW;

                NativeMethods.DwmIsCompositionEnabled(ref _aeroEnabled);
                if (!_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        public new void BringToFront()
        {
            base.BringToFront();
            TopMost = true;
        }

        public new void SendToBack()
        {
            TopMost = false;
            base.SendToBack();
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (!_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource.Cancel();

                UnregisterEvents();
                try
                {
                    var label = toolStripLabel1.Control.Controls.Count > 1 ? toolStripLabel1.Control.Controls[toolStripLabel1.Control.Controls.Count - 1] as Label : null;
                    if (label != null)
                    {
                        label.Image = null;
                        label.Dispose();
                    }

                    var tsddb = toolStrip1.Items.Count > 2 ? toolStrip1.Items[2] as ToolStripDropDownButton : null;
                    if (tsddb != null)
                    {
                        tsddb.DropDown.Closing -= ToolStripDropDown_Closing;
                        tsddb.DropDown.Opening -= ToolStripDropDown_Opening;
                        tsddb.DropDown.Dispose();
                        tsddb.MouseEnter -= ToolStripItem_MouseEnter;
                        tsddb.MouseLeave -= ToolStripItem_MouseLeave;
                        tsddb.MouseMove -= ToolStripItem_MouseMove;
                        tsddb.Click -= ToolStripDropDownButton_Click;
                        tsddb.Dispose();
                    }
                    for (var i = toolStrip1.Items.Count - 1; i > 2; i--)
                    {
                        var tsi = toolStrip1.Items[i];
                        if (tsi.Image != null)
                        {
                            tsi.Image.Dispose();
                            tsi.Image = null;
                        }
                        tsi.MouseEnter -= ToolStripItem_MouseEnter;
                        tsi.MouseLeave -= ToolStripItem_MouseLeave;
                        tsi.MouseMove -= ToolStripItem_MouseMove;
                        tsi.Click -= ToolStripButton_Click;
                        tsi.Dispose();
                    }
                }
                catch { }

                toolStrip1.Items.Clear();

                _dropDownLock.Dispose();
                _cancellationTokenSource.Dispose();

                lock (_lock)
                    _instances.Remove(this);

                Application.RemoveMessageFilter(this);
            }

            base.Dispose(disposing);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (Visible && e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;

            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            if (_secure)
                _tryAutoSwitch = false;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (_secure)
                NativeMethods.SetWindowDisplayAffinity(Handle, NativeMethods.WDA_EXCLUDEFROMCAPTURE);

            RegisterEvents();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnregisterEvents();

            base.OnHandleDestroyed(e);
        }

        private void RegisterEvents()
        {
            if (_eventCallback == null)
                _eventCallback = new NativeMethods.WinEventDelegate(WindowEventCallback);

            if (_secure)
            {
                /*const int WH_KEYBOARD_LL = 13;
                if (_hookId == IntPtr.Zero)
                    _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, IntPtr.Zero, 0);*/

                if (_powerHandle == IntPtr.Zero)
                    _powerHandle = RegisterPowerSettingNotification(Handle, ref PowerSettingGuid.SessionDisplayStatus, DEVICE_NOTIFY.DEVICE_NOTIFY_WINDOW_HANDLE);

                if (_minimizeHook == IntPtr.Zero)
                    _minimizeHook = NativeMethods.SetWinEventHook(
                        NativeMethods.WINEVENT_EVENTS.EVENT_SYSTEM_MINIMIZESTART,
                        NativeMethods.WINEVENT_EVENTS.EVENT_SYSTEM_MINIMIZEEND,
                        IntPtr.Zero,
                        _eventCallback,
                        0,
                        0,
                        NativeMethods.WINEVENT_FLAGS.WINEVENT_OUTOFCONTEXT);
            }
            else
            {
                if (_switchHook == IntPtr.Zero)
                    _switchHook = NativeMethods.SetWinEventHook(
                        NativeMethods.WINEVENT_EVENTS.EVENT_SYSTEM_DESKTOPSWITCH,
                        NativeMethods.WINEVENT_EVENTS.EVENT_SYSTEM_DESKTOPSWITCH,
                        IntPtr.Zero,
                        _eventCallback,
                        0,
                        0,
                        NativeMethods.WINEVENT_FLAGS.WINEVENT_OUTOFCONTEXT | NativeMethods.WINEVENT_FLAGS.WINEVENT_SKIPOWNPROCESS);

                if (_foregroundHook == IntPtr.Zero)
                    _foregroundHook = NativeMethods.SetWinEventHook(
                        NativeMethods.WINEVENT_EVENTS.EVENT_SYSTEM_FOREGROUND,
                        NativeMethods.WINEVENT_EVENTS.EVENT_SYSTEM_FOREGROUND,
                        IntPtr.Zero,
                        _eventCallback,
                        0,
                        0,
                        NativeMethods.WINEVENT_FLAGS.WINEVENT_OUTOFCONTEXT);

                if (_focusHook == IntPtr.Zero)
                    _focusHook = NativeMethods.SetWinEventHook(
                        NativeMethods.WINEVENT_EVENTS.EVENT_OBJECT_FOCUS,
                        NativeMethods.WINEVENT_EVENTS.EVENT_OBJECT_FOCUS,
                        IntPtr.Zero,
                        _eventCallback,
                        0,
                        0,
                        NativeMethods.WINEVENT_FLAGS.WINEVENT_OUTOFCONTEXT);
            }
        }

        private void UnregisterEvents()
        {
            /*if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }*/

            if (_powerHandle != IntPtr.Zero)
            {
                UnregisterPowerSettingNotification(_powerHandle);
                _powerHandle = IntPtr.Zero;
            }

            if (_switchHook != IntPtr.Zero)
            {
                NativeMethods.UnhookWinEvent(_switchHook);
                _switchHook = IntPtr.Zero;
            }

            if (_foregroundHook != IntPtr.Zero)
            {
                NativeMethods.UnhookWinEvent(_foregroundHook);
                _foregroundHook = IntPtr.Zero;
            }

            if (_focusHook != IntPtr.Zero)
            {
                NativeMethods.UnhookWinEvent(_focusHook);
                _focusHook = IntPtr.Zero;
            }

            if (_minimizeHook != IntPtr.Zero)
            {
                NativeMethods.UnhookWinEvent(_minimizeHook);
                _minimizeHook = IntPtr.Zero;
            }

            _eventCallback = null;
        }

        private void KprSecureDesktopToolBar_Load(object sender, EventArgs e)
        {
            lock (_lock)
                if (_instances.Count > 1 && _instances.IndexOf(this) > 0)
                    Location = _instances[0].Location;

            toolStrip1.SuspendLayout();
            toolStrip1.LockHeight(true);

            if (_secure)
            {
                //var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                //Task.Factory.StartNew(() =>
                {
                    var iconSize = UIUtil.GetSmallIconSize().Height;
                    var sysDir = Environment.SystemDirectory;
                    var items = new[]
                    {
                        new Tuple<string, string>("Task-Manager", string.Format("{0}#{1}", Path.Combine(sysDir, "cmd.exe"), Path.Combine(sysDir, "Taskmgr.exe"))),
                        new Tuple<string, string>("Snipping Tool", Path.Combine(sysDir, "SnippingTool.exe")),
                        new Tuple<string, string>("Editor", Path.Combine(sysDir, "notepad.exe")),
                        new Tuple<string, string>(string.Empty, string.Empty)
                    }
                    //.Select(x => Environment.ExpandEnvironmentVariables(x))
                    .Where(x => string.IsNullOrEmpty(x.Item2) || File.Exists(x.Item2.Split(new[] { '#' }, 2).FirstOrDefault()));

                    //Task.Factory.FromAsync(base.BeginInvoke(new Action(() =>
                    {
                        toolStrip1.Items.Add(new ToolStripSeparator
                        {
                            Margin = new Padding(2, 0, 3, 0)
                        });
                        toolStrip1.Items.AddRange(items.Select(x =>
                        {
                            var filename = x.Item2.Split(new[] { '#' }, 2).LastOrDefault();
                            var text = !string.IsNullOrEmpty(x.Item1) ? x.Item1 : Path.GetFileName(filename);
                            var ttext = string.IsNullOrEmpty(text) ? KprResourceManager.Instance["Run"] : string.Format(KprResourceManager.Instance["Run {0}"], text).Trim();

                            var tb = new ToolStripButton
                            {
                                Tag = x.Item2,
                                Text = string.IsNullOrEmpty(text) ? "Run" : text,
                                DisplayStyle = ToolStripItemDisplayStyle.Image,
                                ImageScaling = ToolStripItemImageScaling.SizeToFit,
                                ImageAlign = ContentAlignment.MiddleCenter,
                                TextAlign = ContentAlignment.MiddleCenter,
                                AutoSize = true,
                                Dock = DockStyle.Fill,
                                AutoToolTip = false,
                                ToolTipText = ttext
                            };

                            var iconIdx = 0;

                            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(filename))
                            {
                                filename = Path.Combine(sysDir, "imageres.dll");
                                iconIdx = 95;
                            }

                            if (!string.IsNullOrEmpty(filename))
                            {
                                using (var icon = IconUtil.ExtractIcon(filename, iconIdx, iconSize))
                                    if (icon != null)
                                    {
                                        if (icon.Height < iconSize)
                                            using (var bmp = icon.ToBitmap())
                                                tb.Image = GfxUtil.ScaleImage(bmp, iconSize, iconSize);
                                        else
                                            tb.Image = icon.ToBitmap();
                                    }
                                    else
                                        tb.DisplayStyle = ToolStripItemDisplayStyle.Text;
                            }
                            else
                                tb.DisplayStyle = ToolStripItemDisplayStyle.Text;

                            tb.MouseEnter += ToolStripItem_MouseEnter;
                            tb.MouseLeave += ToolStripItem_MouseLeave;
                            tb.MouseMove += ToolStripItem_MouseMove;
                            tb.Click += ToolStripButton_Click;

                            return tb;
                        }).ToArray());

                        toolStrip1.ResumeLayout(false);
                    }//)), endInvoke => base.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, taskScheduler);
                }//, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }
            else
            {
                toolStrip1.ResumeLayout(false);
                if (!Visible)
                    Show();
            }
        }

        private void KprSecureDesktopToolBar_Shown(object sender, EventArgs e)
        {
            Activate();
            KprSecureDesktopToolBar_Activated(null, EventArgs.Empty);

            if (_secure)
            {
                Select();
                Focus();
            }
        }

        private void KprSecureDesktopToolBar_Deactivate(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            if (toolStrip1.Items.Count > 2 && toolStrip1.Items[2] is ToolStripDropDownItem)
                (toolStrip1.Items[2] as ToolStripDropDownItem).HideDropDown();

            if (TopMost)
                BringToFront();

            Opacity = .5;
        }

        /*[DllImport("User32.dll")]
        private static extern IntPtr GetForegroundWindow();*/

        private void KprSecureDesktopToolBar_Activated(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            if (_tryAutoSwitch)
            {
                if (!_secure)
                {
                    //if (GetForegroundWindow() == Handle)
                    {
                        _tryAutoSwitch = false;

                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                var inputDesktop = SecureDesktop.GetInputDesktopName();

                                if (string.IsNullOrEmpty(inputDesktop))
                                {
                                    _tryAutoSwitch = true;
                                    return;
                                }

                                if (SecureDesktop.IsValueCreated &&!SecureDesktop.Instance.IsActive)
                                    Task.Factory.FromAsync(Program.MainForm.BeginInvoke(new Action(() =>
                                    {
                                        try
                                        {
                                            if (SecureDesktop.IsInput(NativeMethods.GetThreadDesktop(NativeMethods.GetCurrentThreadId())))
                                                SecureDesktop.Instance.SwitchDesktop();
                                        }
                                        catch { }
                                    })), endInvoke => Program.MainForm.EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                                else
                                    _tryAutoSwitch = false;
                            }
                            catch { }
                        }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }

                    return;
                }

                if (SecureDesktop.IsValueCreated && SecureDesktop.Instance.IsActive)
                    _tryAutoSwitch = false;
            }

            BringToFront();
            Opacity = 1;

            if (_secure)
            {
                Select();
                Focus();
            }
        }

        private void KprSecureDesktopToolBar_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            /*if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Q)
            {
                toolStripButton1.PerformClick();
                e.IsInputKey = false;
            }*/
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ToolStripItem_MouseLeave(null, EventArgs.Empty);

            if (SecureDesktop.IsValueCreated)
                SecureDesktop.Instance.SwitchDesktop();
        }

        [Flags]
        private enum RunFileDlgFlags : uint
        {
            NONE = 0,
            RFF_NOBROWSE = 1, // Remove the browse button
            RFF_NODEFAULT = 2, // No default item selected
            RFF_CALCDIRECTORY = 4, // Calculate the working directory from the file name
            RFF_NOLABEL = 8, // Remove the edit box label
            RFF_NOSEPARATEMEM = 14 // Remove the Separate Memory Space check box (Windows NT only)
        }

        [DllImport("Shell32.dll", EntryPoint = "#61", CharSet = CharSet.Unicode, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RunFileDlg([In] IntPtr hwndOwner, [In] IntPtr hIcon, [In] string lpszDirectory, [In] string lpszTitle, [In] string lpszDescription, [In][MarshalAs(UnmanagedType.U4)] RunFileDlgFlags uFlags);

        private void ToolStripButton_Click(object sender, EventArgs e)
        {
            ToolStripItem_MouseLeave(null, EventArgs.Empty);

            var tsi = sender as ToolStripItem;
            var tag = tsi.Tag as string;

            if (string.IsNullOrEmpty(tag))
            {
                RunFileDlg(Handle, (_largeIconSecure.Value as Bitmap).GetHicon(), Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), null, null, RunFileDlgFlags.NONE);
                return;
            }

            Task.Factory.StartNew(() =>
            {
                var split = tag.Split(new[] { '#' }, 2);
                var filename = split.FirstOrDefault();
                var noWindow = false;
                if (split.Length > 1 && string.Equals(Path.Combine(Environment.SystemDirectory, "cmd.exe"), filename, StringComparison.OrdinalIgnoreCase))
                {
                    noWindow = true;
                    split[1] = string.Format("/c start \"\" \"{0}\"", split[1]);
                }

                using (var p = new SecureProcess
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = filename,
                        Arguments = split.Length > 1 ? split.LastOrDefault() : string.Empty,
                        LoadUserProfile = false,
                        ErrorDialog = true,
                        ErrorDialogParentHandle = Handle,
                        WorkingDirectory = Path.GetTempPath(),
                        UseShellExecute = false,
                        CreateNoWindow = noWindow,
                        WindowStyle = noWindow ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
                    }
                })
                    p.StartSecure(false);
            }, _cancellationTokenSource.Token, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void ToolStripDropDownButton_Click(object sender, EventArgs e)
        {
            ToolStripItem_MouseLeave(null, EventArgs.Empty);

            var tsdi = sender as ToolStripDropDownItem;
            var td = tsdi.DropDown;

            if (td.Tag == null)
            {
                tsdi.ShowDropDown();
                td.Tag = true;
            }
            else
            {
                tsdi.HideDropDown();
                td.Tag = null;
            }
        }

        private void ToolStripDropDown_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            var td = sender as ToolStripDropDown;
            if (e.CloseReason == ToolStripDropDownCloseReason.AppClicked && td.OwnerItem.Bounds.Contains(PointToClient(MousePosition)))
                td.Tag = true;
            else
                td.Tag = null;
        }

        public new void Hide()
        {
            base.Hide();

            DestroyHandle();
            if (Container != null)
                Container.Remove(this);
        }

        private object InvokeBase(Delegate method)
        {
            return base.Invoke(method);
        }

        public new object Invoke(Delegate method)
        {
            return Invoke(method, null);
        }

        public new object Invoke(Delegate method, params object[] args)
        {
            return Program.MainForm.InvokeRequired ? Program.MainForm.Invoke(method, args) : base.Invoke(method, args);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public new IAsyncResult BeginInvoke(Delegate method)
        {
            return BeginInvoke(method, null);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public new IAsyncResult BeginInvoke(Delegate method, params object[] args)
        {
            return Program.MainForm.InvokeRequired ? Program.MainForm.BeginInvoke(method, args) : base.BeginInvoke(method, args);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public new object EndInvoke(IAsyncResult asyncResult)
        {
            return Program.MainForm.InvokeRequired ? Program.MainForm.EndInvoke(asyncResult) : base.EndInvoke(asyncResult);
        }

        private void ToolStripDropDown_Opening(object sender, EventArgs e)
        {
            if (_dropDownLock.IsSet)
                return;

            _dropDownLock.Set();

            ToolStripItem_MouseLeave(null, EventArgs.Empty);

            var td = sender as ToolStripDropDown;
            var tsdi = td.OwnerItem as ToolStripDropDownItem;
            var oldButtons = td.Items.OfType<ToolStripButton>().ToList();

            if (td.Items.Count > 0)
            {
                tsdi.HideDropDown();
                td.Items.Clear();
            }

            var dwThreadId = NativeMethods.GetCurrentThreadId();
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Factory.StartNew(() =>
            {
                string[] result = null;
                using (var h = new BlockingCollection<IntPtr>())
                {
                    var l = new List<IntPtr>();
                    Task.Factory.StartNew(() =>
                    {
                        NativeMethods.EnumDesktopWindows(NativeMethods.GetThreadDesktop(dwThreadId), (hWnd, lParam) =>
                        {
                            h.Add(hWnd);
                            l.Add(hWnd);
                            return true;
                        }, IntPtr.Zero);

                        if (!h.IsAddingCompleted)
                            h.CompleteAdding();
                    }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                    var i = new ConcurrentDictionary<int, Tuple<string, string, IntPtr>>(10, 0);
                    Parallel.ForEach(h.GetConsumingEnumerable(), new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 10,
                        TaskScheduler = TaskScheduler.Default,
                        CancellationToken = _cancellationTokenSource.Token
                    }, hWnd =>
                    {
                        uint procId;
                        if (NativeMethods.GetWindowThreadProcessId(hWnd, out procId) == dwThreadId)
                            return;

                        using (var cts1 = new CancellationTokenSource())
                        using (var cts2 = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, _cancellationTokenSource.Token))
                        using (var mrs = new ManualResetEventSlim(false))
                        {
                            Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    using (var proc = Process.GetProcessById(Convert.ToInt32(procId)))
                                    {
                                        var pid = proc.Id;
                                        if (i.ContainsKey(pid))
                                            return;

                                        var mainHandle = proc.MainWindowHandle;
                                        var name = string.Empty;

                                        if (mainHandle == hWnd)
                                        {
                                            var text = proc.MainWindowTitle;
                                            if (string.IsNullOrEmpty(text))
                                            {
                                                var windowTitle = new char[NativeMethods.GetWindowTextLength(hWnd) + 1];
                                                if (NativeMethods.GetWindowText(hWnd, windowTitle, windowTitle.Length) != 0)
                                                    text = new string(windowTitle).TrimEnd(char.MinValue);
                                            }

                                            try
                                            {
                                                using (var main = proc.MainModule)
                                                    name = main != null && !string.IsNullOrEmpty(main.FileName) ? Path.GetFileName(main.FileName) : string.Format("{0}.exe", proc.ProcessName);
                                            }
                                            catch
                                            {
                                                name = string.Format("{0}.exe", proc.ProcessName);
                                            }

                                            if (!string.IsNullOrEmpty(text))
                                                i.TryAdd(pid, new Tuple<string, string, IntPtr>(name, text, mainHandle));
                                            else
                                                i.TryAdd(pid, new Tuple<string, string, IntPtr>(name, string.Empty, mainHandle));
                                        }
                                        else if (mainHandle == IntPtr.Zero)
                                            i.TryAdd(pid, new Tuple<string, string, IntPtr>(name, string.Empty, IntPtr.Zero));
                                    }
                                }
                                finally
                                {
                                    try
                                    {
                                        if (!cts1.IsCancellationRequested)
                                            mrs.Set();
                                    }
                                    catch { }
                                }
                            }, cts2.Token, TaskCreationOptions.AttachedToParent, taskScheduler);

                            try
                            {
                                mrs.Wait(TimeSpan.FromSeconds(1), cts2.Token);

                                if (!cts1.IsCancellationRequested)
                                    cts1.Cancel();
                            }
                            catch { }
                        }
                    });

                    result = i.OrderBy(x => l.IndexOf(x.Value.Item3)).Select(x => string.Format("{0}/{1} - {2}", x.Key, x.Value.Item1, x.Value.Item2)).ToArray();
                }

                if (result != null && result.Length > 0)
                {
                    var items = result.Select(x =>
                    {
                        var tsb = new ToolStripButton
                        {
                            Text = x,
                            DisplayStyle = ToolStripItemDisplayStyle.Text,
                            Padding = new Padding(1, 2, 1, 2),
                            Margin = Padding.Empty,
                            AutoToolTip = false,
                            AutoSize = true,
                            Dock = DockStyle.Fill,
                            TextAlign = ContentAlignment.MiddleLeft
                        };
                        tsb.Click += ToolStripDropDownToolStripButton_Click;
                        return tsb;
                    }).ToArray();

                    var invoke = base.BeginInvoke(new Action(() =>
                    {
                        if (!_dropDownLock.IsSet)
                            return;

                        if (td.Items.Count > 0)
                        {
                            tsdi.HideDropDown();
                            td.Items.Clear();
                        }

                        td.Items.AddRange(items);

                        var toolStrip = tsdi.GetCurrentParent();
                        var bounds = tsdi.Bounds;

                        var currentScreen = Screen.FromControl(toolStrip);
                        var maxWidth = td.Width; // td.Items.OfType<ToolStripItem>().Max(x => x.Width);
                        var totalHeight = td.Height; // td.Items.OfType<ToolStripItem>().Sum(x => x.Height);

                        var screenCoordinatesTopLeft = toolStrip.PointToScreen(new Point(bounds.Left, bounds.Top));
                        var screenCoordinatesBottomRight = toolStrip.PointToScreen(new Point(bounds.Right, bounds.Bottom));

                        var above = screenCoordinatesBottomRight.Y + totalHeight + 5 >= currentScreen.Bounds.Bottom;

                        tsdi.DropDownDirection =
                            screenCoordinatesBottomRight.X - maxWidth >= currentScreen.Bounds.Left &&
                            screenCoordinatesTopLeft.X + maxWidth + 5 >= currentScreen.Bounds.Right ?
                                (above ? ToolStripDropDownDirection.AboveLeft : ToolStripDropDownDirection.BelowLeft) :
                                (above ? ToolStripDropDownDirection.AboveRight : ToolStripDropDownDirection.BelowRight);

                        tsdi.ShowDropDown();
                    }));

                    if (!invoke.IsCompleted)
                        Task.Factory.FromAsync(
                            invoke,
                            endinvoke =>
                            {
                                base.EndInvoke(endinvoke);
                                _dropDownLock.Reset();
                                if (oldButtons.Count > 0)
                                {
                                    Task.Factory.StartNew(() =>
                                    {
                                        foreach (var tsb in oldButtons)
                                        {
                                            tsb.Click -= ToolStripDropDownToolStripButton_Click;
                                            tsb.Dispose();
                                        }
                                        oldButtons.Clear();
                                    }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
                                }
                            },
                            TaskCreationOptions.AttachedToParent,
                            TaskScheduler.Default);
                    else
                    {
                        base.EndInvoke(invoke);
                        _dropDownLock.Reset();
                        if (oldButtons.Count > 0)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                foreach (var tsb in oldButtons)
                                {
                                    tsb.Click -= ToolStripDropDownToolStripButton_Click;
                                    tsb.Dispose();
                                }
                                oldButtons.Clear();
                            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
                        }
                    }
                }
                else
                {
                    _dropDownLock.Reset();
                    if (oldButtons.Count > 0)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            foreach (var tsb in oldButtons)
                            {
                                tsb.Click -= ToolStripDropDownToolStripButton_Click;
                                tsb.Dispose();
                            }
                            oldButtons.Clear();
                        }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
                    }
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void ToolStripDropDownToolStripButton_Click(object sender, EventArgs e)
        {
            ToolStripItem_MouseLeave(null, EventArgs.Empty);

            var tsb = sender as ToolStripButton;
            var text = tsb.Text;
            var pHwnd = IntPtr.Zero;

            var sendSyscommand = false;

            try
            {
                using (var p = Process.GetProcessById(Convert.ToInt32(text.Split(new[] { '/' }, 2).FirstOrDefault())))
                {
                    pHwnd = p.MainWindowHandle;

                    using (var main = p.MainModule)
                        if (main == null)
                            throw new AccessViolationException();
                }
            }
            catch
            {
                sendSyscommand = true;
            }

            KprSecureDesktopAppBar.RestoreWindow(pHwnd, sendSyscommand);
        }

        private void ToolStripItem_ShowToolTip(object sender, EventArgs e)
        {
            var timer = sender as Timer;
            timer.Stop();

            toolTip1.Active = false;
            try
            {
                toolTip1.Hide(this);
            }
            catch { }

            var o = timer.Tag as ToolStripItem;
            if (!string.IsNullOrEmpty(o.ToolTipText))
            {
                var point = PointToClient(MousePosition);

                Size size;
                if (CursorUtil.GetIconSize(out size) && !size.IsEmpty)
                    point.Y += size.Height / 2;

                point.X += 2;
                point.Y += 1;

                toolTip1.Active = true;
                toolTip1.Show(o.ToolTipText, this, point, toolTip1.AutoPopDelay);
            }
        }

        private void ToolStripItem_MouseEnter(object sender, EventArgs e)
        {
            _tooltipTimer.Tag = sender;
            _tooltipTimer.Tick += ToolStripItem_ShowToolTip;

            _tooltipTimer.Stop();
            if (!_lastTooltipMousePosition.HasValue)
                _tooltipTimer.Start();
            toolTip1.Active = false;

            try
            {
                toolTip1.Hide(this);
            }
            catch { }
        }

        private void ToolStripItem_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                _tooltipTimer.Tick -= ToolStripItem_ShowToolTip;
            }
            catch { }

            _tooltipTimer.Stop();
            _lastTooltipMousePosition = null;
            toolTip1.Active = false;

            try
            {
                toolTip1.Hide(this);
            }
            catch { }
        }

        private void ToolStripItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (_lastTooltipMousePosition.HasValue && _lastTooltipMousePosition.Value == e.Location)
                return;

            _lastTooltipMousePosition = e.Location;
            _tooltipTimer.Stop();
            _tooltipTimer.Start();
        }

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_NCLBUTTONDOWN = 0xA1;
            const int HT_CAPTION = 0x2;

            if (m.Msg == NativeMethods.WM_LBUTTONDOWN && _controls.Contains(FromHandle(m.HWnd)))
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);

                return true;
            }

            return false;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPOS
        {
            public IntPtr Hwnd;
            public IntPtr HwndInsertAfter;
            public int X;
            public int Y;
            public int Cx;
            public int Cy;
            public uint Flags;
        }

        private void Snap(IntPtr handle)
        {
            var workingArea = Screen.FromControl(this).Bounds;
            var newPos = (WINDOWPOS)Marshal.PtrToStructure(handle, typeof(WINDOWPOS));
            var snapMarginX = DpiUtil.ScaleIntX(12);
            var snapMarginY = DpiUtil.ScaleIntY(12);
            var border = (Width - ClientSize.Width) / 2;

            if (newPos.Y != workingArea.Y)
            {
                if (Math.Abs(newPos.Y - (workingArea.Y - border)) < snapMarginY && Top + snapMarginY > newPos.Y)
                    newPos.Y = workingArea.Y - border;
                else if (Math.Abs(newPos.Y + Height - (workingArea.Bottom - border)) < snapMarginY && Top - snapMarginY < newPos.Y)
                    newPos.Y = workingArea.Bottom - border - Height;
            }

            if (newPos.X != workingArea.X)
            {
                if (Math.Abs(newPos.X - (workingArea.X - border)) < snapMarginX && Left + snapMarginX > newPos.X)
                    newPos.X = workingArea.X - border;
                else if (Math.Abs(newPos.X + Width - (workingArea.Right - border)) < snapMarginX && Left - snapMarginX < newPos.X)
                    newPos.X = workingArea.Right - border - Width;
            }

            Marshal.StructureToPtr(newPos, handle, false);
        }

        private const int WM_EXITSIZEMOVE = 0x232;

        private void Snap(ref Message m)
        {
            const int WM_WINDOWPOSCHANGING = 0x46;
            const int WM_SIZING = 0x214;

            switch (m.Msg)
            {
                case WM_SIZING:
                    _isResizing = true;
                    break;
                case WM_EXITSIZEMOVE:
                    _isResizing = false;
                    break;
                case WM_WINDOWPOSCHANGING:
                    if (!_isResizing)
                        Snap(m.LParam);
                    break;
            }
        }

        // https://learn.microsoft.com/en-us/windows/win32/power/power-setting-guids
        private static class PowerSettingGuid
        {
            // 0=Powered by AC, 1=Powered by Battery, 2=Powered by short-term source (UPC)
            public static Guid AcdcPowerSource = new Guid("5d3e9a59-e9D5-4b00-a6bd-ff34ff516548");
            // POWERBROADCAST_SETTING.Data = 1-100
            public static Guid BatteryPercentageRemaining = new Guid("a7ad8041-b45a-4cae-87a3-eecbb468a9e1");
            // Windows 8+: 0=Monitor Off, 1=Monitor On, 2=Monitor Dimmed
            public static Guid ConsoleDisplayState = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");
            // 0=Monitor Off, 1=Monitor On.
            public static Guid MonitorPowerGuid = new Guid("02731015-4510-4526-99e6-e5a17ebd1aea");
            // 0=Battery Saver Off, 1=Battery Saver On.
            public static Guid PowerSavingStatus = new Guid("E00958C0-C213-4ACE-AC77-FECCED2EEEA5");

            // Windows 8+: 0=Off, 1=On, 2=Dimmed
            public static Guid SessionDisplayStatus = new Guid("2B84C20E-AD23-4ddf-93DB-05FFBD7EFCA5");

            // Windows 8+, no Session 0: 0=User providing Input, 1=Not present, 2=User inactive
            public static Guid SessionUserPresence = new Guid("3C0F4548-C03F-4c4d-B9F2-237EDE686376");
            // 0=Exiting away mode 1=Entering away mode
            public static Guid SystemAwaymode = new Guid("98a7f580-01f7-48aa-9c0f-44352c29e5C0");

            /* Windows 8+ */
            // POWERBROADCAST_SETTING.Data not used
            public static Guid IdleBackgroundTask = new Guid(0x515C31D8, 0xF734, 0x163D, 0xA0, 0xFD, 0x11, 0xA0, 0x8C, 0x91, 0xE8, 0xF1);

            public static Guid PowerSchemePersonality = new Guid(0x245D8541, 0x3943, 0x4422, 0xB0, 0x25, 0x13, 0xA7, 0x84, 0xF6, 0x79, 0xB7);

            // The Following 3 Guids are the POWERBROADCAST_SETTING.Data result of PowerSchemePersonality
            public static Guid MinPowerSavings = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
            public static Guid MaxPowerSavings = new Guid("a1841308-3541-4fab-bc81-f71556f20b4a");
            public static Guid TypicalPowerSavings = new Guid("381b4222-f694-41f0-9685-ff5bb260df2e");
        }

        private enum DisplayState : byte
        {
            Off = 0x0,
            On = 0x1,
            Dimmed = 0x2
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        private enum DEVICE_NOTIFY : uint
        {
            DEVICE_NOTIFY_WINDOW_HANDLE = 0,
            DEVICE_NOTIFY_SERVICE_HANDLE = 1
        }

        [DllImport("User32.dll", SetLastError = false, EntryPoint = "RegisterPowerSettingNotification")]
        private static extern IntPtr RegisterPowerSettingNotification([In] IntPtr hRecipient, [In] ref Guid PowerSettingGuid, [In][MarshalAs(UnmanagedType.U4)] DEVICE_NOTIFY Flags);

        [DllImport("User32.dll", SetLastError = false, EntryPoint = "UnregisterPowerSettingNotification")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterPowerSettingNotification([In] IntPtr Handle);

        private void WindowEventCallback(IntPtr hWinEventHook, NativeMethods.WINEVENT_EVENTS eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            switch (eventType)
            {
                case NativeMethods.WINEVENT_EVENTS.EVENT_SYSTEM_FOREGROUND:
                case NativeMethods.WINEVENT_EVENTS.EVENT_SYSTEM_DESKTOPSWITCH:
                case NativeMethods.WINEVENT_EVENTS.EVENT_OBJECT_FOCUS:
                    if (_tryAutoSwitch)
                        Task.Factory.StartNew(() =>
                        {
                            if (SecureDesktop.IsValueCreated && !SecureDesktop.Instance.IsActive)
                                Activate();
                            else
                                _tryAutoSwitch = false;
                        }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    break;
                case NativeMethods.WINEVENT_EVENTS.EVENT_SYSTEM_MINIMIZESTART:
                    if (SecureDesktop.IsValueCreated && SecureDesktop.Instance.IsActive && MinimizeStart != null)
                        MinimizeStart.Invoke(hwnd);
                    break;
                case NativeMethods.WINEVENT_EVENTS.EVENT_SYSTEM_MINIMIZEEND:
                    if (SecureDesktop.IsValueCreated && SecureDesktop.Instance.IsActive && MinimizeEnd != null)
                        MinimizeEnd.Invoke(hwnd);
                    break;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NMHDR
        {
            public IntPtr hwndFrom;
            public IntPtr idFrom;
            public int code;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LPNMRUNFILEDLG
        {
            public NMHDR hdr;
            public string lpszFile;
            public string lpszDirectory;
            public int nShow;
        }

        protected override void WndProc(ref Message m)
        {
            Snap(ref m);
            base.WndProc(ref m);

            const int WM_NOTIFY = 0x004E;
            const int WM_POWERBROADCAST = 0x0218;
            const int PBT_POWERSETTINGCHANGE = 0x8013;

            switch (m.Msg)
            {
                case NativeMethods.WM_WINDOWPOSCHANGING:
                    var winpos = (NativeMethods.WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.WINDOWPOS));
                    if (TopMost)
                        winpos.hwndInsertAfter = NativeMethods.HWND_TOPMOST;
                    Marshal.StructureToPtr(winpos, m.LParam, false);
                    break;
                case NativeMethods.WM_NCPAINT:
                    if (_aeroEnabled)
                    {
                        /*var v = (int)NativeMethods.DWMNCRENDERINGPOLICY.DWMNCRP_ENABLED;
                        NativeMethods.DwmSetWindowAttribute(Handle, NativeMethods.DWMWINDOWATTRIBUTE.WCA_NCRENDERING_POLICY, ref v, sizeof(int));*/
                        var margins = new NativeMethods.MARGINS
                        {
                            cxLeftWidth = 1,
                            cxRightWidth = 1,
                            cyTopHeight = 1,
                            cyBottomHeight = 1
                        };
                        NativeMethods.DwmExtendFrameIntoClientArea(Handle, ref margins);
                    }
                    break;
                case NativeMethods.WM_DWMCOMPOSITIONCHANGED:
                    NativeMethods.DwmIsCompositionEnabled(ref _aeroEnabled);
                    break;
                case WM_EXITSIZEMOVE:
                    if (_instances.Count > 1)
                    {
                        var location = Location;
                        Task.Factory.StartNew(() =>
                        {
                            lock (_lock)
                                foreach (var instance in _instances.Where(x => !x.IsDisposed && !x.Disposing && x != this))
                                {
                                    if (instance.InvokeRequired)
                                        instance.InvokeBase(new Action(() =>
                                        {
                                            instance.Location = location;
                                        }));
                                    else
                                        instance.Location = location;
                                }
                        }, _cancellationTokenSource.Token, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }
                    break;
                case WM_POWERBROADCAST:
                    if (_secure)
                    {
                        if (m.WParam.ToInt32() == PBT_POWERSETTINGCHANGE)
                        {
                            var s = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(m.LParam, typeof(POWERBROADCAST_SETTING));
                            if (s.PowerSetting == PowerSettingGuid.SessionDisplayStatus)
                            {
                                switch ((DisplayState)s.Data)
                                {
                                    case DisplayState.Off:
                                        _setAutoSwitch = false;
                                        if (SecureDesktop.Instance.IsActive && !SecureDesktop.Instance.IsLocked)
                                        {
                                            try
                                            {
                                                if (SecureDesktop.IsInput())
                                                    SecureDesktop.Instance.SwitchDesktop(true);
                                            }
                                            catch { }

                                            _setAutoSwitch = true;
                                            _tryAutoSwitch = false;
                                        }
                                        break;
                                    case DisplayState.On:
                                        if (_setAutoSwitch)
                                        {
                                            _setAutoSwitch = false;
                                            _tryAutoSwitch = true;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    break;
                case WM_NOTIFY:
                    if (_secure)
                    {
                        const int RFN_VALIDATE = -510;

                        var n = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));
                        if (n.code == RFN_VALIDATE)
                        {
                            var d = (LPNMRUNFILEDLG)Marshal.PtrToStructure(m.LParam, typeof(LPNMRUNFILEDLG));

                            if (File.Exists(d.lpszFile))
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    using (var p = new SecureProcess
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = d.lpszFile,
                                            //Arguments = split.Length > 1 ? split.LastOrDefault() : string.Empty,
                                            LoadUserProfile = false,
                                            ErrorDialog = true,
                                            ErrorDialogParentHandle = Handle,
                                            WorkingDirectory = d.lpszDirectory,
                                            UseShellExecute = false,
                                            CreateNoWindow = d.nShow == 0,
                                            WindowStyle = d.nShow == 0 ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
                                        }
                                    })
                                        p.StartSecure(false);
                                }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                                m.Result = new IntPtr(1);
                            }
                            else
                                m.Result = new IntPtr(2);

                            /*RF_OK - Allow the application to run
                            RF_CANCEL - Cancel the operation and close the dialog
                            RF_RETRY - Cancel the operation, but leave the dialog open*/
                        }
                    }
                    break;
            }
        }

        /*private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState([MarshalAs(UnmanagedType.I4)] Keys keyCode);

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LockWorkStation();

        private static Keys AddModifiers(Keys key)
        {
            const int KF_UP = 0x8000;

            if ((GetKeyState(Keys.ShiftKey) & KF_UP) != 0) key |= Keys.Shift;
            if ((GetKeyState(Keys.ControlKey) & KF_UP) != 0) key |= Keys.Control;
            if ((GetKeyState(Keys.Menu) & KF_UP) != 0) key |= Keys.Alt;
            if ((GetKeyState(Keys.Capital) & 0x0001) != 0) key |= Keys.CapsLock;
            if ((GetKeyState(Keys.LWin) & KF_UP) != 0) key |= Keys.LWin;
            if ((GetKeyState(Keys.RWin) & KF_UP) != 0) key |= Keys.RWin;

            return key;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            const int WM_KEYDOWN = 0x0100;
            const int WM_SYSKEYDOWN = 0x0104;
            const int HC_ACTION = 0;

            if (nCode == HC_ACTION && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                var vkCode = Marshal.ReadInt32(lParam);
                var key = (Keys)vkCode;
                if (key == Keys.L)
                {
                    key = AddModifiers(key);
                    if (key.HasFlag(Keys.LWin) || key.HasFlag(Keys.RWin))
                    {
                        if (SecureDesktop.IsValueCreated && !SecureDesktop.Instance.IsCancellationRequested)
                            SecureDesktop.Instance.SwitchDesktop(true);
                    }
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }*/
    }
}
