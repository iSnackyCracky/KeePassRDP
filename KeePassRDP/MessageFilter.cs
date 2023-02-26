/*
 *  Copyright (C) 2018 - 2023 iSnackyCracky, NETertainer
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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeePassRDP
{
    internal static class MessageFilter
    {
        public class FormClickHandler : IMessageFilter
        {
            private static FormClickHandler Instance;

            public static void Enable(bool enabled, Form form = null)
            {
                if (enabled)
                {
                    if (Instance == null)
                    {
                        Instance = new FormClickHandler(form);
                        Application.AddMessageFilter(Instance);
                    }
                }
                else
                {
                    if (Instance != null)
                    {
                        Application.RemoveMessageFilter(Instance);
                        Instance = null;
                    }
                }
            }

            private const int WM_LBUTTONDOWN = 0x0201;

            private readonly WeakReference _form;

            private FormClickHandler(Form form)
            {
                _form = new WeakReference(form ?? Form.ActiveForm);
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_LBUTTONDOWN && _form.IsAlive && _form.Target is Form)
                {
                    var control = Control.FromHandle(m.HWnd);
                    var form = _form.Target as Form;
                    if (form.ActiveControl != control)
                        form.ActiveControl = null;
                }

                return false;
            }
        }

        public class ListBoxMouseWheelHandler : IMessageFilter
        {
            private static ListBoxMouseWheelHandler Instance;

            public static void Enable(bool enabled)
            {
                if (enabled)
                {
                    if (Instance == null)
                    {
                        Instance = new ListBoxMouseWheelHandler();
                        Application.AddMessageFilter(Instance);
                    }
                }
                else
                {
                    if (Instance != null)
                    {
                        Application.RemoveMessageFilter(Instance);
                        Instance = null;
                    }
                }
            }

            private const int WM_MOUSEMOVE = 0x200;
            private const int WM_MOUSEWHEEL = 0x20A;
            //private const int WM_HSCROLL = 0x114;
            private const int WM_VSCROLL = 0x115;
            private const int SB_THUMBPOSITION = 4;
            private const uint SB_THUMBTRACK = 5;

            private IntPtr activeHwnd;
            private WeakReference activeControl;

            public bool PreFilterMessage(ref Message m)
            {
                switch (m.Msg)
                {
                    case WM_MOUSEMOVE:
                        if (activeHwnd != m.HWnd)
                        {
                            activeHwnd = m.HWnd;
                            var control = Control.FromHandle(m.HWnd);
                            if (control is ListBox)
                                activeControl = new WeakReference(control);
                            else
                                activeControl = null;
                        }
                        break;
                    case WM_MOUSEWHEEL:
                        if (activeControl != null && activeControl.IsAlive && activeControl.Target is ListBox)
                        {
                            var delta = (short)(ushort)(((uint)(ulong)m.WParam) >> 16);
                            HandleDelta(activeControl.Target as ListBox, delta);
                            return true;
                        }
                        break;
                    case WM_VSCROLL:
                        if (activeControl != null && activeControl.IsAlive && activeControl.Target is ListBox)
                        {
                            if (((uint)m.WParam & 0xFF) == SB_THUMBTRACK)
                            {
                                var delta = (short)(ushort)(((uint)(ulong)m.WParam) >> 16);
                                HandleDelta(activeControl.Target as ListBox, delta);
                                // Change SB_THUMBTRACK to SB_THUMBPOSITION.
                                m.WParam = (IntPtr)(((int)m.WParam & ~0xFFFF) | SB_THUMBPOSITION);
                                return true;
                            }
                        }
                        break;
                }

                return false;
            }

            /*[DllImport("User32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

            [DllImport("User32.dll")]
            private static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref SCROLLINFO lpsi, bool fRedraw);

            [DllImport("User32.Dll", EntryPoint = "PostMessageW")]
            private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            [DllImport("User32.dll", EntryPoint = "SendMessageW")]
            private static extern int SendMessage(IntPtr hWnd, int wMsg, int itemIndex, int itemHeight);

            //private const int LB_SETITEMHEIGHT = 0x01A0;
            private const int LB_SETTOPINDEX = 0x0197;

            [Flags]
            private enum ScrollInfoMask : uint
            {
                SIF_RANGE = 0x1,
                SIF_PAGE = 0x2,
                SIF_POS = 0x4,
                SIF_DISABLENOSCROLL = 0x8,
                SIF_TRACKPOS = 0x10,
                SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS,
            }

            private enum SBOrientation : int
            {
                SB_HORZ = 0x0,
                SB_VERT = 0x1,
                SB_CTL = 0x2,
                SB_BOTH = 0x3
            }

            [Serializable, StructLayout(LayoutKind.Sequential)]
            private struct SCROLLINFO
            {
                public int cbSize; // (uint) int is because of Marshal.SizeOf
                public uint fMask;
                public int nMin;
                public int nMax;
                public uint nPage;
                public int nPos;
                public int nTrackPos;
            }

            //private const int WM_HSCROLL = 0x114;*/

            private static void HandleDelta(ListBox listBox, int delta)
            {
                /*var si = new SCROLLINFO();
                si.cbSize = Marshal.SizeOf(si);
                si.fMask = (int)(ScrollInfoMask.SIF_PAGE | ScrollInfoMask.SIF_POS);

                GetScrollInfo(listBox.Handle, (int)SBOrientation.SB_VERT, ref si);

                si.nPage = 1;

                var oPos = si.nPos;
                si.nPos += delta > 0 ? -1 : 1;
                si.nPos = Math.Min(listBox.Items.Count - 1, Math.Max(0, si.nPos));

                var nPos = si.nPos;*/

                if (delta == 0)
                    return;

                var oPos = listBox.TopIndex;
                var nPos = Math.Min(listBox.Items.Count - 1, Math.Max(0, delta > 0 ? oPos - 1 : oPos + 1));

                if (oPos != nPos)
                {
                    //SetScrollInfo(listBox.Handle, (int)SBOrientation.SB_VERT, ref si, true);
                    //SendMessage(listBox.Handle, LB_SETTOPINDEX, si.nPos, 0);
                    listBox.TopIndex = nPos;
                    //PostMessage(listBox.Handle, WM_VSCROLL, new IntPtr(SB_THUMBPOSITION | (uint)(si.nPos << 16)), IntPtr.Zero);
                }
            }
        }

        public class ToolStripDropDownMouseWheelHandler : IMessageFilter
        {
            private static ToolStripDropDownMouseWheelHandler Instance;

            public static void Enable(bool enabled)
            {
                if (enabled)
                {
                    if (Instance == null)
                    {
                        Instance = new ToolStripDropDownMouseWheelHandler();
                        Application.AddMessageFilter(Instance);
                    }
                }
                else
                {
                    if (Instance != null)
                    {
                        Application.RemoveMessageFilter(Instance);
                        Instance = null;
                    }
                }
            }

            private const int WM_MOUSEMOVE = 0x200;
            private const int WM_MOUSEWHEEL = 0x20A;

            private IntPtr activeHwnd;
            private WeakReference activeControl;

            public bool PreFilterMessage(ref Message m)
            {
                switch (m.Msg)
                {
                    case WM_MOUSEMOVE:
                        if (activeHwnd != m.HWnd)
                        {
                            activeHwnd = m.HWnd;
                            var control = Control.FromHandle(m.HWnd);
                            if (control is ToolStripDropDown)
                                activeControl = new WeakReference(control);
                            else
                                activeControl = null;
                        }
                        break;
                    case WM_MOUSEWHEEL:
                        if (activeControl != null && activeControl.IsAlive && activeControl.Target is ToolStripDropDown)
                        {
                            var delta = (short)(ushort)(((uint)(ulong)m.WParam) >> 16);
                            HandleDelta(activeControl.Target as ToolStripDropDown, delta);
                            return true;
                        }
                        break;
                }

                return false;
            }

            private static readonly Action<ToolStrip, int> ScrollInternal = (Action<ToolStrip, int>)Delegate.CreateDelegate(
                typeof(Action<ToolStrip, int>),
                typeof(ToolStrip).GetMethod("ScrollInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));

            private static void HandleDelta(ToolStripDropDown toolStripDropDown, int delta)
            {
                var count = toolStripDropDown.Items.Count;
                if (count < 1)
                    return;

                var top = toolStripDropDown.Items[0].Bounds.Top;
                var bottom = toolStripDropDown.Items[count - 1].Bounds.Bottom;
                var height = toolStripDropDown.Height;

                if (bottom < height && top > 0)
                    return;

                delta /= -4;

                if (delta < 0)
                {
                    if (top - delta > 9)
                        delta = top - 9;
                }
                else if (delta > 0)
                {
                    if (delta > bottom - height + 9)
                        delta = bottom - height + 9;
                }

                if (delta != 0)
                    ScrollInternal(toolStripDropDown, delta);
            }
        }

        public class ListViewGroupHeaderClickHandler : IMessageFilter
        {
            private static ListViewGroupHeaderClickHandler Instance;

            public static void Enable(bool enabled)
            {
                if (enabled)
                {
                    if (Instance == null)
                    {
                        Instance = new ListViewGroupHeaderClickHandler();
                        Application.AddMessageFilter(Instance);
                    }
                }
                else
                {
                    if (Instance != null)
                    {
                        Application.RemoveMessageFilter(Instance);
                        Instance = null;
                    }
                }
            }

            [DllImport("User32.dll", EntryPoint = "SendMessageW", SetLastError = false)]
            private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref LVHITTESTINFO lParam);

            private const int WM_LBUTTONDOWN = 0x0201;
            private const int LVM_FIRST = 0x1000;
            private const int LVM_SUBITEMHITTEST = LVM_FIRST + 57;

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_LBUTTONDOWN)
                {
                    var info = new LVHITTESTINFO
                    {
                        pt = LParamToPOINT(m.LParam)
                    };

                    // If the click is on the group header, filter message.
                    if (SendMessage(m.HWnd, LVM_SUBITEMHITTEST, -1, ref info) != -1)
                        if (info.flags.HasFlag(LVHITTESTFLAGS.LVHT_EX_GROUP_HEADER))
                            return true;
                }

                return false;
            }

            private static POINT LParamToPOINT(IntPtr lparam)
            {
                return new POINT
                {
                    x = lparam.ToInt32() & 0xFFFF,
                    y = lparam.ToInt32() >> 16
                };
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct POINT
            {
                public int x;
                public int y;
            }

            [Flags]
            private enum LVHITTESTFLAGS : uint
            {
                LVHT_NOWHERE = 0x00000001,
                LVHT_ONITEMICON = 0x00000002,
                LVHT_ONITEMLABEL = 0x00000004,
                LVHT_ONITEMSTATEICON = 0x00000008,
                LVHT_ONITEM = LVHT_ONITEMICON | LVHT_ONITEMLABEL | LVHT_ONITEMSTATEICON,
                LVHT_ABOVE = 0x00000008,
                LVHT_BELOW = 0x00000010,
                LVHT_TORIGHT = 0x00000020,
                LVHT_TOLEFT = 0x00000040,
                LVHT_EX_GROUP_HEADER = 0x10000000,
                LVHT_EX_GROUP_FOOTER = 0x20000000,
                LVHT_EX_GROUP_COLLAPSE = 0x40000000,
                LVHT_EX_GROUP_BACKGROUND = 0x80000000,
                LVHT_EX_GROUP_STATEICON = 0x01000000,
                LVHT_EX_GROUP_SUBSETLINK = 0x02000000,
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct LVHITTESTINFO
            {
                public POINT pt;
                public LVHITTESTFLAGS flags;
                public int iItem;
                public int iSubItem;
                public int iGroup;
            }
        }
    }
}