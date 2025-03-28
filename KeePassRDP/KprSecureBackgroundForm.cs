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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KeePassRDP
{
    internal class KprSecureBackgroundForm : Form
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

        private readonly bool _isPrimary;

        public KprSecureBackgroundForm(Bitmap bmpBackground) : this(bmpBackground, Screen.PrimaryScreen)
        { }

        public KprSecureBackgroundForm(Bitmap bmpBackground, Screen sc)
        {
            var screen = sc ?? Screen.PrimaryScreen;
            _isPrimary = screen.Primary;
            TopLevel = true;
            ControlBox = false;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Location = screen.Bounds.Location;
            Size = screen.Bounds.Size;
            DoubleBuffered = true;
            BackColor = Color.Black;
            BackgroundImageLayout = ImageLayout.None;
            if (bmpBackground != null)
                BackgroundImage = bmpBackground;
        }

        protected override void OnPaint(PaintEventArgs e)
        { }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var bgImage = BackgroundImage;
            if (bgImage != null)
            {
                var rect = e.ClipRectangle;
                e.Graphics.DrawImage(bgImage, rect, 0, 0, rect.Width, rect.Height, GraphicsUnit.Pixel);
                return;
            }

            if (_isPrimary)
            {
                var state = e.Graphics.Save();

                e.Graphics.SmoothingMode = SmoothingMode.None;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;

                var s = Screen.FromControl(this);
                var bounds = s.Bounds;
                using (var b = new Bitmap(bounds.Width + bounds.Left, bounds.Height + bounds.Top, e.Graphics))
                {
                    using (var g = Graphics.FromImage(b))
                    {
                        g.SetClip(bounds, CombineMode.Replace);

                        /*g.SmoothingMode = SmoothingMode.None;
                        g.PixelOffsetMode = PixelOffsetMode.None;
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.CompositingQuality = CompositingQuality.HighQuality;*/

                        var h = g.GetHdc();
                        if (Program.MainForm.InvokeRequired)
                            Program.MainForm.Invoke(new Action(() => NativeMethods.PaintDesktop(h)));
                        else
                            NativeMethods.PaintDesktop(h);
                        g.ReleaseHdc(h);

                        using (var brush = new SolidBrush(Color.FromArgb(192, Color.Black)))
                            g.FillRectangle(brush, e.ClipRectangle);
                    }

                    e.Graphics.DrawImage(b, -bounds.Left, -bounds.Top);
                }

                e.Graphics.Restore(state);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (Visible && e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;

            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (BackgroundImage != null)
                {
                    BackgroundImage.Dispose();
                    BackgroundImage = null;
                }
            }

            base.Dispose(disposing);
        }

        public new void Hide()
        {
            base.Hide();

            DestroyHandle();
            if (Container != null)
                Container.Remove(this);
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
    }
}