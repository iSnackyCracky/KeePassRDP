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

using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace KeePassRDP
{
    [ToolboxBitmap(typeof(ToolStripDropDown))]
    public class KprToolStripDropDownButton : ToolStripDropDownButton
    {
        /*public bool ClickThrough { get; set; }

        private const uint MA_ACTIVATE = 1;
        private const uint MA_ACTIVATEANDEAT = 2;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (ClickThrough && m.Msg == NativeMethods.WM_MOUSEACTIVATE && m.Result == (IntPtr)MA_ACTIVATEANDEAT)
                m.Result = (IntPtr)MA_ACTIVATE;
        }*/

        protected override Point DropDownLocation
        {
            get
            {
                if (Parent == null || !HasDropDownItems)
                {
                    return Point.Empty;
                }

                return GetDropDownBounds(DropDownDirection).Location;
            }
        }

        private static readonly MethodInfo TranslatePoint = typeof(ToolStripItem).GetMethod("TranslatePoint", BindingFlags.Instance | BindingFlags.NonPublic);

        private Rectangle GetDropDownBounds(ToolStripDropDownDirection dropDownDirection)
        {
            var dropDownBounds = DropDownDirectionToDropDownBounds(dropDownDirection, new Rectangle(Point.Empty, DropDown.AutoSize ? DropDown.GetPreferredSize(Size.Empty) : DropDown.Size));
            var b = new Rectangle((Point)TranslatePoint.Invoke(this, new object[] { Point.Empty, ToolStripPointType.ToolStripItemCoords, ToolStripPointType.ScreenCoords }), Size);
            if (Rectangle.Intersect(dropDownBounds, b).Height > 1)
            {
                var flag = RightToLeft == RightToLeft.Yes;
                if (Rectangle.Intersect(dropDownBounds, b).Width > 1)
                {
                    dropDownBounds = DropDownDirectionToDropDownBounds((!flag) ? ToolStripDropDownDirection.Right : ToolStripDropDownDirection.Left, dropDownBounds);
                }

                if (Rectangle.Intersect(dropDownBounds, b).Width > 1)
                {
                    dropDownBounds = DropDownDirectionToDropDownBounds((!flag) ? ToolStripDropDownDirection.Left : ToolStripDropDownDirection.Right, dropDownBounds);
                }
            }

            return dropDownBounds;
        }

        private Rectangle DropDownDirectionToDropDownBounds(ToolStripDropDownDirection dropDownDirection, Rectangle dropDownBounds)
        {
            var empty = Point.Empty;
            switch (dropDownDirection)
            {
                case ToolStripDropDownDirection.AboveLeft:
                    empty.X = -dropDownBounds.Width + Width;
                    empty.Y = -dropDownBounds.Height + 1;
                    break;
                case ToolStripDropDownDirection.AboveRight:
                    empty.Y = -dropDownBounds.Height + 1;
                    break;
                case ToolStripDropDownDirection.BelowRight:
                    empty.Y = Height - 1;
                    break;
                case ToolStripDropDownDirection.BelowLeft:
                    empty.X = -dropDownBounds.Width + Width;
                    empty.Y = Height - 1;
                    break;
                case ToolStripDropDownDirection.Right:
                    empty.X = Width;
                    if (!IsOnDropDown)
                    {
                        empty.X--;
                    }

                    break;
                case ToolStripDropDownDirection.Left:
                    empty.X = -dropDownBounds.Width;
                    break;
            }
            var point = (Point)TranslatePoint.Invoke(this, new object[] { Point.Empty, ToolStripPointType.ToolStripItemCoords, ToolStripPointType.ScreenCoords });
            dropDownBounds.Location = new Point(point.X + empty.X, point.Y + empty.Y);
            //dropDownBounds = WindowsFormsUtils.ConstrainToScreenWorkingAreaBounds(dropDownBounds);
            return dropDownBounds;
        }

        internal enum ToolStripPointType
        {
            ToolStripCoords,
            ScreenCoords,
            ToolStripItemCoords
        }
    }
}
