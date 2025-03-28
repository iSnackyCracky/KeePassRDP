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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP
{
    internal class KprSecureLockScreen : IDisposable
    {
        private class BackgroundFormTask
        {
            public KprSecureBackgroundForm Form { get; set; }
            public ManualResetEventSlim Shown { get; set; }
            public Bitmap Bitmap { get; set; }
        }

        private readonly List<BackgroundFormTask> _backgroundForms;
        private readonly ManualResetEventSlim _backgroundFormsLock;
        private readonly ManualResetEventSlim _prepareLock;
        private readonly ManualResetEventSlim _bitmapLock;
        private readonly ManualResetEventSlim _bitmapWorkerLock;
        private CancellationTokenSource _cancellationTokenSource;

        public KprSecureLockScreen()
        {
            _backgroundForms = new List<BackgroundFormTask>();
            _backgroundFormsLock = new ManualResetEventSlim(false);
            _prepareLock = new ManualResetEventSlim(false);
            _bitmapLock = new ManualResetEventSlim(false);
            _bitmapWorkerLock = new ManualResetEventSlim(false);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Lock()
        {
            foreach (var bmpBackTask in _backgroundForms.Where(x => x.Shown.IsSet && x.Form.Visible))
            {
                var formBack = bmpBackTask.Form;
                formBack.BringToFront();

                if (bmpBackTask.Bitmap != null && Screen.FromControl(formBack).Primary)
                {
                    formBack.Invalidate();
                    formBack.Update();
                    Application.DoEvents();

                    Task.Factory.StartNew(() =>
                    {
                        var bitmap = (Bitmap)bmpBackTask.Bitmap.Clone();

                        using (var f0 = new Font("Segoe UI", 32, FontStyle.Regular, GraphicsUnit.Point))
                        using (var f1 = new Font("Segoe UI Semibold", 26, FontStyle.Regular, GraphicsUnit.Point))
                        using (var f2 = new Font("Segoe UI", 14, FontStyle.Regular, GraphicsUnit.Point))
                        using (var sf = new StringFormat(StringFormat.GenericTypographic)
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Near,
                            FormatFlags = StringFormatFlags.NoWrap,
                            Trimming = StringTrimming.None,
                            HotkeyPrefix = HotkeyPrefix.None
                        })
                        using (var g = Graphics.FromImage(bitmap))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                            g.TextContrast = 0;
                            var username = Environment.UserName;
                            var t1 = KprResourceManager.Instance["Locked"];
                            var t2 = KprResourceManager.Instance["Press Ctrl+Alt+Del to unlock."];
                            var s0 = g.MeasureString(username, f0, SizeF.Empty, sf); //TextRenderer.MeasureText(g, username, f0, Size.Empty, TextFormatFlags.Default | TextFormatFlags.SingleLine);
                            var s1 = g.MeasureString(t1, f1, SizeF.Empty, sf); //TextRenderer.MeasureText(g, t1, f1, Size.Empty, TextFormatFlags.Default | TextFormatFlags.SingleLine);
                            var s2 = g.MeasureString(t2, f2, SizeF.Empty, sf); //TextRenderer.MeasureText(g, t2, f2, Size.Empty, TextFormatFlags.Default | TextFormatFlags.SingleLine);
                            var y = bitmap.Height / 2f - (s0.Height + s1.Height + s2.Height) / 2f;
                            var hw = bitmap.Width / 2f;
                            using (var brush = new SolidBrush(Color.FromArgb(200, Color.White)))
                            {
                                g.DrawString(username, f0, brush, new RectangleF(new PointF(hw - s0.Width / 2f, y + 4), s0), sf); //TextRenderer.DrawText(g, username, f0, new Point(hw - s0.Width / 2, y - 4), Color.FromArgb(200, Color.White));
                                g.DrawString(t1, f1, brush, new RectangleF(new PointF(hw - s1.Width / 2f, y + 12 + s0.Height), s1), sf); //TextRenderer.DrawText(g, t1, f1, new Point(hw - s1.Width / 2, y + 8 + s0.Height), Color.FromArgb(200, Color.White));
                                g.DrawString(t2, f2, brush, new RectangleF(new PointF(hw - s2.Width / 2f, y + 12 + s0.Height + s1.Height + 4), s2), sf); //TextRenderer.DrawText(g, t2, f2, new Point(hw - s2.Width / 2, y + 8 + s0.Height + s1.Height + 4), Color.FromArgb(150, Color.White));
                            }

                            var sb = new StringBuilder(512);
                            GetUserTile(username, 0x80000000, sb, sb.Capacity);
                            try
                            {
                                using (var userTile = Image.FromFile(sb.ToString()))
                                {
                                    var colorMatrix = new ColorMatrix
                                    {
                                        Matrix33 = .8f
                                    };
                                    var imageAttributes = new ImageAttributes();
                                    imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                                    var r = Math.Min(208, Math.Max(userTile.Width, userTile.Height));
                                    var rect = new RectangleF(hw - r / 2, y - r - 4, r, r);
                                    using (var gp = new GraphicsPath())
                                    {
                                        gp.AddEllipse(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8);
                                        using (var brush = new SolidBrush(Color.FromArgb(150, Color.White)))
                                        using (var pen = new Pen(brush, 2))
                                        {
                                            g.FillPath(brush, gp);
                                            g.DrawPath(pen, gp);
                                        }
                                        g.SetClip(gp, CombineMode.Replace);
                                        g.DrawImage(userTile, Rectangle.Round(rect), 0, 0, userTile.Width, userTile.Height, GraphicsUnit.Pixel, imageAttributes);
                                    }
                                }
                            }
                            catch { }
                        }
                        formBack.SuspendLayout();
                        formBack.BackgroundImage = bitmap;
                        formBack.ResumeLayout(false);

                        formBack.Invalidate();
                        formBack.Update();

                        Application.DoEvents();
                    }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                }
                else
                {
                    formBack.SuspendLayout();
                    formBack.BackgroundImage = null;
                    formBack.ResumeLayout(false);
                }
            }
        }

        public void Unlock()
        {
            foreach (var bmpBackTask in _backgroundForms.Where(x => x.Shown.IsSet && x.Form.Visible))
            {
                var formBack = bmpBackTask.Form;
                formBack.SendToBack();
            }

            ClearBackgroundForms();
        }

        public void Show(IWin32Window owner)
        {
            if (!_prepareLock.IsSet)
            {
                _prepareLock.Set();
                Prepare();
                RefreshBackgrounds();
            }

            Application.DoEvents();

            if (_backgroundForms.Any(x => !x.Shown.IsSet || !x.Form.Visible))
            {
                foreach (var formBackTask in _backgroundForms.Where(x => !x.Shown.IsSet || !x.Form.Visible))
                {
                    var formBack = formBackTask.Form;
                    if (!formBack.Visible)
                        formBack.Show(owner);
                    else if (!formBackTask.Shown.IsSet)
                        formBackTask.Shown.Set();
                }

                Application.DoEvents();

                var i = _backgroundForms.Where(x => !x.Shown.IsSet).Count() * 1000;
                while (!_backgroundForms.All(x => x.Shown.IsSet) && i-- >= 0)
                    foreach (var formBackTask in _backgroundForms.Where(x => !x.Shown.IsSet))
                        if (!formBackTask.Shown.IsSet)
                        {
                            Application.DoEvents();
                            if (!formBackTask.Shown.IsSet)
                                formBackTask.Shown.Wait(TimeSpan.FromMilliseconds(1), _cancellationTokenSource.Token);
                        }
            }
        }

        private void Prepare()
        {
            if (_bitmapWorkerLock.IsSet)
                return;

            _bitmapWorkerLock.Set();
            _bitmapLock.Reset();

            if (_backgroundForms.Count != Screen.AllScreens.Length)
            {
                ClearBackgroundForms();
                _backgroundForms.AddRange(Screen.AllScreens.Select(x =>
                {
                    var bgForm = new KprSecureBackgroundForm(null, x);
                    var mrs = new ManualResetEventSlim(false);
                    EventHandler ev = null;
                    bgForm.Shown += ev = (s, e) =>
                    {
                        mrs.Set();
                        bgForm.Shown -= ev;
                        ev = null;
                    };
                    return new BackgroundFormTask
                    {
                        Form = bgForm,
                        Shown = mrs,
                        Bitmap = null
                    };
                }));
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Parallel.ForEach(_backgroundForms, new ParallelOptions
                    {
                        CancellationToken = _cancellationTokenSource.Token,
                        MaxDegreeOfParallelism = _backgroundForms.Count,
                        TaskScheduler = TaskScheduler.Default
                    }, (formBackTask) =>
                    {
                        Bitmap bmpBack = null;
                        var formBack = formBackTask.Form;
                        var screen = Screen.FromPoint(Point.Add(formBack.Location, new Size(formBack.Width / 2, formBack.Height / 2)));

                        if (!screen.Primary)
                        {
                            if (formBackTask.Bitmap != null)
                            {
                                formBackTask.Bitmap.Dispose();
                                formBackTask.Bitmap = null;
                            }
                            return;
                        }

                        var bounds = screen.Bounds;
                        using (var b = new Bitmap(bounds.Width + bounds.Left, bounds.Height + bounds.Top, PixelFormat.Format32bppArgb))
                        {
                            using (var g = Graphics.FromImage(b))
                            {
                                g.SetClip(bounds, CombineMode.Replace);

                                var h = g.GetHdc();

                                var result = false;
                                if (Program.MainForm.InvokeRequired)
                                    result = (bool)Program.MainForm.Invoke(new Func<bool>(() => NativeMethods.PaintDesktop(h)));
                                else
                                    result = NativeMethods.PaintDesktop(h);

                                g.ReleaseHdc(h);

                                if (result)
                                {
                                    using (var brush = new SolidBrush(Color.FromArgb(192, Color.Black)))
                                        g.FillRectangle(brush, new Rectangle(Point.Empty, b.Size));

                                    bmpBack = new Bitmap(bounds.Width, bounds.Height, b.PixelFormat);
                                }
                            }

                            if (bmpBack != null)
                            {
                                if (screen.Primary)
                                    GaussianBlur.Process(b, 12, _cancellationTokenSource.Token);

                                using (var g = Graphics.FromImage(bmpBack))
                                {
                                    g.SmoothingMode = SmoothingMode.None;
                                    g.PixelOffsetMode = PixelOffsetMode.None;
                                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                                    g.CompositingQuality = CompositingQuality.HighSpeed;

                                    g.DrawImage(b, -bounds.Left, -bounds.Top);
                                }
                            }
                        }

                        formBackTask.Bitmap = bmpBack;
                    });

                    _bitmapLock.Set();
                }
                finally
                {
                    _bitmapWorkerLock.Reset();
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void RefreshBackgrounds()
        {
            if (!_bitmapLock.IsSet)
                try
                {
                    if (!_bitmapLock.Wait(TimeSpan.FromSeconds(5), _cancellationTokenSource.Token))
                        return;
                }
                catch
                {
                    return;
                }

            var disposableImages = new List<Image>();
            foreach (var formBackTask in _backgroundForms)
            {
                var formBack = formBackTask.Form;
                var bmpBack = formBackTask.Bitmap;
                var oldImage = formBack.BackgroundImage;
                if (bmpBack != oldImage)
                {
                    formBack.SuspendLayout();
                    formBack.BackgroundImage = bmpBack;
                    formBack.ResumeLayout(false);
                    if (oldImage != null)
                        disposableImages.Add(oldImage);
                }

                if (!formBack.Created)
                    formBack.CreateControl();
            }

            if (disposableImages.Count > 0)
            {
                Task.Factory.StartNew(() =>
                {
                    foreach (var image in disposableImages)
                        image.Dispose();
                    disposableImages.Clear();
                }, CancellationToken.None, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }
        }

        private void ClearBackgroundForms()
        {
            if (_backgroundFormsLock.IsSet)
                return;

            _backgroundFormsLock.Set();
            _prepareLock.Reset();

            try
            {
                foreach (var formBackTask in _backgroundForms)
                {
                    var formBack = formBackTask.Form;
                    try
                    {
                        using (formBackTask.Shown)
                        {
                            try
                            {
                                formBack.Hide();
                                formBack.Close();
                            }
                            catch { }
                            try
                            {
                                var bmp = formBack.BackgroundImage;
                                if (bmp != null)
                                    using (bmp)
                                        formBack.BackgroundImage = null;
                            }
                            catch { }
                        }
                    }
                    catch { }
                    finally
                    {
                        UIUtil.DestroyForm(formBack);
                    }
                }

                _backgroundForms.Clear();
            }
            finally
            {
                _backgroundFormsLock.Reset();
            }
        }

        public void Dispose()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();

            ClearBackgroundForms();

            _backgroundFormsLock.Dispose();
            _prepareLock.Dispose();
            _bitmapLock.Dispose();
            _bitmapWorkerLock.Dispose();
        }

        [DllImport("Shell32.dll", EntryPoint = "#261", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void GetUserTile(string username, uint flags, StringBuilder path, int maxLength);
    }
}