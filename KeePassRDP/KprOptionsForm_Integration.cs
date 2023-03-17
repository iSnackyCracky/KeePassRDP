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

using KeePass.UI;
using KeePassRDP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeePassRDP
{
    public partial class KprOptionsForm
    {
        private void numCredVaultTtl_ValueChanged(object sender, EventArgs e)
        {
            chkCredVaultAdaptiveTtl.Enabled = numCredVaultTtl.Value > 0;
        }

        private void tblKeyboardSettings_SizeChanged(object sender, EventArgs e)
        {
            if (!_tabIntegrationInitialized || tabIntegration.UseWaitCursor)
                return;

            var columnCount = tblKeyboardSettings.ColumnCount;
            if (columnCount < 1)
                return;

            var widths = tblKeyboardSettings.GetColumnWidths();
            if (widths.Length < 2)
                return;

            var autosize = widths[1] + widths[1] - 4;
            var width = tblKeyboardSettings.Width - autosize;
            var w1 = DpiUtil.ScaleIntX(300);
            var w2 = DpiUtil.ScaleIntX(350);
            var large = width >= w1 + w2;
            var changed = false;

            tblKeyboardSettings.SuspendLayout();

            if (columnCount > 0)
            {
                var style = tblKeyboardSettings.ColumnStyles[0];
                if (style.SizeType != SizeType.Absolute && (large || (int)(width * 0.55) >= w1))
                {
                    tblKeyboardSettings.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, w1);
                    changed = true;
                }
                else if (style.SizeType == SizeType.Absolute && !(large || (int)(width * 0.55) >= w1))
                {
                    tblKeyboardSettings.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 55);
                    changed = true;
                }
            }

            if (columnCount > 1)
            {
                var style = tblKeyboardSettings.ColumnStyles[2];
                if (style.SizeType != SizeType.Absolute && (large || (int)(width * 0.45) >= w2))
                {
                    tblKeyboardSettings.ColumnStyles[2] = new ColumnStyle(SizeType.Absolute, w2);
                    changed = true;
                }
                else if (style.SizeType == SizeType.Absolute && !(large || (int)(width * 0.45) >= w2))
                {
                    tblKeyboardSettings.ColumnStyles[2] = new ColumnStyle(SizeType.Percent, 45);
                    changed = true;
                }
            }

            tblKeyboardSettings.ResumeLayout(changed);
        }

        private void cmdOpenRdpKeyReset_Click(object sender, EventArgs e)
        {
            txtOpenRdpKey.Hotkey = KprMenu.DefaultOpenRdpConnectionShortcut;
            ResetActiveControl(sender as Control);
        }

        private void cmdOpenRdpAdminKeyReset_Click(object sender, EventArgs e)
        {
            txtOpenRdpAdminKey.Hotkey = KprMenu.DefaultOpenRdpConnectionAdminShortcut;
            ResetActiveControl(sender as Control);
        }

        private void cmdOpenRdpNoCredKeyReset_Click(object sender, EventArgs e)
        {
            txtOpenRdpNoCredKey.Hotkey = Keys.None;
            ResetActiveControl(sender as Control);
        }

        private void cmdOpenRdpNoCredAdminKeyReset_Click(object sender, EventArgs e)
        {
            txtOpenRdpNoCredAdminKey.Hotkey = Keys.None;
            ResetActiveControl(sender as Control);
        }

        private void lstKeePassContextMenuItemsAvailable_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmdKeePassContextMenuItemsAdd.Enabled = lstKeePassContextMenuItemsAvailable.SelectedItems.Count > 0;
        }

        private void lstKeePassContextMenuItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmdKeePassContextMenuItemsRemove.Enabled = lstKeePassContextMenuItems.SelectedItems.Count > 0;
        }

        private void lstKeePassToolbarItemsAvailable_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmdKeePassToolbarItemsAdd.Enabled = lstKeePassToolbarItemsAvailable.SelectedItems.Count > 0;
        }

        private void lstKeePassToolbarItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmdKeePassToolbarItemsRemove.Enabled = lstKeePassToolbarItems.SelectedItems.Count > 0;
        }

        private void cmdKeePassContextMenuItemsAdd_Click(object sender, EventArgs e)
        {
            var list1 = lstKeePassContextMenuItemsAvailable.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;
            var list2 = lstKeePassContextMenuItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;

            var selectedIndices = lstKeePassContextMenuItemsAvailable.SelectedIndices.Cast<int>().ToList();

            lstKeePassContextMenuItems.BeginUpdate();
            foreach (var i in selectedIndices.OrderBy(x => x))
            {
                var item = list1[i];
                var newIndex = list2.IndexOf(list2.FirstOrDefault(x => x.Key > item.Key));
                if (newIndex < 0)
                    newIndex = list2.Count;
                list2.Insert(newIndex, item);
            }
            lstKeePassContextMenuItems.EndUpdate();

            lstKeePassContextMenuItemsAvailable.BeginUpdate();
            foreach (var i in selectedIndices.OrderByDescending(x => x))
                list1.RemoveAt(i);
            lstKeePassContextMenuItemsAvailable.EndUpdate();

            ResetActiveControl(sender as Control);
        }

        private void cmdKeePassContextMenuItemsRemove_Click(object sender, EventArgs e)
        {
            var list1 = lstKeePassContextMenuItemsAvailable.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;
            var list2 = lstKeePassContextMenuItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;

            var selectedIndices = lstKeePassContextMenuItems.SelectedIndices.Cast<int>().ToList();

            lstKeePassContextMenuItemsAvailable.BeginUpdate();
            foreach (var i in selectedIndices.OrderBy(x => x))
            {
                var item = list2[i];
                var newIndex = list1.IndexOf(list1.FirstOrDefault(x => x.Key > item.Key));
                if (newIndex < 0)
                    newIndex = list1.Count;
                list1.Insert(newIndex, item);
            }
            lstKeePassContextMenuItemsAvailable.EndUpdate();

            lstKeePassContextMenuItems.BeginUpdate();
            foreach (var i in selectedIndices.OrderByDescending(x => x))
                list2.RemoveAt(i);
            lstKeePassContextMenuItems.EndUpdate();

            ResetActiveControl(sender as Control);
        }

        private void cmdKeePassContextMenuItemsReset_Click(object sender, EventArgs e)
        {
            var list1 = lstKeePassContextMenuItemsAvailable.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;
            var list2 = lstKeePassContextMenuItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;

            var menuItems = Enum.GetValues(typeof(KprMenu.MenuItem)).Cast<KprMenu.MenuItem>();

            lstKeePassContextMenuItems.BeginUpdate();
            list2.Clear();
            foreach (var menu in menuItems.Where(menu => menu > KprMenu.MenuItem.Empty && menu <= KprMenu.MaxMenuItem && KprMenu.DefaultContextMenuItems.HasFlag(menu)))
                list2.Add(new KeyValuePair<KprMenu.MenuItem, string>(menu, KprMenu.GetText(menu)));
            lstKeePassContextMenuItems.EndUpdate();

            lstKeePassContextMenuItemsAvailable.BeginUpdate();
            list1.Clear();
            lstKeePassContextMenuItemsAvailable.EndUpdate();

            ResetActiveControl(sender as Control);
        }

        private void cmdKeePassToolbarItemsAdd_Click(object sender, EventArgs e)
        {
            var list1 = lstKeePassToolbarItemsAvailable.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;
            var list2 = lstKeePassToolbarItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;

            var selectedIndices = lstKeePassToolbarItemsAvailable.SelectedIndices.Cast<int>().ToList();

            lstKeePassToolbarItems.BeginUpdate();
            foreach (var i in selectedIndices.OrderBy(x => x))
            {
                var item = list1[i];
                var newIndex = list2.IndexOf(list2.FirstOrDefault(x => x.Key > item.Key));
                if (newIndex < 0)
                    newIndex = list2.Count;
                list2.Insert(newIndex, item);
            }
            lstKeePassToolbarItems.EndUpdate();

            lstKeePassToolbarItemsAvailable.BeginUpdate();
            foreach (var i in selectedIndices.OrderByDescending(x => x))
                list1.RemoveAt(i);
            lstKeePassToolbarItemsAvailable.EndUpdate();

            ResetActiveControl(sender as Control);
        }

        private void cmdKeePassToolbarItemsRemove_Click(object sender, EventArgs e)
        {
            var list1 = lstKeePassToolbarItemsAvailable.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;
            var list2 = lstKeePassToolbarItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;

            var selectedIndices = lstKeePassToolbarItems.SelectedIndices.Cast<int>().ToList();

            lstKeePassToolbarItemsAvailable.BeginUpdate();
            foreach (var i in selectedIndices.OrderBy(x => x))
            {
                var item = list2[i];
                var newIndex = list1.IndexOf(list1.FirstOrDefault(x => x.Key > item.Key));
                if (newIndex < 0)
                    newIndex = list1.Count;
                list1.Insert(newIndex, item);
            }
            lstKeePassToolbarItemsAvailable.EndUpdate();

            lstKeePassToolbarItems.BeginUpdate();
            foreach (var i in selectedIndices.OrderByDescending(x => x))
                list2.RemoveAt(i);
            lstKeePassToolbarItems.EndUpdate();

            ResetActiveControl(sender as Control);
        }

        private void cmdKeePassToolbarItemsReset_Click(object sender, EventArgs e)
        {
            var list1 = lstKeePassToolbarItemsAvailable.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;
            var list2 = lstKeePassToolbarItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;

            var menuItems = Enum.GetValues(typeof(KprMenu.MenuItem)).Cast<KprMenu.MenuItem>();

            lstKeePassToolbarItems.BeginUpdate();
            list2.Clear();
            foreach (var menu in menuItems.Where(menu => menu > KprMenu.MenuItem.Empty && menu <= KprMenu.MaxMenuItem && KprMenu.DefaultToolbarItems.HasFlag(menu)))
                list2.Add(new KeyValuePair<KprMenu.MenuItem, string>(menu, KprMenu.GetText(menu)));
            lstKeePassToolbarItems.EndUpdate();

            lstKeePassToolbarItemsAvailable.BeginUpdate();
            list1.Clear();
            foreach (var menu in menuItems.Where(menu => menu > KprMenu.MenuItem.Empty && KprMenu.DefaultContextMenuItems.HasFlag(menu) && !KprMenu.DefaultToolbarItems.HasFlag(menu)))
                list1.Add(new KeyValuePair<KprMenu.MenuItem, string>(menu, KprMenu.GetText(menu)));
            lstKeePassToolbarItemsAvailable.EndUpdate();

            ResetActiveControl(sender as Control);
        }

        [DllImport("User32.dll", EntryPoint = "SendMessageW", SetLastError = false)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, int itemIndex, int itemHeight);

        private const int LB_SETITEMHEIGHT = 0x01A0;

        private void lst_SizeChanged(object sender, EventArgs e)
        {
            if (!_tabIntegrationInitialized || tabIntegration.UseWaitCursor)
                return;

            var lst = sender as ListBox;

            var cnt = lst.Items.Count;
            if (cnt > 0)
            {
                lst.SuspendLayout();
                using (var graphics = lst.CreateGraphics())
                    for (var i = 0; i < cnt; i++)
                    {
                        var args = new MeasureItemEventArgs(graphics, i);
                        lst_MeasureItem(lst, args);
                        SendMessage(lst.Handle, LB_SETITEMHEIGHT, i, args.ItemHeight);
                        lst.Invalidate(lst.GetItemRectangle(i));
                    }
                lst.ResumeLayout(false);
            }
        }

        private void lst_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (!_tabIntegrationInitialized || tabIntegration.UseWaitCursor)
                return;

            var lst = sender as ListBox;

            if (e.Index >= 0)
                e.ItemHeight = Math.Max(
                    lst.ItemHeight,
                    TextRenderer.MeasureText(
                        e.Graphics,
                        ((KeyValuePair<KprMenu.MenuItem, string>)lst.Items[e.Index]).Value,
                        lst.Font,
                        new Size(lst.Width - (ScrollbarUtil.GetVisibleScrollbars(lst).HasFlag(ScrollBars.Vertical) ? UIUtil.GetVScrollBarWidth() : 0) - 6 - 3, 0),
                        TextFormatFlags.VerticalCenter |
                            TextFormatFlags.WordBreak |
                            TextFormatFlags.NoPadding |
                            TextFormatFlags.TextBoxControl |
                            TextFormatFlags.PreserveGraphicsClipping |
                            TextFormatFlags.PreserveGraphicsTranslateTransform).Height + 4);
            else
                e.ItemHeight = lst.ItemHeight;
        }

        private void lst_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (!_tabIntegrationInitialized || tabIntegration.UseWaitCursor)
                return;

            if (e.Index >= 0)
            {
                var lst = sender as ListBox;

                e.DrawBackground();

                TextRenderer.DrawText(
                    e.Graphics,
                    ((KeyValuePair<KprMenu.MenuItem, string>)lst.Items[e.Index]).Value,
                    e.Font,
                    new Rectangle(new Point(e.Bounds.X + 3, e.Bounds.Y), new Size(lst.Width - (ScrollbarUtil.GetVisibleScrollbars(lst).HasFlag(ScrollBars.Vertical) ? UIUtil.GetVScrollBarWidth() : 0) - 6 - 3, e.Bounds.Height)),
                    e.ForeColor,
                    e.BackColor,
                    TextFormatFlags.VerticalCenter |
                        TextFormatFlags.WordBreak |
                        TextFormatFlags.Default |
                        TextFormatFlags.NoPadding |
                        TextFormatFlags.TextBoxControl |
                        TextFormatFlags.PreserveGraphicsClipping |
                        TextFormatFlags.PreserveGraphicsTranslateTransform);

                e.DrawFocusRectangle();
            }
        }
    }
}