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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace KeePassRDP
{
    public static class GaussianBlur
    {
        private static readonly ParallelOptions _pOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 16,
            TaskScheduler = TaskScheduler.Default
        };

        // Based on https://github.com/mdymel/superfastblur
        public static void Process(Image image, int radial = 4, CancellationToken? cancellationToken = null)
        {
            var bitmap = image as Bitmap;
            if (bitmap == null)
                return;

            _pOptions.CancellationToken = cancellationToken ?? CancellationToken.None;

            var rct = new Rectangle(Point.Empty, bitmap.Size);
            var width = rct.Width;
            var height = rct.Height;
            var wh = width * height;

            var data = new int[wh];
            var bits = bitmap.LockBits(rct, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(bits.Scan0, data, 0, data.Length);

            var alpha = new int[wh];
            var red = new int[wh];
            var green = new int[wh];
            var blue = new int[wh];

            Parallel.For(0, data.Length, _pOptions, i =>
            {
                alpha[i] = (int)((data[i] & 0xff000000) >> 24);
                red[i] = (data[i] & 0xff0000) >> 16;
                green[i] = (data[i] & 0x00ff00) >> 8;
                blue[i] = (data[i] & 0x0000ff);
            });

            int[] newAlpha = null, newRed = null, newGreen = null, newBlue = null;

            Parallel.Invoke(
                _pOptions,
                () => GaussBlur_4(ref alpha, out newAlpha, width, height, radial),
                () => GaussBlur_4(ref red, out newRed, width, height, radial),
                () => GaussBlur_4(ref green, out newGreen, width, height, radial),
                () => GaussBlur_4(ref blue, out newBlue, width, height, radial));

            Parallel.For(0, data.Length, _pOptions, i =>
            {
                var iAlpha = Math.Max(0, Math.Min(255, newAlpha[i]));
                var iRed = Math.Max(0, Math.Min(255, newRed[i]));
                var iGreen = Math.Max(0, Math.Min(255, newGreen[i]));
                var iBlue = Math.Max(0, Math.Min(255, newBlue[i]));

                data[i] = (int)((uint)(iAlpha << 24) | (uint)(iRed << 16) | (uint)(iGreen << 8) | (uint)iBlue);
            });

            newAlpha = newRed = newGreen = newBlue = null;

            Marshal.Copy(data, 0, bits.Scan0, data.Length);
            bitmap.UnlockBits(bits);

            data = null;

            GC.Collect(GC.MaxGeneration);
        }

        [MethodImpl(256)] //MethodImplOptions.AggressiveInlining
        private static void GaussBlur_4(ref int[] source, out int [] dest, int w, int h, int r)
        {
            dest = new int[w * h];

            var bxs = BoxesForGauss(r, 3);
            BoxBlur_4(ref source, ref dest, w, h, (bxs[0] - 1) / 2);
            BoxBlur_4(ref dest, ref source, w, h, (bxs[1] - 1) / 2);
            BoxBlur_4(ref source, ref dest, w, h, (bxs[2] - 1) / 2);
        }

        [MethodImpl(256)] //MethodImplOptions.AggressiveInlining
        private static int[] BoxesForGauss(int sigma, int n)
        {
            var ss = Math.Pow(sigma, 2);
            var wIdeal = Math.Sqrt((12d * ss / n) + 1d);
            var wl = (int)Math.Floor(wIdeal);
            //if (wl % 2 == 0) wl--;
            if ((wl & 1) == 0) wl--;
            var wu = wl + 2;

            var nwl = n * wl;
            var mIdeal = (12d * ss - nwl * wl - 4d * nwl - 3d * n) / (-4d * wl - 4d);
            var m = Math.Round(mIdeal);

            var sizes = new int[n];
            for (var i = 0; i < n; i++)
                sizes[i] = i < m ? wl : wu;
            return sizes;
        }

        [MethodImpl(256)] //MethodImplOptions.AggressiveInlining
        private static void BoxBlur_4(ref int[] source, ref int[] dest, int w, int h, int r)
        {
            Buffer.BlockCopy(source, 0, dest, 0, source.Length * sizeof(int));
            BoxBlurH_4(dest, source, w, h, r);
            BoxBlurT_4(source, dest, w, h, r);
        }

        [MethodImpl(256)] //MethodImplOptions.AggressiveInlining
        private static void BoxBlurH_4(int[] source, int[] dest, int w, int h, int r)
        {
            var iar = 1d / (r + r + 1);
            Parallel.For(0, h, _pOptions, i =>
            {
                var ti = i * w;
                var li = ti;
                var ri = ti + r;
                var fv = source[ti];
                var lv = source[ti + w - 1];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += source[ti + j];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri++] - fv;
                    dest[ti++] = (int)Math.Round(val * iar);
                }
                for (int j = r + 1, k = w - r; j < k; j++)
                {
                    val += source[ri++] - source[li++];
                    dest[ti++] = (int)Math.Round(val * iar);
                }
                for (var j = w - r; j < w; j++)
                {
                    val += lv - source[li++];
                    dest[ti++] = (int)Math.Round(val * iar);
                }
            });
        }

        [MethodImpl(256)] //MethodImplOptions.AggressiveInlining
        private static void BoxBlurT_4(int[] source, int[] dest, int w, int h, int r)
        {
            var iar = (double)1 / (r + r + 1);
            Parallel.For(0, w, _pOptions, i =>
            {
                var ti = i;
                var li = ti;
                var ri = ti + r * w;
                var fv = source[ti];
                var lv = source[ti + w * (h - 1)];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++)
                    val += source[ti + j * w];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri] - fv;
                    dest[ti] = (int)Math.Round(val * iar);
                    ri += w;
                    ti += w;
                }
                for (int j = r + 1, k = h - r; j < k; j++)
                {
                    val += source[ri] - source[li];
                    dest[ti] = (int)Math.Round(val * iar);
                    li += w;
                    ri += w;
                    ti += w;
                }
                for (var j = h - r; j < h; j++)
                {
                    val += lv - source[li];
                    dest[ti] = (int)Math.Round(val * iar);
                    li += w;
                    ti += w;
                }
            });
        }
    }
}