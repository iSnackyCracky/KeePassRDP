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
using KeePassRDP.Extensions;
using KeePassRDP.Utils;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using TTimer = System.Threading.Timer;

namespace KeePassRDP
{
    internal class KprSecureDesktopAppBar : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;

                cp.ExStyle |= NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOINHERITLAYOUT | NativeMethods.WS_EX_NOPARENTNOTIFY;
                cp.ExStyle &= ~NativeMethods.WS_EX_APPWINDOW;

                cp.ClassStyle |= NativeMethods.CS_PARENTDC;

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

        public Screen Screen { get { return _screen; } }

        public bool IsFullscreen
        {
            get
            {
                return _isFullscreen && _buttons.Values.Any(x => x.IsFullscreen);
            }

            set
            {
                if (_isFullscreen = value)
                    SendToBack();
                else
                    BringToFront();
            }
        }

        public bool IsLocked
        {
            get
            {
                return _lock.IsSet;
            }

            set
            {
                if (value)
                    _lock.Set();
                else
                    _lock.Reset();
            }
        }

        private class DateTimeWidget : UserControl
        {
            private readonly TTimer _timer;

            private bool _timerEnabled;
            private DateTime _now;

            public DateTimeWidget() : base()
            {
                _timer = new TTimer(state =>
                {
                    _now = DateTime.Now;
                    Invalidate();
                }, null, Timeout.Infinite, Timeout.Infinite);

                _timerEnabled = false;
                _now = DateTime.MinValue;

                DoubleBuffered = true;
                BackColor = Color.Transparent;
                ForeColor = Color.WhiteSmoke;
                Dock = DockStyle.Fill;
                Margin = Padding.Empty;
                Padding = Padding.Empty;
                Font = new Font(SystemFonts.StatusFont.FontFamily, 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
                Size = new Size(80, 0);
                MinimumSize = new Size(25, 25);
            }

            public DateTime Now
            {
                set
                {
                    if (_now == value)
                        return;

                    _now = value;
                    Text = value.ToString("D");

                    Invalidate();
                }
            }

            protected override void OnDoubleClick(EventArgs e)
            {
                if (_timerEnabled = !_timerEnabled)
                {
                    _now = DateTime.Now;
                    _timer.Change(TimeSpan.FromMilliseconds(1000 - _now.Millisecond), TimeSpan.FromMilliseconds(1000));
                }
                else
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);

                Invalidate();

                base.OnDoubleClick(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (_now == DateTime.MinValue)
                    return;

                var font = Font;
                using (var f1 = new Font(font.FontFamily, font.Size + 1.75f, font.Style, font.Unit))
                using (var sf = new StringFormat(StringFormat.GenericTypographic)
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.None,
                    HotkeyPrefix = HotkeyPrefix.None
                })
                {
                    var state = e.Graphics.Save();

                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                    e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    e.Graphics.TextContrast = 0;

                    var t1 = _now.ToString(_timerEnabled ? "T" : "t");
                    var t2 = _now.ToString("d");
                    var s1 = e.Graphics.MeasureString(t1, f1, SizeF.Empty, sf);
                    var s2 = e.Graphics.MeasureString(t2, font, SizeF.Empty, sf);

                    var th = s1.Height / 2f + s2.Height / 2f;

                    var r1 = new RectangleF(new PointF(Width / 2f - s1.Width / 2f, 1 + Height / 2f - th), s1);
                    var r2 = new RectangleF(new PointF(Width / 2f - s2.Width / 2f, 1 + Height / 2f), s2);

                    var singleLine = false;
                    var rect = Rectangle.Ceiling(RectangleF.Union(r1, r2));

                    if (rect.Height > Height - 5)
                    {
                        r1.Y = Height / 2f - s1.Height / 2f;
                        singleLine = true;
                    }

                    if (e.ClipRectangle.IntersectsWith(rect))
                        using (var brush = new SolidBrush(Color.FromArgb(200, ForeColor)))
                        {
                            e.Graphics.DrawString(t1, f1, brush, r1, sf);  //TextRenderer.DrawText(e.Graphics, t1, f1, p1, Color.FromArgb(200, ForeColor));
                            if (!singleLine)
                                e.Graphics.DrawString(t2, font, brush, r2, sf); //TextRenderer.DrawText(e.Graphics, t2, font, p2, Color.FromArgb(200, ForeColor));
                        }

                    e.Graphics.Restore(state);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }

                base.Dispose(disposing);
            }
        }

        private class DwmPreview : Form
        {
            [DllImport("dwmapi.dll")]
            private static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

            [DllImport("dwmapi.dll")]
            private static extern int DwmUnregisterThumbnail(IntPtr thumb);

            [DllImport("dwmapi.dll")]
            private static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

            [StructLayout(LayoutKind.Sequential)]
            private struct PSIZE
            {
                public int x;
                public int y;
            }

            [DllImport("dwmapi.dll")]
            static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);

            [StructLayout(LayoutKind.Sequential)]
            private struct DWM_THUMBNAIL_PROPERTIES
            {
                public int dwFlags;
                public RECT rcDestination;
                public RECT rcSource;
                public byte opacity;
                public bool fVisible;
                public bool fSourceClientAreaOnly;
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;

                    cp.ExStyle |= NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOINHERITLAYOUT | NativeMethods.WS_EX_NOPARENTNOTIFY | NativeMethods.WS_EX_NOACTIVATE;
                    cp.ExStyle &= ~NativeMethods.WS_EX_APPWINDOW;

                    cp.ClassStyle |= NativeMethods.CS_PARENTDC;

