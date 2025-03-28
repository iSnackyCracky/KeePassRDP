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

using KeePassRDP.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace KeePassRDP
{
    public partial class KprPictureBox : PictureBox
    {
        private const int _interval = 333;

        private readonly Timer _timer;
        private readonly Random _rnd;
        private readonly Bitmap _bitmap;
        private readonly int _perPixel;
        private readonly LinkedList<Point> _snake;
        private readonly Cursor _greenCursor;
        private readonly Cursor _yellowCursor;
        private readonly Cursor _redCursor;

        private Cursor _currentCursor;
        private Point _pos;
        private PointF _scale;
        private PointF _offset;
        private Rectangle _rect;

        private bool _isPainting;
        private bool _needsApple;
        private bool _paused;
        private int _totalScore;
        private int _nextScore;
        private Direction _direction;

        private enum Direction : int
        {
            None = 0,
            Up = 1,
            Down = -1,
            Left = 2,
            Right = -2
        }

        public KprPictureBox()
        {
            InitializeComponent();

            _timer = new Timer
            {
                Interval = _interval,
                Enabled = false
            };

            _timer.Tick += Tick;

            _rnd = new Random();

            _bitmap = new Bitmap(32, 32, PixelFormat.Format32bppRgb);
            _perPixel = Image.GetPixelFormatSize(_bitmap.PixelFormat) / 8;

            _pos = new Point(_bitmap.Width / 2, _bitmap.Height / 2);

            _scale = new PointF(
                Math.Min(Width, Height) / (float)_bitmap.Width,
                Math.Min(Width, Height) / (float)_bitmap.Height);
            _offset = new PointF(
                (Size.Width - _bitmap.Width * _scale.X) / 2,
                (Size.Height - _bitmap.Height * _scale.Y) / 2);
            _rect = new Rectangle
            {
                Width = (int)Math.Floor(_scale.X) + 2,
                Height = (int)Math.Floor(_scale.Y) + 2,
            };

            _snake = new LinkedList<Point>();
            _isPainting = false;
            _needsApple = true;
            _paused = true;
            _direction = Direction.None;
            _totalScore = 0;
            _nextScore = 32;

            using (var icon = Icon.FromHandle(Cursors.Default.Handle))
            using (var bGreen = new Bitmap(icon.Width, icon.Height + 8, PixelFormat.Format32bppArgb))
            using (var bYellow = new Bitmap(icon.Width, icon.Height + 8, PixelFormat.Format32bppArgb))
            using (var bRed = new Bitmap(icon.Width, icon.Height + 8, PixelFormat.Format32bppArgb))
            {
                foreach (var b in new[] { bGreen, bYellow, bRed })
                    using (var g = Graphics.FromImage(b))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.DrawIconUnstretched(icon, new Rectangle(new Point(16, 20), icon.Size));
                    }

                var green = Color.FromArgb(255, 0, 255, 0);
                var yellow = Color.FromArgb(255, 255, 255, 0);
                var red = Color.FromArgb(255, 255, 0, 0);

                for (var x = 0; x < bGreen.Width; x++)
                    for (var y = 0; y < bGreen.Height; y++)
                    {
                        var p = bGreen.GetPixel(x, y);
                        if (p.A == 255)
                        {
                            bGreen.SetPixel(x, y, green);
                            bYellow.SetPixel(x, y, yellow);
                            bRed.SetPixel(x, y, red);
                        }
                        else if (p.R == 255 && p.G == 255 && p.B == 255)
                        {
                            bGreen.SetPixel(x, y, Color.FromArgb(p.A, green.R, green.G, green.B));
                            bYellow.SetPixel(x, y, Color.FromArgb(p.A, yellow.R, yellow.G, yellow.B));
                            bRed.SetPixel(x, y, Color.FromArgb(p.A, red.R, red.G, red.B));
                        }
                    }

                _greenCursor = new Cursor(bGreen.GetHicon());
                _yellowCursor = new Cursor(bYellow.GetHicon());
                _currentCursor = _redCursor = new Cursor(bRed.GetHicon());

                IconUtil.DestroyIcon(icon.Handle);
            }
        }

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                _timer.Tick -= Tick;
                base.Dispose(disposing);
                _timer.Dispose();
                _bitmap.Dispose();
                _currentCursor = null;
                _greenCursor.Dispose();
                _yellowCursor.Dispose();
                _redCursor.Dispose();
            }
            else
                base.Dispose(disposing);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var x = (float)e.Location.X;
            var y = (float)e.Location.Y;

            var bitmap = Image as Bitmap;
            var width = (float)bitmap.Width;
            var height = (float)bitmap.Height;

            if (bitmap == _bitmap)
            {
                width *= _scale.X;
                height *= _scale.Y;
            }

            var scale = Math.Min(Width / width, Height / height);

            if (Height >= bitmap.Height)
                scale = 1;

            var offset = new PointF(
                (Size.Width - width * scale) / 2,
                (Size.Height - height * scale) / 2);

            x -= offset.X;
            y -= offset.Y;

            x /= scale;
            y /= scale;

            if (bitmap == _bitmap)
            {
                x /= _scale.X;
                y /= _scale.Y;
                width /= _scale.X;
                height /= _scale.Y;
            }

            if (x < 0 || y < 0 || x >= width || y >= height)
            {
                if (Cursor == _currentCursor)
                    Cursor = null;
            }
            else
            {
                if (Image.IsAlphaPixelFormat(bitmap.PixelFormat))
                {
                    var bits = bitmap.LockBits(new Rectangle(new Point((int)x, (int)y), new Size(1, 1)), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                    var alpha = Marshal.ReadByte(IntPtr.Add(bits.Scan0, 3));
                    bitmap.UnlockBits(bits);
                    if (Cursor != _currentCursor && alpha >= 200)
                        Cursor = _currentCursor;
                    else if (alpha < 200)
                        Cursor = null;
                }
                else if (Cursor != _currentCursor)
                    Cursor = _currentCursor;
            }

            base.OnMouseMove(e);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            if (Image == _bitmap)
                return;

            if (Cursor != _currentCursor)
                return;

            if (Cursor == _redCursor)
            {
                Cursor = _currentCursor = _yellowCursor;
                return;
            }

            if (Cursor == _yellowCursor)
            {
                Cursor = _currentCursor = _greenCursor;
                return;
            }

            var bits = _bitmap.LockBits(new Rectangle(_pos, new Size(1, 1)), ImageLockMode.ReadWrite, _bitmap.PixelFormat);
            Marshal.WriteByte(bits.Scan0, 1, 255);
            _bitmap.UnlockBits(bits);

            _snake.AddLast(new Point(_pos.X, _pos.Y));

            Font = new Font(FontFamily.Families.FirstOrDefault(x => x.Name == "Consolas") ?? FontFamily.GenericMonospace, 12, FontStyle.Regular, GraphicsUnit.Pixel);
            Image = _bitmap;
            _paused = false;
            _timer.Start();

            Focus();
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (!Focused && _direction != Direction.None)
                _paused = false;
            Focus();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            if (_direction != Direction.None)
                _paused = true;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            _scale.X = Math.Min(Width, Height) / (float)_bitmap.Width;
            _scale.Y = Math.Min(Width, Height) / (float)_bitmap.Height;

            _offset.X = (Width - _bitmap.Width * _scale.X) / 2;
            _offset.Y = (Height - _bitmap.Height * _scale.Y) / 2;

            _rect.Width = (int)Math.Floor(_scale.X) + 2;
            _rect.Height = (int)Math.Floor(_scale.Y) + 2;

            if (_paused || _direction == Direction.None || !_timer.Enabled)
                return;

            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!_paused || e.KeyCode == Keys.Space)
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        _direction = Direction.Up;
                        return;
                    case Keys.Down:
                        _direction = Direction.Down;
                        return;
                    case Keys.Left:
                        _direction = Direction.Left;
                        return;
                    case Keys.Right:
                        _direction = Direction.Right;
                        return;
                    case Keys.Space:
                        _paused = !_paused;
                        return;
                }

            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (_isPainting)
                return;

            if (Image != _bitmap)
            {
                base.OnPaint(pe);
                return;
            }

            var state = pe.Graphics.Save();

            pe.Graphics.SmoothingMode = SmoothingMode.None;
            pe.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            pe.Graphics.CompositingMode = CompositingMode.SourceCopy;
            pe.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            pe.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            pe.Graphics.TextRenderingHint = TextRenderingHint.SystemDefault;

            base.OnPaint(pe);

            if (_direction != Direction.None && !_timer.Enabled)
            {
                pe.Graphics.Restore(state);

                pe.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                pe.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                pe.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                pe.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                pe.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                using (var sf = new StringFormat(StringFormat.GenericTypographic)
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    HotkeyPrefix = HotkeyPrefix.None,
                    Trimming = StringTrimming.None
                })
                {
                    var font = Font;
                    var text = string.Format("Game over!{0}{1}: {2}", Environment.NewLine, KprResourceManager.Instance["Score"], _totalScore);
                    var size = pe.Graphics.MeasureString(text, font, SizeF.Empty, sf);
                    if (Size.IsEmpty || size.IsEmpty)
                    {
                        pe.Graphics.Restore(state);
                        return;
                    }
                    var frect = new RectangleF(new PointF(Width / 2f - size.Width / 2f, Height / 2f - size.Height / 2f), size);
                    var rrect = Rectangle.Round(frect);
                    rrect.Inflate(6, 6);
                    using (var brush = new SolidBrush(Color.FromArgb(200, Color.DarkOrange)))
                        pe.Graphics.FillRectangle(brush, rrect);
                    frect.Inflate(.5f, .5f);
                    frect.Offset(1, 1);
                    pe.Graphics.DrawString(text, font, Brushes.White, frect, sf);
                    rrect.Inflate(-1, -1);
                    pe.Graphics.DrawRectangle(new Pen(Color.White, 2), rrect);
                    if (!pe.ClipRectangle.Contains(rrect))
                        Invalidate(rrect);
                    else
                        Update();
                }
            }

            pe.Graphics.Restore(state);
        }

        private void Tick(object sender, EventArgs e)
        {
            if (_isPainting || _paused || _direction == Direction.None)
                return;

            _isPainting = true;
            SuspendLayout();

            var bits = _bitmap.LockBits(
                new Rectangle(Point.Empty, _bitmap.Size),
                ImageLockMode.ReadWrite,
                _bitmap.PixelFormat);

            var ptr = bits.Scan0;
            var width = bits.Width;
            var height = bits.Height;
            var stride = bits.Stride;

            _pos.X -= (int)_direction / 2;
            _pos.Y -= (int)_direction % 2;

            if (_pos.X >= width)
                _pos.X = 0;
            else if (_pos.X < 0)
                _pos.X = width - 1;

            if (_pos.Y >= height)
                _pos.Y = 0;
            else if (_pos.Y < 0)
                _pos.Y = height - 1;

            var start = _snake.AddLast(new Point(_pos.X, _pos.Y));
            var off = _pos.X * _perPixel + _pos.Y * stride;

            Point? endPoint = _snake.First.Value;
            Point? apple = null;

            using (var cts = new CancellationTokenSource())
                try
                {
                    Parallel.Invoke(new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 2,
                        CancellationToken = cts.Token,
                        TaskScheduler = TaskScheduler.Default
                    }, () =>
                    {
                        if (Marshal.ReadByte(ptr, off + 2) == 255)
                        {
                            Marshal.WriteByte(ptr, off + 2, 0);
                            _needsApple = true;
                            _totalScore += Math.Max(1, _nextScore + (_interval - _timer.Interval));
                            _nextScore = 32;
                        }
                        else
                        {
                            _nextScore--;
                            _snake.RemoveFirst();
                        }

                        if (_needsApple)
                        {
                            _needsApple = false;
                            if (!cts.IsCancellationRequested)
                            {
                                _timer.Interval = Math.Max(_interval / 10, _interval - (int)(((float)_interval) * (_snake.Count / (width * height) * _interval)));

                                var napple = new Point();
                                int papple;
                                do
                                {
                                    napple.X = _rnd.Next(width);
                                    napple.Y = _rnd.Next(height);
                                    papple = napple.X * _perPixel + napple.Y * stride;
                                } while (Marshal.ReadByte(ptr, papple + 1) != 0 || Marshal.ReadByte(ptr, papple + 2) != 0);

                                Marshal.WriteByte(ptr, papple + 2, 255);
                                apple = napple;
                            }
                        }
                    }, () =>
                    {
                        var end = endPoint.Value.X * _perPixel + endPoint.Value.Y * stride;
                        Marshal.WriteByte(ptr, end + 1, 0);

                        if (Marshal.ReadByte(ptr, off + 1) != 0)
                        {
                            if (!cts.IsCancellationRequested)
                                cts.Cancel();

                            Marshal.WriteByte(ptr, off + 1, 85);
                            _timer.Enabled = _needsApple = false;
                            //Marshal.WriteByte(ptr, end + 1, 255);
                            endPoint = apple = null;
                        }
                        else
                        {
                            Marshal.WriteByte(ptr, off + 1, 255);

                            var r = Rectangle.Empty;
                            var p = start.Previous;
                            for (var i = 10; i <= 100; i += 10)
                            {
                                if (p == null || p.Value == null || p.Previous == null)
                                    break;
                                var pos = p.Value;
                                var offP = pos.X * _perPixel + pos.Y * stride;
                                Marshal.WriteByte(ptr, offP + 1, (byte)(255 - i));
                                r = Rectangle.Union(r, new Rectangle(
                                    (int)Math.Floor(pos.X * _scale.X + _offset.X) - 1,
                                    (int)Math.Floor(pos.Y * _scale.Y + _offset.Y) - 1,
                                    _rect.Width,
                                    _rect.Height));
                                p = p.Previous;
                            }

                            Invalidate(r);
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    Marshal.WriteByte(ptr, off + 1, 85);
                    _timer.Enabled = _needsApple = false;
                    //Marshal.WriteByte(ptr, end + 1, 255);
                    endPoint = apple = null;
                }

            _bitmap.UnlockBits(bits);
            _isPainting = false;

            _rect.X = (int)Math.Floor(_pos.X * _scale.X + _offset.X) - 1;
            _rect.Y = (int)Math.Floor(_pos.Y * _scale.Y + _offset.Y) - 1;
            Invalidate(_rect);

            if (endPoint.HasValue)
            {
                _rect.X = (int)Math.Floor(endPoint.Value.X * _scale.X + _offset.X) - 1;
                _rect.Y = (int)Math.Floor(endPoint.Value.Y * _scale.Y + _offset.Y) - 1;
                Invalidate(_rect);
            }

            if (apple.HasValue)
            {
                _rect.X = (int)Math.Floor(apple.Value.X * _scale.X + _offset.X) - 1;
                _rect.Y = (int)Math.Floor(apple.Value.Y * _scale.Y + _offset.Y) - 1;
                Invalidate(_rect);
            }

            ResumeLayout(false);
            Update();
        }
    }
}