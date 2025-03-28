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
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows.Forms;

namespace KeePassRDP.ResourcesWriter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = args.Length > 0 ? args[0].Trim('"') : string.Empty;

            if (string.IsNullOrWhiteSpace(path))
                path = Environment.CurrentDirectory;

            using (var imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = SystemInformation.SmallIconSize,
                TransparentColor = Color.Transparent
            })
            {
                foreach (var fi in new DirectoryInfo(Path.Combine(path, "Resources")).EnumerateFiles("*.png"))
                {
                    if (!fi.Exists)
                        continue;
                    imageList.Images.Add(
                        Path.GetFileNameWithoutExtension(fi.Name),
                        Image.FromFile(fi.FullName));
                }

                if (imageList.Images.Keys.Count > 0)
                    Console.WriteLine("KeePassRDPResources -> " + string.Join(", ", imageList.Images.Keys.Cast<string>()));

                using (var writer = new ResXResourceWriter(Path.Combine(path, "Resources.resx"), type =>
                {
                    return type.ToString();
                }))
                {
                    writer.AddResource(new ResXDataNode("imageList1.ImageStream", imageList.ImageStream, type =>
                    {
                        return type.ToString();
                    }));
                    writer.AddResource(new ResXDataNode("imageList1.ImageKeys", imageList.Images.Keys.Cast<string>().ToArray(), type =>
                    {
                        return type.ToString();
                    }));
                    writer.Generate();
                }
            }
        }
    }
}