                    return cp;
                }
            }

            private IntPtr _previewWindow;
            private IntPtr _thumbHandle;

            public IntPtr PreviewWindow
            {
                set
                {
                    _previewWindow = value;
                    RegisterThumbnail();
                    RefreshThumbnail();
                }
            }

            public DwmPreview()
            {
                Location = Point.Empty;
                Size = new Size(225, 125);
                AutoScaleDimensions = new SizeF(6F, 13F);
                AutoScaleMode = AutoScaleMode.Font;
                BackgroundImageLayout = ImageLayout.None;
                DoubleBuffered = true;
                FormBorderStyle = FormBorderStyle.None;
                Padding = Padding.Empty;
                StartPosition = FormStartPosition.Manual;
                TopMost = true;
                TopLevel = true;
                //Opacity = .97;
                BackColor = Color.Black;

                _thumbHandle = IntPtr.Zero;
                _previewWindow = IntPtr.Zero;
            }

            public void RefreshThumbnail()
            {
                if (_thumbHandle == IntPtr.Zero)
                    return;

                const int DWM_TNP_VISIBLE = 0x8;
                const int DWM_TNP_OPACITY = 0x4;
                const int DWM_TNP_RECTDESTINATION = 0x1;
                const int DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;

                PSIZE size;
                DwmQueryThumbnailSourceSize(_thumbHandle, out  size);

                var width = Width - 6;
                var height = Height - 6;

                var rect = new RECT
                {
                    left = 3,
                    top = 3,
                    right = width,
                    bottom = height
                };

                if (size.x <= width && size.y <= height)
                {
                    rect.top += (int)Math.Round((height - size.y) / 2f);
                    rect.left += (int)Math.Round((width - size.x) / 2f);
                    rect.right = rect.left + size.x;
                    rect.bottom = rect.top + size.y;
                }
                else
                {
                    var aspectRatio = (float)size.x / size.y;
                    var controlAspectRatio = (float)width / height;

                    if (aspectRatio > controlAspectRatio)
                    {
                        var newHeight = (int)Math.Ceiling(width / aspectRatio);

                        rect.top += (int)Math.Round((height - newHeight) / 2f);
                        rect.bottom = rect.top + newHeight;
                    }
                    else if (aspectRatio < controlAspectRatio)
                    {
                        var newWidth = (int)Math.Ceiling(height * aspectRatio);

                        rect.left += (int)Math.Round((width - newWidth) / 2f);
                        rect.right = rect.left + newWidth;
                    }
                }

                var props = new DWM_THUMBNAIL_PROPERTIES
                {
                    fVisible = true,
                    dwFlags = DWM_TNP_VISIBLE | DWM_TNP_RECTDESTINATION | DWM_TNP_OPACITY | DWM_TNP_SOURCECLIENTAREAONLY,
                    opacity = 255,
                    rcDestination = rect,
                    fSourceClientAreaOnly = true
                };

                DwmUpdateThumbnailProperties(_thumbHandle, ref props);
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);

                /*
  DrawLeftBorder = $20;
  DrawTopBorder = $40;
  DrawRightBorder = $80;
  DrawBottomBorder = $100;*/

                var margins = new NativeMethods.MARGINS
                {
                    cxLeftWidth = -1,
                    cxRightWidth = -1,
                    cyTopHeight = -1,
                    cyBottomHeight = -1
                };
                NativeMethods.DwmExtendFrameIntoClientArea(Handle, ref margins);

                var accPolicy = new NativeMethods.AccentPolicy
                {
                    AccentState = NativeMethods.DWMACCENTSTATE.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                    GradientColor = Color.FromArgb(100, Color.Black).ToAbgr(),
                    //AccentFlags = 2
                };

                var accentSize = Marshal.SizeOf(accPolicy);
                var accentPtr = Marshal.AllocHGlobal(accentSize);
                Marshal.StructureToPtr(accPolicy, accentPtr, false);
                var data = new NativeMethods.WindowCompositionAttributeData
                {
                    Attribute = NativeMethods.DWMWINDOWATTRIBUTE.WCA_ACCENT_POLICY,
                    Data = accentPtr,
                    SizeOfData = accentSize
                };

                NativeMethods.SetWindowCompositionAttribute(Handle, ref data);
                Marshal.FreeHGlobal(accentPtr);


                RegisterThumbnail();
            }

            protected override void OnHandleDestroyed(EventArgs e)
            {
                UnregisterThumbnail();

                base.OnHandleDestroyed(e);
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                RegisterThumbnail();
            }

            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                UnregisterThumbnail();

                base.OnFormClosed(e);
            }

            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);

                RegisterThumbnail();
                RefreshThumbnail();
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);

                if (Visible)
                {
                    RegisterThumbnail();
                    RefreshThumbnail();
                }
                else
                    UnregisterThumbnail();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                RefreshThumbnail();
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                base.OnPaintBackground(e);

                RefreshThumbnail();
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);

                RefreshThumbnail();
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                base.OnLayout(levent);

                RefreshThumbnail();
            }

            private void RegisterThumbnail()
            {
                UnregisterThumbnail();

                if (!IsHandleCreated)
                    return;

                if (_previewWindow != IntPtr.Zero)
                    DwmRegisterThumbnail(Handle, _previewWindow, out _thumbHandle);
            }

            private void UnregisterThumbnail()
            {
                if (_thumbHandle != IntPtr.Zero)
                {
                    DwmUnregisterThumbnail(_thumbHandle);
                    _thumbHandle = IntPtr.Zero;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    UnregisterThumbnail();
                }

                base.Dispose(disposing);
            }
        }

        private class CustomButton : UserControl
        {
            public Image Image { get; set; }
            public bool AutoEllipsis { get; set; }
            public Padding BorderSize { get; set; }
            public Color BorderColor { get; set; }
            public Color ButtonColor { get; set; }

            public bool IsActive
            {
                get { return _isActive; }
                set
                {
                    _isActive = value;
                    Invalidate();
                }
            }

            public bool IsFlashing
            {
                get { return _isFlashing; }
                set
                {
                    _isFlashing = value;
                    Invalidate();
                }
            }

            public bool IsMinimized
            {
                get { return _isMinimized; }
                set
                {
                    _isMinimized = value;
                    Invalidate();
                }
            }

            public bool IsFullscreen
            {
                get { return _isFullscreen; }
                set
                {
                    _isFullscreen = value;
                    Invalidate();
                }
            }

            public bool WasFullscreen
            {
                get { return _wasFullscreen; }
                set
                {
                    _wasFullscreen = value;
                    Invalidate();
                }
            }

            public bool IsDragging
            {
                get { return _isDragging; }
                set
                {
                    _isDragging = value;
                    Invalidate();
                }
            }

            private bool _isDragging;
            private bool _isMinimized;
            private bool _isFullscreen;
            private bool _wasFullscreen;
            private bool _isActive;
            private bool _isFlashing;
            private bool _mouseHover;
            private bool _mouseLeftDown;

            public CustomButton() : base()
            {
                DoubleBuffered = true;

                Image = null;
                AutoEllipsis = true;
                BorderSize = Padding.Empty;
                BorderColor = Color.Transparent;
                ButtonColor = Color.Transparent;
                BackColor = Color.Transparent;
                Margin = new Padding(0, 0, 1, 1);
                Padding = Padding.Empty;
                Anchor = AnchorStyles.None;

                _isDragging = false;
                _isMinimized = false;
                _isFullscreen = false;
                _wasFullscreen = false;
                _isActive = false;
                _isFlashing = false;
                _mouseHover = false;
                _mouseLeftDown = false;
            }

            public void Lock()
            {
                //_lock.Wait();
                SuspendLayout();
            }

            public void Release()
            {
                ResumeLayout(false);
                Invalidate();
                //_lock.Release();
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                _mouseHover = true;
                _mouseLeftDown = false;
                _isDragging = false;
                Invalidate();

                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                _mouseHover = false;
                _mouseLeftDown = false;
                Invalidate();

                base.OnMouseLeave(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                    _mouseLeftDown = true;

                if (e.Clicks == 1)
                    Invalidate();

                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                    _mouseLeftDown = false;

                if (e.Clicks == 1)
                    Invalidate();

                base.OnMouseUp(e);
            }

            protected override void Dispose(bool disposing)
            {
                if (Image != null)
                {
                    Image.Dispose();
                    Image = null;
                }

                base.Dispose(disposing);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                //_lock.Wait();

                var state = e.Graphics.Save();

                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                e.Graphics.TextContrast = 0;

                var rect = e.ClipRectangle; //new Rectangle(Point.Empty, Size);
                if (!_isActive && !_mouseHover && !_mouseLeftDown && !_isDragging)
                    rect.Inflate(-2, 0);

                using (var brush = new SolidBrush(Color.FromArgb(_isDragging ? 135 : _mouseLeftDown ? 195 : _mouseHover ? 235 : _isActive ? 255 : 155, ButtonColor)))
                    e.Graphics.FillRectangle(brush, rect);

                var border = BorderSize;
                using (var brush = new SolidBrush(Color.FromArgb(225, BorderColor)))
                {
                    if (border.Left > 0)
                        e.Graphics.FillRectangle(brush, new Rectangle(new Point(rect.X, rect.Y), new Size(border.Left, rect.Height)));
                    if (border.Top > 0)
                        e.Graphics.FillRectangle(brush, new Rectangle(new Point(rect.X, rect.Y), new Size(rect.Width, border.Top)));
                    if (border.Right > 0)
                        e.Graphics.FillRectangle(brush, new Rectangle(new Point(rect.X + rect.Width - border.Right, rect.Y), new Size(border.Right, rect.Height)));
                    if (border.Bottom > 0)
                        e.Graphics.FillRectangle(brush, new Rectangle(new Point(rect.X, rect.Y + rect.Height - border.Bottom), new Size(rect.Width, border.Bottom)));
                }

                if (Image != null)
                {
                    var image = Image;
                    e.Graphics.DrawImage(
                        image,
                        new RectangleF(rect.X + 6 + (_isActive || _mouseHover || _mouseLeftDown || _isDragging ? 2 : 0), rect.Y + 6 + (border.Top > 0 ? 1 : border.Bottom > 0 ? -1 : 0), rect.Height - 12, rect.Height - 12),
                        new RectangleF(0, 0, image.Width, image.Height),
                        GraphicsUnit.Pixel);
                    rect.X += rect.Height - 5;
                    rect.Width -= rect.Height - 5;
                }

                //using (var gp = new GraphicsPath())
                using (var brush = new SolidBrush(Color.FromArgb(_isDragging ? 135 : _mouseLeftDown ? 195 : _mouseHover ? 235 : _isActive ? 255 : 155, ForeColor)))
                using (var sf = new StringFormat(StringFormat.GenericTypographic)
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = AutoEllipsis ? StringTrimming.EllipsisCharacter : StringTrimming.None,
                    FormatFlags = StringFormatFlags.NoWrap,
                    HotkeyPrefix = HotkeyPrefix.None
                })
                {
                    var font = Font;
                    var text = Text;
                    /*gp.AddString(text, font.FontFamily, (int)font.Style, font.Size, Rectangle.Inflate(rect, -2, -2), sf);
                    e.Graphics.FillPath(brush, gp);*/
                    rect.Inflate(-4 - (_isActive || _mouseHover || _mouseLeftDown || _isDragging ? 2 : 0), 0);
                    var size = e.Graphics.MeasureString(text, font, rect.Size, sf);
                    e.Graphics.DrawString(
                        text,
                        font,
                        brush,
                        new RectangleF(new PointF(rect.X, (rect.Height - size.Height) / 2f + (border.Top > 0 ? 1 : border.Bottom > 0 ? -1 : 0)), size),
                        sf);
                }

                e.Graphics.Restore(state);

                //_lock.Release();
            }
        }

        private readonly TableLayoutPanel tableLayoutPanel1;
        private readonly FlowLayoutPanel flowLayoutPanel1;
        private readonly DateTimeWidget dateTimeWidget;
        private readonly DwmPreview dwmPreview;

        private readonly Screen _screen;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentDictionary<IntPtr, CustomButton> _buttons;
        private readonly KprToolTip _toolTip;
        private readonly Timer _toolTipTimer;
        private readonly Timer _dragDropTimer;
        private readonly ManualResetEventSlim _lock;

        private Point? _lastTooltipMousePosition;
        private volatile bool _isFullscreen;
        private int _rowCount;
        private Rectangle _appBarBounds;
        private IntPtr _appBarHandle;

        public KprSecureDesktopAppBar(Screen screen)
        {
            _screen = screen;
            _cancellationTokenSource = new CancellationTokenSource();
            _buttons = new ConcurrentDictionary<IntPtr, CustomButton>(5, 0);
            _toolTip = new KprToolTip
            {
                AutoPopDelay = 10000,
                InitialDelay = 1000,
                ReshowDelay = 100,
                UseAnimation = false,
                UseFading = false,
                OwnerDraw = true
            };
            _toolTip.Draw += (s, e) =>
            {
                e.DrawBackground();
                e.DrawText();
                e.DrawBorder();
            };
            _toolTipTimer = new Timer
            {
                Enabled = false,
                Interval = _toolTip.InitialDelay
            };
            _toolTipTimer.Tick += ToolTipTimer_Tick;
            _dragDropTimer = new Timer
            {
                Enabled = false,
                Interval = _toolTip.InitialDelay / 2
            };
            _dragDropTimer.Tick += DragDropTimer_Tick;
            _lock = new ManualResetEventSlim(false);
            _lastTooltipMousePosition = null;
            _isFullscreen = false;
            _rowCount = 1;
            _appBarBounds = Rectangle.Empty;
            _appBarHandle = IntPtr.Zero;

            SuspendLayout();

            dwmPreview = new DwmPreview();

            tableLayoutPanel1 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                RowCount = 2,
                ColumnCount = 2,
                BackColor = Color.Transparent
            };

            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            flowLayoutPanel1 = new FlowLayoutPanel
            {
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                ForeColor = Color.FromArgb(64, 64, 64),
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AllowDrop = true
            };
            flowLayoutPanel1.DragOver += (s, e) =>
            {
                if (e.Data.GetDataPresent(typeof(CustomButton)))
                {
                    e.Effect = DragDropEffects.Move;
                    var dragButton = e.Data.GetData(typeof(CustomButton)) as CustomButton;
                    dragButton.IsDragging = true;
                    var point = flowLayoutPanel1.PointToClient(new Point(e.X, e.Y));
                    var dropButton = flowLayoutPanel1.Controls.OfType<CustomButton>().FirstOrDefault(x => x.Bounds.Contains(point));
                    if (dropButton == null)
                        return;
                    var newIdx = flowLayoutPanel1.Controls.IndexOf(dropButton);
                    var oldIdx = flowLayoutPanel1.Controls.IndexOf(dragButton);
                    var dropBounds = dropButton.Bounds;
                    var dragBounds = dragButton.Bounds;
                    if ((newIdx < oldIdx && dropBounds.Top == dragBounds.Top && point.X < dropBounds.Right - dropBounds.Width / 2) ||
                        (newIdx > oldIdx && dropBounds.Top == dragBounds.Top && point.X > dropBounds.Left + dropBounds.Width / 2) ||
                        (newIdx < oldIdx && dropBounds.Top < dragBounds.Top && point.Y < dropBounds.Bottom - dropBounds.Height / 2) ||
                        (newIdx > oldIdx && dropBounds.Top > dragBounds.Top && point.Y > dropBounds.Top + dropBounds.Height / 2))
                        flowLayoutPanel1.Controls.SetChildIndex(dragButton, newIdx);
                }
                else
                    e.Effect = DragDropEffects.None;
            };
            flowLayoutPanel1.DragDrop += (s, e) =>
            {
                if (e.Data.GetDataPresent(typeof(CustomButton)))
                {
                    var dragButton = e.Data.GetData(typeof(CustomButton)) as CustomButton;
                    dragButton.IsDragging = false;
                    NativeMethods.SendMessage(dragButton.Handle, NativeMethods.WM_LBUTTONUP, 0, 0);
                }
            };
            flowLayoutPanel1.SetDoubleBuffered();
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 0);

            dateTimeWidget = new DateTimeWidget();
            dateTimeWidget.MouseEnter += Button_MouseEnter;
            dateTimeWidget.MouseMove += Button_MouseMove;
            dateTimeWidget.MouseLeave += Button_MouseLeave;
            tableLayoutPanel1.Controls.Add(dateTimeWidget, 1, 0);

            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.None;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Padding = new Padding(0, 1, 0, 0);
            StartPosition = FormStartPosition.Manual;
            TopLevel = true;
            Opacity = .97;
            BackColor = Color.Black;
            /*SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            TransparencyKey = Color.Transparent;*/

            tableLayoutPanel1.SetDoubleBuffered();
            Controls.Add(tableLayoutPanel1);

            ResumeLayout(false);
        }

        [DllImport("User32.dll", EntryPoint = "GetClassLongPtr", SetLastError = false)]
        private static extern IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex);

        private static Icon GetIconFromHwnd(IntPtr hwnd)
        {
            const int WM_GETICON = 0x7F;

            const int ICON_SMALL = 0;
            const int ICON_BIG = 1;
            const int ICON_SMALL2 = 2;

            const int GCL_HICONSM = -34;
            const int GCL_HICON = -14;

            var iconHandle = NativeMethods.SendMessage(hwnd, WM_GETICON, ICON_SMALL2, 0);

            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.SendMessage(hwnd, WM_GETICON, ICON_SMALL, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.SendMessage(hwnd, WM_GETICON, ICON_BIG, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, GCL_HICON);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, GCL_HICONSM);

            if (iconHandle == IntPtr.Zero)
                return null;

            return Icon.FromHandle(iconHandle);
        }

        public void UpdateDateTime(DateTime now)
        {
            if (IsDisposed || Disposing)
                return;

            dateTimeWidget.Now = now;
        }

        public void AddOrUpdateWindow(IntPtr hwnd, ManualResetEventSlim signal = null)
        {
            if (hwnd == IntPtr.Zero || IsDisposed || Disposing)
            {
                try
                {
                    if (signal != null && !signal.IsSet)
                        signal.Set();
                }
                catch { }

                return;
            }

            var threadId = NativeMethods.GetCurrentThreadId();

            Task.Factory.StartNew(() =>
            {
                CustomButton button = null;
                var text = string.Empty;

                uint procId;
                if (!_screen.Equals(Screen.FromHandle(hwnd)) || NativeMethods.GetWindowThreadProcessId(hwnd, out procId) == threadId || FromHandle(hwnd) != null)
                {
                    RemoveWindow(hwnd);
                    return;
                }

                var wndStylePtr = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_STYLE);
                var wndStyle = IntPtr.Size == 4 ? wndStylePtr.ToInt32() : wndStylePtr.ToInt64();

                var wndExStylePtr = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE);
                var wndExStyle = IntPtr.Size == 4 ? wndStylePtr.ToInt32() : wndStylePtr.ToInt64();

                const int GW_OWNER = 4;

                if ((wndExStyle & NativeMethods.WS_EX_TOOLWINDOW) != 0 ||
                    (wndStyle & NativeMethods.WS_VISIBLE) == 0 ||
                    (wndExStyle & NativeMethods.WS_EX_APPWINDOW) == 0 && (
                        (wndStyle & NativeMethods.WS_POPUP) != 0 ||
                        (wndStyle & NativeMethods.WS_CHILD) != 0) ||
                    GetWindow(hwnd, GW_OWNER) != IntPtr.Zero)
                {
                    RemoveWindow(hwnd);
                    return;
                }

                using (var c = Process.GetCurrentProcess())
                    if (c.Id == procId)
                    {
                        RemoveWindow(hwnd);
                        return;
                    }

                var isMstsc = false;

                try
                {
                    using (var p = Process.GetProcessById(Convert.ToInt32(procId)))
                    {
                        var pHwnd = p.MainWindowHandle;
                        if (pHwnd != IntPtr.Zero)
                        {
                            if (hwnd == pHwnd)
                                text = p.MainWindowTitle;
                            else
                            {
                                RemoveWindow(hwnd);
                                hwnd = pHwnd;
                            }
                        }

                        try
                        {
                            using (var m = p.MainModule)
                                if (m != null)
                                    isMstsc = string.Equals(Path.GetFileName(m.FileName), "mstsc.exe", StringComparison.OrdinalIgnoreCase);
                        }
                        catch { }
                    }
                }
                catch { }

                if (string.IsNullOrEmpty(text))
                {
                    var title = new char[NativeMethods.GetWindowTextLength(hwnd) + 1];
                    if (NativeMethods.GetWindowText(hwnd, title, title.Length) != 0)
                    {
                        text = new string(title).TrimEnd(char.MinValue);

                        if (text == "Default IME" || text == "MSCTFIME UI")
                        {
                            RemoveWindow(hwnd);
                            return;
                        }
                    }
                }

                try
                {
                    button = _buttons.GetOrAdd(hwnd, h =>
                    {
                        var width = flowLayoutPanel1.Width - 1;
                        var height = flowLayoutPanel1.Height;

                        var size = new Size(height > width ? width + 1 : Math.Min(width / 10, width / (int)Math.Ceiling((float)Math.Max(1, _buttons.Count + 1) / _rowCount)) - 1, (int)Math.Round((float)height / _rowCount));

                        var padding = Padding;
                        using (var icon = GetIconFromHwnd(hwnd))
                        {
                            var b = new CustomButton
                            {
                                Tag = hwnd,
                                ButtonColor = Color.WhiteSmoke,
                                ForeColor = flowLayoutPanel1.ForeColor,
                                Font = flowLayoutPanel1.Font,
                                Image = icon.ToBitmap(),
                                BorderColor = isMstsc ? Color.Red : Color.OrangeRed,
                                BorderSize = Height > Width ?
                                    _appBarBounds.Right < _screen.Bounds.Right ?
                                        new Padding(3, 0, 0, 0) :
                                        new Padding(0, 0, 3, 0) :
                                        _appBarBounds.Bottom < _screen.Bounds.Bottom ?
                                            new Padding(0, 3, 0, 0) :
                                            new Padding(0, 0, 0, 3),
                                Size = size
                            };

                            b.MouseEnter += Button_MouseEnter;
                            b.MouseMove += Button_MouseMove;
                            b.MouseLeave += Button_MouseLeave;
                            b.MouseDown += Button_MouseDown;
                            b.MouseUp += Button_MouseUp;

                            IconUtil.DestroyIcon(icon.Handle);

                            return b;
                        }
                    });
                }
                finally
                {
                    try
                    {
                        if (signal != null && !signal.IsSet)
                            signal.Set();
                    }
                    catch { }
                }

                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    flowLayoutPanel1.SuspendLayout();

                    button = _buttons.AddOrUpdate(hwnd, h => button, (h, b) =>
                    {
                        try
                        {
                            if (button.IsDisposed || button.Disposing)
                                button = b;

                            if (button != b)
                            {
                                lock (flowLayoutPanel1)
                                    if (flowLayoutPanel1.Controls.Contains(b))
                                        flowLayoutPanel1.Controls.Remove(b);

                                if (b.Image != null)
                                {
                                    b.Image.Dispose();
                                    b.Image = null;
                                }

                                b.MouseEnter -= Button_MouseEnter;
                                b.MouseMove -= Button_MouseMove;
                                b.MouseLeave -= Button_MouseLeave;
                                b.MouseDown -= Button_MouseDown;
                                b.MouseUp -= Button_MouseUp;
                                b.Dispose();
                            }
                        }
                        catch (ObjectDisposedException) { }

                        return button;
                    });

                    try
                    {
                        var isActive = !button.IsMinimized && NativeMethods.GetForegroundWindow() == hwnd;
                        if (isActive)
                            foreach (var b in _buttons.Values.Where(x => x.IsActive && (IntPtr)x.Tag != hwnd))
                            {
                                b.Lock();
                                b.IsActive = false;
                                b.Release();
                            }

                        button.Lock();
                        button.Text = text;
                        button.IsFlashing = false;
                        button.IsActive = isActive;
                        if (button.IsActive && !button.IsMinimized && button.WasFullscreen && !button.IsFullscreen)
                        {
                            button.IsFullscreen = true;
                            IsFullscreen = true;
                        }
                        button.Release();

                        lock (flowLayoutPanel1)
                            if (!flowLayoutPanel1.Controls.Contains(button))
                                flowLayoutPanel1.Controls.Add(button);
                    }
                    catch (ObjectDisposedException) { }

                    flowLayoutPanel1.ResumeLayout(true);
                    flowLayoutPanel1.Invalidate();

                    if (_isFullscreen && _buttons.Values.All(x => !x.IsFullscreen))
                        IsFullscreen = false;

                    if (_buttons.Count > 10 * _rowCount)
                    {
                        var oldCount = _buttons.Count;
                        Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                        {
                            if (_buttons.Count != oldCount)
                                return;

                            RefreshButtonSize();
                        })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
        }

        private void RefreshButtonSize()
        {
            if (_buttons.Count == 0)
                return;

            var width = flowLayoutPanel1.Width - 1;
            var height = flowLayoutPanel1.Height;

            var size = new Size(height > width ? width : Math.Min(width / 10, width / (int)Math.Ceiling((float)Math.Max(1, _buttons.Count) / _rowCount)) - 1, (int)Math.Round((float)height / _rowCount));
            var border = Height > Width ?
                _appBarBounds.Right < _screen.Bounds.Right ?
                    new Padding(3, 0, 0, 0) :
                    new Padding(0, 0, 3, 0) :
                    _appBarBounds.Bottom < _screen.Bounds.Bottom ?
                        new Padding(0, 3, 0, 0) :
                        new Padding(0, 0, 0, 3);

            flowLayoutPanel1.SuspendLayout();

            foreach (var b in _buttons.Values)
                try
                {
                    if (b.Size != size)
                    {
                        b.SuspendLayout();
                        b.Size = size;
                        b.BorderSize = border;
                        b.ResumeLayout(true);
                        b.Invalidate();
                    }

                    lock (flowLayoutPanel1)
                        if (!flowLayoutPanel1.Controls.Contains(b))
                            flowLayoutPanel1.Controls.Add(b);
                }
                catch { }

            flowLayoutPanel1.ResumeLayout(true);
            flowLayoutPanel1.Invalidate();
        }

        public void RemoveWindow(IntPtr hwnd)
        {
            if (IsDisposed || Disposing)
                return;

            if (hwnd == IntPtr.Zero)
                return;

            CustomButton button;
            if (_buttons.TryRemove(hwnd, out button))
            {
                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    flowLayoutPanel1.SuspendLayout();

                    lock (flowLayoutPanel1)
                        if (flowLayoutPanel1.Controls.Contains(button))
                            flowLayoutPanel1.Controls.Remove(button);

                    flowLayoutPanel1.ResumeLayout(true);
                    flowLayoutPanel1.Invalidate();

                    if (_isFullscreen && _buttons.Values.All(x => !x.IsFullscreen))
                        IsFullscreen = false;

                    button.MouseEnter -= Button_MouseEnter;
                    button.MouseMove -= Button_MouseMove;
                    button.MouseLeave -= Button_MouseLeave;
                    button.MouseDown -= Button_MouseDown;
                    button.MouseUp -= Button_MouseUp;
                    if (button.Image != null)
                    {
                        button.Image.Dispose();
                        button.Image = null;
                    }
                    button.Dispose();

                    if (_buttons.Count >= 10 * _rowCount)
                    {
                        var oldCount = _buttons.Count;
                        Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                        {
                            if (_buttons.Count != oldCount)
                                return;

                            RefreshButtonSize();
                        })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }
        }

        public void EnterFullscreen(IntPtr hwnd)
        {
            if (IsDisposed || Disposing)
                return;

            if (hwnd == IntPtr.Zero)
                return;

            Task.Factory.StartNew(() =>
            {
                CustomButton button;

                if (!_buttons.TryGetValue(hwnd, out button))
                {
                    if (_screen.Equals(Screen.FromHandle(hwnd)))
                    {
                        var mrs = new ManualResetEventSlim(false);
                        AddOrUpdateWindow(hwnd, mrs);

                        Task.Factory.StartNew(() =>
                        {
                            using (mrs)
                                if (!mrs.Wait(TimeSpan.FromSeconds(1)))
                                    return;

                            if (!_buttons.TryGetValue(hwnd, out button))
                                return;

                            EnterFullscreen(hwnd);
                        }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }

                    return;
                }

                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    button.Lock();
                    if (!button.IsFullscreen)
                    {
                        button.WasFullscreen = true;
                        if (!button.IsMinimized)
                        {
                            button.IsFullscreen = true;
                            IsFullscreen = true;
                        }
                    }
                    button.Release();
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
        }

        public void ExitFullscreen(IntPtr hwnd)
        {
            if (IsDisposed || Disposing)
                return;

            if (hwnd == IntPtr.Zero)
                return;

            Task.Factory.StartNew(() =>
            {
                CustomButton button;

                if (!_buttons.TryGetValue(hwnd, out button))
                {
                    if (_screen.Equals(Screen.FromHandle(hwnd)))
                    {
                        var mrs = new ManualResetEventSlim(false);
                        AddOrUpdateWindow(hwnd, mrs);

                        Task.Factory.StartNew(() =>
                        {
                            using (mrs)
                                if (!mrs.Wait(TimeSpan.FromSeconds(1)))
                                    return;

                            if (!_buttons.TryGetValue(hwnd, out button))
                                return;

                            ExitFullscreen(hwnd);
                        }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }

                    if (_isFullscreen && _buttons.Values.All(x => !x.IsFullscreen))
                        IsFullscreen = false;

                    return;
                }

                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    button.Lock();
                    if (button.IsFullscreen)
                    {
                        if (!button.IsMinimized)
                            button.WasFullscreen = false;
                        button.IsFullscreen = false;
                    }
                    button.Release();

                    if (_isFullscreen && _buttons.Values.All(x => !x.IsFullscreen))
                        IsFullscreen = false;
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
        }

        public void SetActive(IntPtr hwnd)
        {
            if (IsDisposed || Disposing)
                return;

            Task.Factory.StartNew(() =>
            {
                foreach (var b in _buttons.Values.Where(x => x.IsActive && (IntPtr)x.Tag != hwnd))
                    b.IsActive = false;

                if (hwnd == IntPtr.Zero)
                    return;

                CustomButton button;

                if (!_buttons.TryGetValue(hwnd, out button))
                {
                    if (_screen.Equals(Screen.FromHandle(hwnd)))
                    {
                        var mrs = new ManualResetEventSlim(false);
                        AddOrUpdateWindow(hwnd, mrs);

                        Task.Factory.StartNew(() =>
                        {
                            using (mrs)
                                if (!mrs.Wait(TimeSpan.FromSeconds(1)))
                                    return;

                            if (!_buttons.TryGetValue(hwnd, out button))
                                return;

                            SetActive(hwnd);
                        }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }

                    if (_isFullscreen && _buttons.Values.All(x => !x.IsFullscreen))
                        IsFullscreen = false;

                    return;
                }

                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    button.Lock();
                    if (!button.IsActive)
                    {
                        button.IsActive = true;
                        if (!button.IsMinimized && button.WasFullscreen && !button.IsFullscreen)
                        {
                            button.IsFullscreen = true;
                            IsFullscreen = true;
                        }
                    }
                    button.Release();

                    if (_isFullscreen && _buttons.Values.All(x => !x.IsFullscreen))
                        IsFullscreen = false;
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
        }

        public void SetFlashing(IntPtr hwnd)
        {
            if (IsDisposed || Disposing)
                return;

            if (hwnd == IntPtr.Zero)
                return;

            Task.Factory.StartNew(() =>
            {
                CustomButton button;

                if (!_buttons.TryGetValue(hwnd, out button))
                {
                    if (_screen.Equals(Screen.FromHandle(hwnd)))
                    {
                        var mrs = new ManualResetEventSlim(false);
                        AddOrUpdateWindow(hwnd, mrs);

                        Task.Factory.StartNew(() =>
                        {
                            using (mrs)
                                if (!mrs.Wait(TimeSpan.FromSeconds(1)))
                                    return;

                            if (!_buttons.TryGetValue(hwnd, out button))
                                return;

                            SetFlashing(hwnd);
                        }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }

                    return;
                }

                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    button.Lock();
                    button.IsFlashing = true;
                    button.Release();
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
        }

        public void Minimize(IntPtr hwnd)
        {
            if (IsDisposed || Disposing)
                return;

            Task.Factory.StartNew(() =>
            {
                CustomButton button;

                if (!_buttons.TryGetValue(hwnd, out button))
                {
                    if (_isFullscreen && _buttons.Values.All(x => !x.IsFullscreen))
                        IsFullscreen = false;

                    return;
                }

                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    button.Lock();
                    if (!button.IsMinimized)
                    {
                        button.WasFullscreen = button.WasFullscreen || button.IsFullscreen;
                        button.IsFullscreen = false;
                        button.IsMinimized = true;
                    }
                    button.Release();

                    if (_isFullscreen && _buttons.Values.All(x => !x.IsFullscreen))
                        IsFullscreen = false;
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
        }

        public void Restore(IntPtr hwnd)
        {
            if (IsDisposed || Disposing)
                return;

            Task.Factory.StartNew(() =>
            {
                CustomButton button;

                if (!_buttons.TryGetValue(hwnd, out button))
                {
                    if (_isFullscreen && _buttons.Values.All(x => !x.IsFullscreen))
                        IsFullscreen = false;

                    return;
                }

                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    button.Lock();
                    if (button.IsMinimized)
                    {
                        button.IsMinimized = false;
                        if (button.WasFullscreen && !button.IsFullscreen)
                        {
                            button.IsFullscreen = true;
                            IsFullscreen = true;
                        }
                    }
                    button.Release();

                    if (_isFullscreen && _buttons.Values.All(x => !x.IsFullscreen))
                        IsFullscreen = false;
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, TaskScheduler.Default);
        }

        public Rectangle GetMinRect(IntPtr hwnd)
        {
            if (IsDisposed || Disposing)
                return Rectangle.Empty;

            CustomButton button;

            if (!_buttons.TryGetValue(hwnd, out button))
                return Rectangle.Empty;

            return button.RectangleToScreen(button.Bounds);
        }

        internal static void RestoreWindow(IntPtr hwnd, bool sendSyscommand = false)
        {
            if (hwnd == IntPtr.Zero)
                return;

            if (IsIconic(hwnd))
            {
                const int SW_RESTORE = 9;
                NativeMethods.ShowWindow(hwnd, SW_RESTORE);

                if (sendSyscommand)
                    NativeMethods.PostMessage(hwnd, NativeMethods.WM_SYSCOMMAND, NativeMethods.SC_RESTORE, 0);
            }

            NativeMethods.SetForegroundWindow(hwnd);
        }

        private void Reset()
        {
            flowLayoutPanel1.Controls.Clear();

            foreach (var button in _buttons.Values)
            {
                try
                {
                    if (button.Image != null)
                    {
                        button.Image.Dispose();
                        button.Image = null;
                    }
                    button.MouseEnter -= Button_MouseEnter;
                    button.MouseMove -= Button_MouseMove;
                    button.MouseLeave -= Button_MouseLeave;
                    button.MouseDown -= Button_MouseDown;
                    button.MouseUp -= Button_MouseUp;
                    button.Dispose();
                }
                catch { }
            }

            _buttons.Clear();
        }

        public new void Hide()
        {
            base.Hide();

            DestroyHandle();
            if (Container != null)
                Container.Remove(this);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (!IsLocked)
            {
                if (Visible)
                    IsFullscreen = false;
                else
                    Reset();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _lastTooltipMousePosition = null;
            _toolTip.Active = false;
            try
            {
                _toolTip.Hide(this);
            }
            catch { }

            if (dwmPreview.Visible)
            {
                dwmPreview.PreviewWindow = IntPtr.Zero;
                dwmPreview.Hide();
            }

            base.OnMouseLeave(e);
        }

        private void ToolTipTimer_Tick(object sender, EventArgs e)
        {
            var timer = sender as Timer;
            timer.Stop();

            _toolTip.Active = false;
            try
            {
                _toolTip.Hide(this);
            }
            catch { }

            if (dwmPreview.Visible)
            {
                dwmPreview.PreviewWindow = IntPtr.Zero;
                dwmPreview.Hide();
            }

            var control = timer.Tag as Control;
            if (control == null || control.IsDisposed || control.Disposing)
                return;

            var area = _screen.WorkingArea;
            var point = PointToScreen(control.Location);
            if (point.X < area.Left)
                point.X = area.Left;
            if (point.X > area.Right)
                point.X = area.Right;
            if (point.Y < area.Top)
                point.Y = area.Top;
            if (point.Y > area.Bottom)
                point.Y = area.Bottom;

            if (control is CustomButton)
            {
                var button = control as CustomButton;
                var hwnd = (IntPtr)button.Tag;
                dwmPreview.PreviewWindow = hwnd;
                dwmPreview.Location = new Point(point.X, point.Y - dwmPreview.Height);
                dwmPreview.Show(this);
            }

            point = PointToClient(point);
            if (dwmPreview.Visible)
                point.Y -= dwmPreview.Height + 24;

            _toolTip.Active = true;
            _toolTip.Show(control.Text, this, point, _toolTip.AutoPopDelay);
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            _dragDropTimer.Stop();
            _toolTipTimer.Stop();

            if (!_lastTooltipMousePosition.HasValue)
            {
                _toolTipTimer.Tag = sender;
                _toolTipTimer.Start();
            }

            _toolTip.Active = false;
            try
            {
                _toolTip.Hide(this);
            }
            catch { }

            if (dwmPreview.Visible)
            {
                dwmPreview.PreviewWindow = IntPtr.Zero;
                dwmPreview.Hide();
            }
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            _dragDropTimer.Stop();
            _toolTipTimer.Stop();

            _lastTooltipMousePosition = null;

            _toolTip.Active = false;
            try
            {
                _toolTip.Hide(this);
            }
            catch { }

            if (dwmPreview.Visible)
            {
                dwmPreview.PreviewWindow = IntPtr.Zero;
                dwmPreview.Hide();
            }
        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            if (_lastTooltipMousePosition.HasValue && _lastTooltipMousePosition.Value == e.Location)
                return;

            _lastTooltipMousePosition = e.Location;
            _toolTipTimer.Stop();
            _toolTipTimer.Start();
        }

        private void DragDropTimer_Tick(object sender, EventArgs e)
        {
            var timer = sender as Timer;
            timer.Stop();

            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
            {
                if (!timer.Enabled)
                    flowLayoutPanel1.DoDragDrop(timer.Tag, DragDropEffects.Move);
            })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            var button = sender as CustomButton;

            if (e.Button == MouseButtons.Left)
            {
                if (e.Clicks == 1)
                {
                    if (button.IsActive)
                    {
                        _dragDropTimer.Tag = sender;
                        _dragDropTimer.Start();
                    }
                }
            }
        }

        private void Button_MouseUp(object sender, MouseEventArgs e)
        {
            Button_MouseLeave(null, EventArgs.Empty);

            var button = sender as CustomButton;

            if (e.Button == MouseButtons.Left)
            {
                _dragDropTimer.Stop();

                if (e.Clicks == 1)
                {
                    var hwnd = (IntPtr)button.Tag;

                    Task.Factory.StartNew(() =>
                    {
                        uint procId;
                        NativeMethods.GetWindowThreadProcessId(hwnd, out procId);

                        var sendSyscommand = false;

                        try
                        {
                            using (var p = Process.GetProcessById(Convert.ToInt32(procId)))
                            {
                                var pHwnd = p.MainWindowHandle;
                                if (pHwnd != IntPtr.Zero && pHwnd != hwnd)
                                    hwnd = pHwnd;

                                using (var main = p.MainModule)
                                    if (main == null)
                                        throw new AccessViolationException();
                            }
                        }
                        catch
                        {
                            sendSyscommand = true;
                        }

                        Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                        {
                            RestoreWindow(hwnd, sendSyscommand);
                            Application.DoEvents();

                            if (button.WasFullscreen && !button.IsFullscreen)
                                button.IsFullscreen = true;

                            if (!_isFullscreen && button.IsFullscreen)
                                IsFullscreen = true;
                        })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                }
            }
        }

        private const int APPBAR_CALLBACK = NativeMethods.WM_USER + 1010;
        //private int APPBAR_CALLBACK;

        private void AddAppBar()
        {
            RemoveAppBar();

            if (!IsHandleCreated)
                return;

            /*if (APPBAR_CALLBACK == 0)
                APPBAR_CALLBACK = NativeMethods.RegisterWindowMessage("AppBarMessage");*/

            if (_appBarHandle == IntPtr.Zero)
            {
                var abd = new APPBARDATA
                {
                    cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
                    hWnd = _appBarHandle = Handle,
                    uCallbackMessage = APPBAR_CALLBACK
                };

                SHAppBarMessage(AppbarMessages.ABM_NEW, ref abd);
            }
        }

        private void UpdateBackgroundImage()
        {
            if (BackgroundImage == null || BackgroundImage.Size != _appBarBounds.Size || Bounds != _appBarBounds)
            {
                if (BackgroundImage != null)
                    BackgroundImage.Dispose();

                if (_appBarBounds.IsEmpty)
                    BackgroundImage = null;
                else
                    BackgroundImage = new Bitmap(_appBarBounds.Width, _appBarBounds.Height, PixelFormat.Format32bppArgb);
            }
        }

        internal void UpdateAppBar()
        {
            if (!IsHandleCreated)
            {
                _appBarBounds = Rectangle.Empty;
                UpdateBackgroundImage();
                return;
            }

            if (_appBarHandle == IntPtr.Zero)
            {
                _appBarBounds = Rectangle.Empty;
                UpdateBackgroundImage();
                return;
            }

            var bounds = _screen.Bounds;
            var area = _screen.WorkingArea;

            if (bounds.IsEmpty || area.IsEmpty)
            {
                _appBarBounds = Rectangle.Empty;
                UpdateBackgroundImage();
                return;
            }

            var leftDockedWidth = Math.Abs(Math.Abs(bounds.Left) - Math.Abs(area.Left));
            var topDockedHeight = Math.Abs(Math.Abs(bounds.Top) - Math.Abs(area.Top));
            var rightDockedWidth = bounds.Width - leftDockedWidth - area.Width;
            var bottomDockedHeight = bounds.Height - topDockedHeight - area.Height;

            var edge = AppbarEdge.ABE_NONE;
            var rect = new RECT
            {
                left = bounds.Left,
                top = bounds.Top,
                right = bounds.Right,
                bottom = bounds.Bottom
            };

            if (leftDockedWidth > 0)
            {
                edge = AppbarEdge.ABE_LEFT;
                rect.right = area.Left;
            }
            else if (topDockedHeight > 0)
            {
                edge = AppbarEdge.ABE_TOP;
                rect.bottom = area.Top;
            }
            else if (rightDockedWidth > 0)
            {
                edge = AppbarEdge.ABE_RIGHT;
                rect.left = area.Right;
            }
            else if (bottomDockedHeight > 0)
            {
                edge = AppbarEdge.ABE_BOTTOM;
                rect.top = area.Bottom;
            }

            if (edge != AppbarEdge.ABE_NONE)
            {
                var abd = new APPBARDATA
                {
                    cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
                    hWnd = _appBarHandle,
                    rc = rect,
                    uEdge = edge
                };

                SHAppBarMessage(AppbarMessages.ABM_QUERYPOS, ref abd);
                Application.DoEvents();

                var newAppBarBounds = Rectangle.FromLTRB(abd.rc.left, abd.rc.top, abd.rc.right, abd.rc.bottom);
                if (_appBarBounds == newAppBarBounds)
                    return;

                SHAppBarMessage(AppbarMessages.ABM_SETPOS, ref abd);
                Application.DoEvents();

                /*abd.lParam = (IntPtr)AppbarFlags.ABS_ALWAYSONTOP;
                SHAppBarMessage(AppbarMessages.ABM_SETSTATE, ref abd);*/

                _appBarBounds = newAppBarBounds;
                _rowCount = Math.Max(1, (int)Math.Floor(_appBarBounds.Height / 30f));
            }
            else
                _appBarBounds = Rectangle.Empty;

            var isLeftOrRight = edge == AppbarEdge.ABE_LEFT || edge == AppbarEdge.ABE_RIGHT;

            SuspendLayout();

            flowLayoutPanel1.FlowDirection = isLeftOrRight ? FlowDirection.TopDown : FlowDirection.LeftToRight;
            switch(edge)
            {
                case AppbarEdge.ABE_LEFT:
                    Padding = new Padding(0, 0, 1, 0);
                    break;
                case AppbarEdge.ABE_TOP:
                    Padding = new Padding(0, 0, 0, 1);
                    break;
                case AppbarEdge.ABE_RIGHT:
                    Padding = new Padding(1, 0, 0, 0);
                    break;
                case AppbarEdge.ABE_BOTTOM:
                    Padding = new Padding(0, 1, 0, 0);
                    break;
            }
            tableLayoutPanel1.SetCellPosition(dateTimeWidget, new TableLayoutPanelCellPosition
            {
                Column = isLeftOrRight ? 0 : 1,
                Row = isLeftOrRight ? 1 : 0
            });

            UpdateBackgroundImage();

            ResumeLayout(false);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            NativeMethods.SetWindowDisplayAffinity(Handle, NativeMethods.WDA_EXCLUDEFROMCAPTURE);

            AddAppBar();
            UpdateAppBar();
            RefreshBounds();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RefreshBounds();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            RefreshBounds();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
            {
                RefreshButtonSize();
            })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

            if (_appBarHandle != IntPtr.Zero)
            {
                var abd = new APPBARDATA
                {
                    cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
                    hWnd = _appBarHandle
                };

                SHAppBarMessage(AppbarMessages.ABM_WINDOWPOSCHANGED, ref abd);
            }
        }

        private void RemoveAppBar()
        {
            if (_appBarHandle != IntPtr.Zero)
            {
                var abd = new APPBARDATA
                {
                    cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
                    hWnd = _appBarHandle
                };

                SHAppBarMessage(AppbarMessages.ABM_REMOVE, ref abd);

                _appBarHandle = IntPtr.Zero;
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            RemoveAppBar();

            base.OnHandleDestroyed(e);
        }

        protected override void OnBackgroundImageChanged(EventArgs e)
        {
            var bgImage = BackgroundImage;
            if (bgImage != null)
            {
                var bounds = _screen.Bounds;

                using (var b = new Bitmap(bounds.Width + bounds.Left, bounds.Height + bounds.Top, bgImage.PixelFormat))
                {
                    using (var g = Graphics.FromImage(b))
                    {
                        g.SetClip(bounds, CombineMode.Replace);

                        g.SmoothingMode = SmoothingMode.None;
                        g.PixelOffsetMode = PixelOffsetMode.None;
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.CompositingQuality = CompositingQuality.HighSpeed;

                        var h = g.GetHdc();
                        if (Program.MainForm.InvokeRequired)
                            Program.MainForm.Invoke(new Action(() => NativeMethods.PaintDesktop(h)));
                        else
                            NativeMethods.PaintDesktop(h);
                        g.ReleaseHdc(h);
                    }

                    var accentColor = Color.Black;

                    uint pColor;
                    bool pOpaque;
                    if (NativeMethods.IsThemeActive() && NativeMethods.DwmGetColorizationColor(out pColor, out pOpaque) == 0)
                    {
                        pColor |= pOpaque ? 0xFF000000 : 0;
                        accentColor = Color.FromArgb(
                            (byte)((pColor & 0xFF000000) >> 24),
                            (byte)((pColor & 0x00FF0000) >> 16),
                            (byte)((pColor & 0x0000FF00) >> 8),
                            (byte)((pColor & 0x000000FF) >> 0));
                    }

                    using (var abrush = new SolidBrush(Color.FromArgb(15, accentColor)))
                    using (var brush = new SolidBrush(Color.FromArgb(235, Color.Black)))
                    using (var pen = new Pen(Color.FromArgb(175, Color.LightGray), 1))
                    using (var g = Graphics.FromImage(bgImage))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;

                        if (bgImage.Height > bgImage.Width)
                        {
                            if (_appBarBounds.Right < bounds.Right)
                                g.DrawImage(b, -bounds.Left, -bounds.Top - bounds.Height + bgImage.Height);
                            else
                                g.DrawImage(b, -bounds.Left - bounds.Width + bgImage.Width, -bounds.Top - bounds.Height + bgImage.Height);
                        }
                        else
                        {
                            if (_appBarBounds.Bottom < bounds.Bottom)
                                g.DrawImage(b, -bounds.Left - bounds.Width + bgImage.Width, -bounds.Top);
                            else
                                g.DrawImage(b, -bounds.Left - bounds.Width + bgImage.Width, -bounds.Top - bounds.Height + bgImage.Height);
                        }
                        g.FillRectangle(abrush, new Rectangle(Point.Empty, bgImage.Size));
                        g.FillRectangle(brush, new Rectangle(Point.Empty, bgImage.Size));
                        if (bgImage.Height > bgImage.Width)
                        {
                            if (_appBarBounds.Right < bounds.Right)
                                g.DrawLine(pen, new Point(bgImage.Width, 0), new Point(bgImage.Width, bgImage.Height));
                            else
                                g.DrawLine(pen, Point.Empty, new Point(0, bgImage.Height));
                        }
                        else
                        {
                            if (_appBarBounds.Bottom < bounds.Bottom)
                                g.DrawLine(pen, new Point(0, bgImage.Height), new Point(bgImage.Width, bgImage.Height));
                            else
                                g.DrawLine(pen, Point.Empty, new Point(bgImage.Width, 0));
                        }
                    }

                    GaussianBlur.Process(bgImage, 8, _cancellationTokenSource.Token);
                }
            }

            base.OnBackgroundImageChanged(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (Visible && e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;

            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            RemoveAppBar();
            Reset();
        }

        [DllImport("User32.dll", EntryPoint = "GetWindow", SetLastError = false)]
        private static extern IntPtr GetWindow([In] IntPtr hWnd, uint wCmd);

        /*[DllImport("User32.dll", EntryPoint = "GetWindow", SetLastError = false)]
        private static extern IntPtr GetParent([In] IntPtr hWnd);*/

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!IsHandleCreated)
                return;

            if (_appBarHandle != IntPtr.Zero)
            {
                var abd = new APPBARDATA
                {
                    cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
                    hWnd = _appBarHandle
                };

                SHAppBarMessage(AppbarMessages.ABM_ACTIVATE, ref abd);
            }

            if (IsFullscreen)
                SendToBack();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            if (!IsHandleCreated)
                return;

            if (IsFullscreen)
                SendToBack();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource.Cancel();

                RemoveAppBar();
                Reset();

                Controls.Clear();
                tableLayoutPanel1.Controls.Clear();
                tableLayoutPanel1.Dispose();
                flowLayoutPanel1.Controls.Clear();
                flowLayoutPanel1.Dispose();
                dateTimeWidget.MouseEnter -= Button_MouseEnter;
                dateTimeWidget.MouseMove -= Button_MouseMove;
                dateTimeWidget.MouseLeave -= Button_MouseLeave;
                dateTimeWidget.Dispose();

                if (BackgroundImage != null)
                {
                    BackgroundImage.Dispose();
                    BackgroundImage = null;
                }

                try
                {
                    _toolTipTimer.Tick -= ToolTipTimer_Tick;
                }
                catch { }
                _toolTipTimer.Dispose();
                _toolTip.Dispose();
                _lock.Dispose();
                _cancellationTokenSource.Dispose();
            }

            base.Dispose(disposing);
        }

        public new void BringToFront()
        {
            NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOREDRAW | NativeMethods.SWP_NOACTIVATE);
            Application.DoEvents();
        }

        public new void SendToBack()
        {
            NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_BOTTOM, 0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOREDRAW | NativeMethods.SWP_NOACTIVATE);
            Application.DoEvents();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var bgImage = BackgroundImage;
            if (bgImage != null)
            {
                var rect = e.ClipRectangle;
                e.Graphics.DrawImage(bgImage, rect, 0, 0, rect.Width, rect.Height, GraphicsUnit.Pixel);

                return;
            }

            base.OnPaintBackground(e);
        }

        private const int WM_SETTINGCHANGE = 0x001A;
        private const int SPI_SETWORKAREA = 0x002F;

        private void RefreshBounds()
        {
            if (Bounds != _appBarBounds)
            {
                Bounds = _appBarBounds;
                //NativeMethods.PostMessage(NativeMethods.HWND_BROADCAST, WM_SETTINGCHANGE, SPI_SETWORKAREA, 0);
                /*foreach (var button in _buttons.Values)
                    NativeMethods.PostMessage((IntPtr)button.Tag, WM_SETTINGCHANGE, SPI_SETWORKAREA, 0);*/
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == APPBAR_CALLBACK)
            {
                switch ((AppbarMsg)m.WParam.ToInt32())
                {
                    case AppbarMsg.ABN_WINDOWARRANGE:
                    case AppbarMsg.ABN_FULLSCREENAPP:
                        var state = m.LParam.ToInt32() != 0; // true
                        if (IsHandleCreated)
                            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                            {
                                IsFullscreen = state;
                            })), endInvoke => IsHandleCreated ? EndInvoke(endInvoke) : null, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                        break;
                    case AppbarMsg.ABN_STATECHANGE:
                    case AppbarMsg.ABN_POSCHANGED:
                        if (IsHandleCreated)
                            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                            {
                                UpdateAppBar();
                                RefreshBounds();
                            })), endInvoke => IsHandleCreated ? EndInvoke(endInvoke) : null, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                        break;
                }
            }
            else
            {

                switch (m.Msg)
                {
                    case NativeMethods.WM_WINDOWPOSCHANGING:
                        var winpos = (NativeMethods.WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.WINDOWPOS));
                        winpos.hwndInsertAfter = _isFullscreen ? NativeMethods.HWND_BOTTOM : NativeMethods.HWND_TOPMOST;
                        Marshal.StructureToPtr(winpos, m.LParam, false);
                        break;
                    case WM_SETTINGCHANGE:
                        if (m.WParam.ToInt32() == SPI_SETWORKAREA)
                        {
                            if (IsHandleCreated)
                                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                                {
                                    UpdateAppBar();
                                    RefreshBounds();
                                })), endInvoke => IsHandleCreated ? EndInvoke(endInvoke) : null, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                        }
                        break;
                    case NativeMethods.WM_NCPAINT:
                        /*var v = (int)NativeMethods.DWMNCRENDERINGPOLICY.DWMNCRP_ENABLED;
                        NativeMethods.DwmSetWindowAttribute(Handle, NativeMethods.DWMWINDOWATTRIBUTE.WCA_NCRENDERING_POLICY, ref v, sizeof(int));*/

                        /*var margins = new NativeMethods.MARGINS
                        {
                            cxLeftWidth = -1,
                            cxRightWidth = -1,
                            cyTopHeight = -1,
                            cyBottomHeight = -1
                        };
                        NativeMethods.DwmExtendFrameIntoClientArea(Handle, ref margins);

                        var accPolicy = new NativeMethods.AccentPolicy
                        {
                            AccentState = NativeMethods.DWMACCENTSTATE.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                            GradientColor = Color.FromArgb(100, Color.Black).ToAbgr(),
                            //AccentFlags = 2
                        };

                        var accentSize = Marshal.SizeOf(accPolicy);
                        var accentPtr = Marshal.AllocHGlobal(accentSize);
                        Marshal.StructureToPtr(accPolicy, accentPtr, false);
                        var data = new NativeMethods.WindowCompositionAttributeData
                        {
                            Attribute = NativeMethods.DWMWINDOWATTRIBUTE.WCA_ACCENT_POLICY,
                            Data = accentPtr,
                            SizeOfData = accentSize
                        };

                        NativeMethods.SetWindowCompositionAttribute(Handle, ref data);
                        Marshal.FreeHGlobal(accentPtr);*/

                        /*var bbh = new NativeMethods.DWM_BLURBEHIND
                        {
                            fEnable = true,
                            dwFlags = NativeMethods.DWM_BB.DWM_BB_ENABLE
                        };
                        NativeMethods.DwmEnableBlurBehindWindow(Handle, ref bbh);*/
                        break;
                }
            }

            base.WndProc(ref m);
        }

        [DllImport("User32.dll", EntryPoint = "IsIconic", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        /*[DllImport("User32.dll", EntryPoint = "OpenIcon", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenIcon(IntPtr hWnd);*/

        [DllImport("Shell32.dll", EntryPoint = "SHAppBarMessage", SetLastError = true)]
        private static extern IntPtr SHAppBarMessage([MarshalAs(UnmanagedType.U4)] AppbarMessages dwMessage, ref APPBARDATA pData);

        private enum AppbarMessages : uint
        {
            ABM_NEW = 0x00000000,
            ABM_REMOVE = 0x00000001,
            ABM_QUERYPOS = 0x00000002,
            ABM_SETPOS = 0x00000003,
            ABM_GETSTATE = 0x00000004,
            ABM_GETTASKBARPOS = 0x00000005,
            ABM_ACTIVATE = 0x00000006,
            ABM_GETAUTOHIDEBAR = 0x00000007,
            ABM_SETAUTOHIDEBAR = 0x00000008,
            ABM_WINDOWPOSCHANGED = 0x0000009,
            ABM_SETSTATE = 0x0000000a,
            ABM_GETAUTOHIDEBAREX = 0x0000000b,
            ABM_SETAUTOHIDEBAREX = 0x0000000c,
        }

        [Flags]
        private enum AppbarFlags : int
        {
            ABS_AUTOHIDE = 0x0000001,
            ABS_ALWAYSONTOP = 0x0000002
        }

        private enum AppbarMsg : int
        {
            ABN_STATECHANGE = 0x0000000,
            ABN_POSCHANGED = 0x0000001,
            ABN_FULLSCREENAPP = 0x0000002,
            ABN_WINDOWARRANGE = 0x0000003
        }

        private enum AppbarEdge : uint
        {
            ABE_LEFT = 0,
            ABE_TOP = 1,
            ABE_RIGHT = 2,
            ABE_BOTTOM = 3,
            ABE_NONE = 0xff
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            [MarshalAs(UnmanagedType.U4)]
            public AppbarEdge uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

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
