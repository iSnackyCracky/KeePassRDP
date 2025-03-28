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

using KeePass.UI;
using KeePassRDP.Extensions;
using KeePassRDP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP
{
    public partial class KprSettingsControl : UserControl
    {
        public delegate void MenuItemCheckedChanged(KprMenu.MenuItem menuItem, bool isChecked);
        public delegate void MenuItemHotkeyChanged(KprMenu.MenuItem menuItem, Hotkey hotkey);

        public event MenuItemCheckedChanged MenuItemContextMenu_CheckedChanged
        {
            add { tblKeyboardSettings.ContextMenu_CheckedChanged += value; }
            remove { tblKeyboardSettings.ContextMenu_CheckedChanged -= value; }
        }

        public event MenuItemCheckedChanged MenuItemToolbar_CheckedChanged
        {
            add { tblKeyboardSettings.Toolbar_CheckedChanged += value; }
            remove { tblKeyboardSettings.Toolbar_CheckedChanged -= value; }
        }

        public event MenuItemHotkeyChanged MenuItemHotkey_Changed
        {
            add { tblKeyboardSettings.Hotkey_Changed += value; }
            remove { tblKeyboardSettings.Hotkey_Changed -= value; }
        }

        internal ManualResetEventSlim ControlsCreated { get { return tblKeyboardSettings.ControlsCreated; } }

        public MenuItemSettings this[KprMenu.MenuItem menuItem]
        {
            get { return tblKeyboardSettings[menuItem]; }
            set { tblKeyboardSettings[menuItem] = value; }
        }

        public KprSettingsControl()
        {
            InitializeComponent();

            SuspendLayout();

            Util.EnableDoubleBuffered(
                tblVisibilitySettings,
                tblKeePassContextMenuItems,
                lstKeePassContextMenuItems,
                lstKeePassContextMenuItemsAvailable,
                tblKeePassToolbarItems,
                lstKeePassToolbarItems,
                lstKeePassToolbarItemsAvailable
            );

            KprResourceManager.Instance.TranslateMany(
                lblKeyboardSettings,
                lblKeyboardShortcut,
                lblVisibilitySettings,
                lblKeePassContextMenuItems,
                lblKeePassToolbarItems
            );

            ResumeLayout(false);

            lstKeePassContextMenuItems.Tag = cmdKeePassContextMenuItemsRemove;
            lstKeePassContextMenuItemsAvailable.Tag = cmdKeePassContextMenuItemsAdd;
            lstKeePassToolbarItems.Tag = cmdKeePassToolbarItemsRemove;
            lstKeePassToolbarItemsAvailable.Tag = cmdKeePassToolbarItemsAdd;

            MenuItemContextMenu_CheckedChanged += ContextMenu_CheckedChanged;
            MenuItemToolbar_CheckedChanged += Toolbar_CheckedChanged;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!DesignMode)
                Task.Factory.StartNew(() =>
                {
                    var keePassContextMenuItems = new BindingList<KeyValuePair<KprMenu.MenuItem, string>>();
                    var keePassToolbarItems = new BindingList<KeyValuePair<KprMenu.MenuItem, string>>();
                    var keePassContextMenuItemsAvailable = new BindingList<KeyValuePair<KprMenu.MenuItem, string>>();
                    var keePassToolbarItemsAvailable = new BindingList<KeyValuePair<KprMenu.MenuItem, string>>();

                    foreach (var menuItem in KprMenu.MenuItemValues)
                    {
                        var menuItemSettings = this[menuItem];
                        var text = menuItem.GetText();
                        var kvp = new KeyValuePair<KprMenu.MenuItem, string>(menuItem, text);

                        if (menuItemSettings.ContextMenuChecked)
                            keePassContextMenuItems.Add(kvp);
                        else
                            keePassContextMenuItemsAvailable.Add(kvp);

                        if (menuItemSettings.ToolbarChecked)
                            keePassToolbarItems.Add(kvp);
                        else
                            keePassToolbarItemsAvailable.Add(kvp);
                    }

                    Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                    {
                        BeginUpdate();
                        lstKeePassContextMenuItems.DataSource = keePassContextMenuItems;
                        lstKeePassToolbarItems.DataSource = keePassToolbarItems;
                        lstKeePassContextMenuItemsAvailable.DataSource = keePassContextMenuItemsAvailable;
                        lstKeePassToolbarItemsAvailable.DataSource = keePassToolbarItemsAvailable;
                        EndUpdate();
                    })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                }, CancellationToken.None, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (UseWaitCursor || Tag as bool? == true)
                return;

            var columnCount = tblKeyboardSettings.ColumnCount;
            if (columnCount < 1)
                return;

            var widths = tblKeyboardSettings.GetColumnWidths();
            if (widths.Length < 5)
                return;

            Tag = true;

            var autosize = widths[1] + widths[2] + widths[4] + 6;
            var width = tblKeyboardSettings.Width - autosize;
            var w1 = DpiUtil.ScaleIntX((int)(700 * 0.57));
            var w2 = DpiUtil.ScaleIntX((int)(700 * 0.43));
            var large = width >= w1 + w2;

            var layout = false;
            SuspendLayout();
            if (large)
            {
                if (tblKeyboardSettings.MaximumSize.Width != tblKeyboardSettings.Width)
                {
                    tblKeyboardSettings.MaximumSize = new Size(tblKeyboardSettings.Width, 0);
                    layout = true;
                }
                if (tblVisibilitySettings.MaximumSize.Width != tblVisibilitySettings.Width)
                {
                    tblVisibilitySettings.MaximumSize = new Size(tblVisibilitySettings.Width, 0);
                    layout = true;
                }
            }
            else
            {
                if (tblKeyboardSettings.MaximumSize.Width != 0)
                {
                    tblKeyboardSettings.MaximumSize = Size.Empty;
                    layout = true;
                }
                if (tblVisibilitySettings.MaximumSize.Width != 0)
                {
                    tblVisibilitySettings.MaximumSize = Size.Empty;
                    layout = true;
                }
            }
            ResumeLayout(layout);

            Tag = null;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            var value = Visible;
            tblKeyboardSettings.Visible = value;
            tblVisibilitySettings.Visible = value;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            ttGeneric.Site = Parent != null ? Parent.Site : Site;
        }

        public void BeginUpdate()
        {
            lstKeePassContextMenuItems.BeginUpdate();
            lstKeePassContextMenuItemsAvailable.BeginUpdate();
            lstKeePassToolbarItems.BeginUpdate();
            lstKeePassToolbarItemsAvailable.BeginUpdate();
        }

        public void EndUpdate()
        {
            lstKeePassContextMenuItems.EndUpdate();
            lstKeePassContextMenuItemsAvailable.EndUpdate();
            lstKeePassToolbarItems.EndUpdate();
            lstKeePassToolbarItemsAvailable.EndUpdate();
        }

        new public void SuspendLayout()
        {
            base.SuspendLayout();
            tblKeyboardSettings.SuspendLayout();
            tblVisibilitySettings.SuspendLayout();
            tblKeePassContextMenuItems.SuspendLayout();
            tblKeePassToolbarItems.SuspendLayout();
            lstKeePassContextMenuItemsAvailable.SuspendLayout();
            lstKeePassContextMenuItems.SuspendLayout();
            lstKeePassToolbarItemsAvailable.SuspendLayout();
            lstKeePassToolbarItems.SuspendLayout();
        }

        new public void ResumeLayout(bool performLayout)
        {
            base.ResumeLayout(performLayout);
            tblKeyboardSettings.ResumeLayout(performLayout);
            tblVisibilitySettings.ResumeLayout(performLayout);
            tblKeePassContextMenuItems.ResumeLayout(performLayout);
            tblKeePassToolbarItems.ResumeLayout(performLayout);
            lstKeePassContextMenuItemsAvailable.ResumeLayout(performLayout);
            lstKeePassContextMenuItems.ResumeLayout(performLayout);
            lstKeePassToolbarItemsAvailable.ResumeLayout(performLayout);
            lstKeePassToolbarItems.ResumeLayout(performLayout);
        }

        new public void ResumeLayout()
        {
            ResumeLayout(true);
        }

        private void ResetActiveControl(Control control)
        {
            if (control == null || ActiveControl == control)
                ActiveControl = null;
        }

        private void ContextMenu_CheckedChanged(KprMenu.MenuItem menuItem, bool isChecked)
        {
            if (isChecked)
            {
                var list = lstKeePassContextMenuItemsAvailable.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;
                if (list == null)
                    return;
                var idx = list.IndexOf(list.FirstOrDefault(x => x.Key == menuItem));
                if (idx >= 0)
                {
                    lstKeePassContextMenuItemsAvailable.BeginUpdate();
                    lstKeePassContextMenuItemsAvailable.ClearSelected();
                    lstKeePassContextMenuItemsAvailable.SetSelected(idx, true);
                    lstKeePassContextMenuItemsAvailable.EndUpdate();
                    cmdKeePassContextMenuItemsAdd_Click(null, EventArgs.Empty);
                }
            }
            else
            {
                var list = lstKeePassContextMenuItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;
                if (list == null)
                    return;
                var idx = list.IndexOf(list.FirstOrDefault(x => x.Key == menuItem));
                if (idx >= 0)
                {
                    lstKeePassContextMenuItems.BeginUpdate();
                    lstKeePassContextMenuItems.ClearSelected();
                    lstKeePassContextMenuItems.SetSelected(idx, true);
                    lstKeePassContextMenuItems.EndUpdate();
                    cmdKeePassContextMenuItemsRemove_Click(null, EventArgs.Empty);
                }
            }
        }

        private void Toolbar_CheckedChanged(KprMenu.MenuItem menuItem, bool isChecked)
        {
            if (isChecked)
            {
                var list = lstKeePassToolbarItemsAvailable.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;
                if (list == null)
                    return;
                var idx = list.IndexOf(list.FirstOrDefault(x => x.Key == menuItem));
                if (idx >= 0)
                {
                    lstKeePassToolbarItemsAvailable.BeginUpdate();
                    lstKeePassToolbarItemsAvailable.ClearSelected();
                    lstKeePassToolbarItemsAvailable.SetSelected(idx, true);
                    lstKeePassToolbarItemsAvailable.EndUpdate();
                    cmdKeePassToolbarItemsAdd_Click(null, EventArgs.Empty);
                }
            }
            else
            {
                var list = lstKeePassToolbarItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;
                if (list == null)
                    return;
                var idx = list.IndexOf(list.FirstOrDefault(x => x.Key == menuItem));
                if (idx >= 0)
                {
                    lstKeePassToolbarItems.BeginUpdate();
                    lstKeePassToolbarItems.ClearSelected();
                    lstKeePassToolbarItems.SetSelected(idx, true);
                    lstKeePassToolbarItems.EndUpdate();
                    cmdKeePassToolbarItemsRemove_Click(null, EventArgs.Empty);
                }
            }
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
            var flps = tblKeyboardSettings.Controls.OfType<TableLayoutPanel>();

            lstKeePassContextMenuItems.BeginUpdate();
            foreach (var i in selectedIndices.OrderBy(x => x))
            {
                var item = list1[i];
                var newIndex = list2.IndexOf(list2.FirstOrDefault(x => x.Key > item.Key));
                if (newIndex < 0)
                    newIndex = list2.Count;
                list2.Insert(newIndex, item);
                var midx = KprMenu.MenuItemValues.IndexOf(item.Key);
                var flp = flps.FirstOrDefault(x => tblKeyboardSettings.GetRow(x) - 1 == midx);
                if (flp != null)
                {
                    var chk = flp.Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 0);
                    if (!chk.Checked)
                    {
                        chk.Tag = true;
                        chk.Checked = true;
                    }
                }
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
            var flps = tblKeyboardSettings.Controls.OfType<TableLayoutPanel>();

            lstKeePassContextMenuItemsAvailable.BeginUpdate();
            foreach (var i in selectedIndices.OrderBy(x => x))
            {
                var item = list2[i];
                var newIndex = list1.IndexOf(list1.FirstOrDefault(x => x.Key > item.Key));
                if (newIndex < 0)
                    newIndex = list1.Count;
                list1.Insert(newIndex, item);
                var midx = KprMenu.MenuItemValues.IndexOf(item.Key);
                var flp = flps.FirstOrDefault(x => tblKeyboardSettings.GetRow(x) - 1 == midx);
                if (flp != null)
                {
                    var chk = flp.Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 0);
                    if (chk.Checked)
                    {
                        chk.Tag = true;
                        chk.Checked = false;
                    }
                }
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

            var flps = tblKeyboardSettings.Controls.OfType<TableLayoutPanel>();

            lstKeePassContextMenuItems.BeginUpdate();
            list2.Clear();
            foreach (var menu in KprMenu.MenuItemValues.Where(menu => KprMenu.DefaultContextMenuItems.HasFlag(menu)))
            {
                list2.Add(new KeyValuePair<KprMenu.MenuItem, string>(menu, menu.GetText()));
                var midx = KprMenu.MenuItemValues.IndexOf(menu);
                var flp = flps.FirstOrDefault(x => tblKeyboardSettings.GetRow(x) - 1 == midx);
                if (flp != null)
                {
                    var chk = flp.Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 0);
                    if (!chk.Checked)
                    {
                        chk.Tag = true;
                        chk.Checked = true;
                    }
                }
            }
            lstKeePassContextMenuItems.EndUpdate();

            lstKeePassContextMenuItemsAvailable.BeginUpdate();
            list1.Clear();
            foreach (var menu in KprMenu.MenuItemValues.Where(menu => !KprMenu.DefaultContextMenuItems.HasFlag(menu)))
            {
                list1.Add(new KeyValuePair<KprMenu.MenuItem, string>(menu, menu.GetText()));
                var midx = KprMenu.MenuItemValues.IndexOf(menu);
                var flp = flps.FirstOrDefault(x => tblKeyboardSettings.GetRow(x) - 1 == midx);
                if (flp != null)
                {
                    var chk = flp.Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 0);
                    if (chk.Checked)
                    {
                        chk.Tag = true;
                        chk.Checked = false;
                    }
                }
            }
            lstKeePassContextMenuItemsAvailable.EndUpdate();

            ResetActiveControl(sender as Control);
        }

        private void cmdKeePassToolbarItemsAdd_Click(object sender, EventArgs e)
        {
            var list1 = lstKeePassToolbarItemsAvailable.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;
            var list2 = lstKeePassToolbarItems.DataSource as BindingList<KeyValuePair<KprMenu.MenuItem, string>>;

            var selectedIndices = lstKeePassToolbarItemsAvailable.SelectedIndices.Cast<int>().ToList();
            var flps = tblKeyboardSettings.Controls.OfType<TableLayoutPanel>();

            lstKeePassToolbarItems.BeginUpdate();
            foreach (var i in selectedIndices.OrderBy(x => x))
            {
                var item = list1[i];
                var newIndex = list2.IndexOf(list2.FirstOrDefault(x => x.Key > item.Key));
                if (newIndex < 0)
                    newIndex = list2.Count;
                list2.Insert(newIndex, item);
                var midx = KprMenu.MenuItemValues.IndexOf(item.Key);
                var flp = flps.FirstOrDefault(x => tblKeyboardSettings.GetRow(x) - 1 == midx);
                if (flp != null)
                {
                    var chk = flp.Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 1);
                    if (!chk.Checked)
                    {
                        chk.Tag = true;
                        chk.Checked = true;
                    }
                }
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
            var flps = tblKeyboardSettings.Controls.OfType<TableLayoutPanel>();

            lstKeePassToolbarItemsAvailable.BeginUpdate();
            foreach (var i in selectedIndices.OrderBy(x => x))
            {
                var item = list2[i];
                var newIndex = list1.IndexOf(list1.FirstOrDefault(x => x.Key > item.Key));
                if (newIndex < 0)
                    newIndex = list1.Count;
                list1.Insert(newIndex, item);
                var midx = KprMenu.MenuItemValues.IndexOf(item.Key);
                var flp = flps.FirstOrDefault(x => tblKeyboardSettings.GetRow(x) - 1 == midx);
                if (flp != null)
                {
                    var chk = flp.Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 1);
                    if (chk.Checked)
                    {
                        chk.Tag = true;
                        chk.Checked = false;
                    }
                }
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

            var flps = tblKeyboardSettings.Controls.OfType<TableLayoutPanel>();

            lstKeePassToolbarItems.BeginUpdate();
            list2.Clear();
            foreach (var menu in KprMenu.MenuItemValues.Where(menu => KprMenu.DefaultToolbarItems.HasFlag(menu)))
            {
                list2.Add(new KeyValuePair<KprMenu.MenuItem, string>(menu, menu.GetText()));
                var midx = KprMenu.MenuItemValues.IndexOf(menu);
                var flp = flps.FirstOrDefault(x => tblKeyboardSettings.GetRow(x) - 1 == midx);
                if (flp != null)
                {
                    var chk = flp.Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 1);
                    if (!chk.Checked)
                    {
                        chk.Tag = true;
                        chk.Checked = true;
                    }
                }
            }
            lstKeePassToolbarItems.EndUpdate();

            lstKeePassToolbarItemsAvailable.BeginUpdate();
            list1.Clear();
            foreach (var menu in KprMenu.MenuItemValues.Where(menu => !KprMenu.DefaultToolbarItems.HasFlag(menu)))
            {
                list1.Add(new KeyValuePair<KprMenu.MenuItem, string>(menu, menu.GetText()));
                var midx = KprMenu.MenuItemValues.IndexOf(menu);
                var flp = flps.FirstOrDefault(x => tblKeyboardSettings.GetRow(x) - 1 == midx);
                if (flp != null)
                {
                    var chk = flp.Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 1);
                    if (chk.Checked)
                    {
                        chk.Tag = true;
                        chk.Checked = false;
                    }
                }
            }
            lstKeePassToolbarItemsAvailable.EndUpdate();

            ResetActiveControl(sender as Control);
        }

        private void lst_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Modifiers == Keys.None)
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        var listBox = sender as ListBox;
                        if (listBox.SelectedIndices.Count == 0)
                            return;
                        e.IsInputKey = true;
                        break;
                }
        }

        private void lst_KeyDownEnter(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.None)
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        var listBox = sender as ListBox;
                        var button = listBox.Tag as Button;
                        button.PerformClick();
                        e.SuppressKeyPress = e.Handled = true;
                        break;
                }
        }

        private void lst_KeyDownDelete(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.None)
                switch (e.KeyCode)
                {
                    case Keys.Delete:
                        var listBox = sender as ListBox;
                        var button = listBox.Tag as Button;
                        button.PerformClick();
                        e.SuppressKeyPress = e.Handled = true;
                        break;
                }
        }


        [DllImport("User32.dll", EntryPoint = "SendMessageW", SetLastError = false)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, int itemIndex, int itemHeight);

        private const int LB_SETITEMHEIGHT = 0x01A0;

        private void lst_SizeChanged(object sender, EventArgs e)
        {
            if (UseWaitCursor)
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
            if (UseWaitCursor)
                return;

            var lst = sender as ListBox;

            if (e.Index >= 0)
            {
                e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                e.Graphics.TextContrast = 0;
                e.ItemHeight = Math.Max(
                    lst.ItemHeight,
                    TextRenderer.MeasureText(
                        e.Graphics,
                        ((KeyValuePair<KprMenu.MenuItem, string>)lst.Items[e.Index]).Value,
                        lst.Font,
                        new Size(lst.Width - (ScrollbarUtil.GetVisibleScrollbars(lst) >= ScrollBars.Vertical ? UIUtil.GetVScrollBarWidth() : 0) - 6 - 3, 0),
                        TextFormatFlags.VerticalCenter |
                            TextFormatFlags.WordBreak |
                            TextFormatFlags.NoPadding |
                            TextFormatFlags.TextBoxControl |
                            TextFormatFlags.PreserveGraphicsClipping |
                            TextFormatFlags.PreserveGraphicsTranslateTransform).Height + 4);
            }
            else
                e.ItemHeight = lst.ItemHeight;
        }

        private void lst_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (UseWaitCursor)
                return;

            if (e.Index >= 0)
            {
                var lst = sender as ListBox;

                e.DrawBackground();

                TextRenderer.DrawText(
                    e.Graphics,
                    ((KeyValuePair<KprMenu.MenuItem, string>)lst.Items[e.Index]).Value,
                    e.Font,
                    new Rectangle(new Point(e.Bounds.X + 3, e.Bounds.Y), new Size(lst.Width - (ScrollbarUtil.GetVisibleScrollbars(lst) >= ScrollBars.Vertical ? UIUtil.GetVScrollBarWidth() : 0) - 6 - 3, e.Bounds.Height)),
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

        public class MenuItemSettings
        {
            public Hotkey Hotkey { get; set; }
            public bool ContextMenuChecked { get; set; }
            public bool ToolbarChecked { get; set; }
            internal Image Image { get; set; }
        }

        private interface IGetSetHotkey
        {
            Hotkey this[KprMenu.MenuItem menuItem] { get; set; }
        }

        private interface IGetSetContextMenuChecked
        {
            bool this[KprMenu.MenuItem menuItem] { get; set; }
        }

        private interface IGetSetToolbarChecked
        {
            bool this[KprMenu.MenuItem menuItem] { get; set; }
        }

        private interface IGetSetImage
        {
            Image this[KprMenu.MenuItem menuItem] { get; set; }
        }

        private class KprKeyboardSettingsTableLayoutPanel : TableLayoutPanel, IGetSetHotkey, IGetSetContextMenuChecked, IGetSetToolbarChecked, IGetSetImage
        {
            public event MenuItemCheckedChanged ContextMenu_CheckedChanged;
            public event MenuItemCheckedChanged Toolbar_CheckedChanged;
            public event MenuItemHotkeyChanged Hotkey_Changed;

            private readonly IDictionary<KprMenu.MenuItem, CheckBox> _contextMenuControlsCache;
            private readonly IDictionary<KprMenu.MenuItem, CheckBox> _toolbarControlsCache;
            private readonly IDictionary<KprMenu.MenuItem, KprHotkeyBox> _hotkeyControlsCache;
            private readonly IDictionary<KprMenu.MenuItem, Button> _imageControlsCache;
            private readonly IDictionary<KprMenu.MenuItem, Button> _resetButtonControlsCache;

            private readonly ManualResetEventSlim _controlsCreated;
            private readonly ImageList _imageList;

            internal ManualResetEventSlim ControlsCreated { get { return _controlsCreated; } }

            public MenuItemSettings this[KprMenu.MenuItem menuItem]
            {
                get
                {
                    return new MenuItemSettings
                    {
                        Hotkey = (this as IGetSetHotkey)[menuItem],
                        ContextMenuChecked = (this as IGetSetContextMenuChecked)[menuItem],
                        ToolbarChecked = (this as IGetSetToolbarChecked)[menuItem],
                        Image = (this as IGetSetImage)[menuItem]
                    };
                }
                set
                {
                    (this as IGetSetHotkey)[menuItem] = value.Hotkey;
                    (this as IGetSetContextMenuChecked)[menuItem] = value.ContextMenuChecked;
                    (this as IGetSetToolbarChecked)[menuItem] = value.ToolbarChecked;
                    (this as IGetSetImage)[menuItem] = value.Image;
                }
            }

            Hotkey IGetSetHotkey.this[KprMenu.MenuItem menuItem]
            {
                get
                {
                    KprHotkeyBox kph = null;
                    if (!_hotkeyControlsCache.TryGetValue(menuItem, out kph) || kph == null)
                    {
                        var idx = KprMenu.MenuItemValues.IndexOf(menuItem);
                        kph = _hotkeyControlsCache[menuItem] = Controls.OfType<KprHotkeyBox>().FirstOrDefault(x => GetRow(x) - 1 == idx);
                    }
                    return kph.Hotkey;
                }
                set
                {
                    KprHotkeyBox kph = null;
                    if (!_hotkeyControlsCache.TryGetValue(menuItem, out kph) || kph == null)
                    {
                        var idx = KprMenu.MenuItemValues.IndexOf(menuItem);
                        kph = _hotkeyControlsCache[menuItem] = Controls.OfType<KprHotkeyBox>().FirstOrDefault(x => GetRow(x) - 1 == idx);
                    }
                    if (kph.Hotkey != value)
                        kph.Hotkey = value;
                }
            }

            bool IGetSetContextMenuChecked.this[KprMenu.MenuItem menuItem]
            {
                get
                {
                    CheckBox cb = null;
                    if (!_contextMenuControlsCache.TryGetValue(menuItem, out cb) || cb == null)
                    {
                        var idx = KprMenu.MenuItemValues.IndexOf(menuItem);
                        cb = _contextMenuControlsCache[menuItem] = Controls.OfType<TableLayoutPanel>().FirstOrDefault(x => GetRow(x) - 1 == idx).Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 0);
                    }
                    return cb.Checked;
                }
                set
                {
                    CheckBox cb = null;
                    if (!_contextMenuControlsCache.TryGetValue(menuItem, out cb) || cb == null)
                    {
                        var idx = KprMenu.MenuItemValues.IndexOf(menuItem);
                        cb = _contextMenuControlsCache[menuItem] = Controls.OfType<TableLayoutPanel>().FirstOrDefault(x => GetRow(x) - 1 == idx).Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 0);
                    }
                    if (cb.Checked != value)
                        cb.Checked = value;
                }
            }

            bool IGetSetToolbarChecked.this[KprMenu.MenuItem menuItem]
            {
                get
                {
                    CheckBox cb = null;
                    if (!_toolbarControlsCache.TryGetValue(menuItem, out cb) || cb == null)
                    {
                        var idx = KprMenu.MenuItemValues.IndexOf(menuItem);
                        cb = _toolbarControlsCache[menuItem] = Controls.OfType<TableLayoutPanel>().FirstOrDefault(x => GetRow(x) - 1 == idx).Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 1);
                    }
                    return cb.Checked;
                }
                set
                {
                    CheckBox cb = null;
                    if (!_toolbarControlsCache.TryGetValue(menuItem, out cb) || cb == null)
                    {
                        var idx = KprMenu.MenuItemValues.IndexOf(menuItem);
                        cb = _toolbarControlsCache[menuItem] = Controls.OfType<TableLayoutPanel>().FirstOrDefault(x => GetRow(x) - 1 == idx).Controls.OfType<CheckBox>().FirstOrDefault(x => (x.Parent as TableLayoutPanel).GetColumn(x) == 1);
                    }
                    if (cb.Checked != value)
                        cb.Checked = value;
                }
            }

            Image IGetSetImage.this[KprMenu.MenuItem menuItem]
            {
                get
                {
                    Button b = null;
                    if (!_imageControlsCache.TryGetValue(menuItem, out b) || b == null)
                    {
                        var idx = KprMenu.MenuItemValues.IndexOf(menuItem);
                        b = _imageControlsCache[menuItem] = Controls.OfType<Button>().FirstOrDefault(x => GetColumn(x) == 1 && GetRow(x) - 1 == idx);
                    }
                    return b.Image;
                }
                set
                {
                    Button b = null;
                    if (!_imageControlsCache.TryGetValue(menuItem, out b) || b == null)
                    {
                        var idx = KprMenu.MenuItemValues.IndexOf(menuItem);
                        b = _imageControlsCache[menuItem] = Controls.OfType<Button>().FirstOrDefault(x => GetColumn(x) == 1 && GetRow(x) - 1 == idx);
                    }
                    if (b.Image != value)
                        b.Image = value;
                    b.Visible = value != null;
                }
            }

            public KprKeyboardSettingsTableLayoutPanel() : base()
            {
                _contextMenuControlsCache = new Dictionary<KprMenu.MenuItem, CheckBox>();
                _toolbarControlsCache = new Dictionary<KprMenu.MenuItem, CheckBox>();
                _hotkeyControlsCache = new Dictionary<KprMenu.MenuItem, KprHotkeyBox>();
                _imageControlsCache = new Dictionary<KprMenu.MenuItem, Button>();
                _resetButtonControlsCache = new Dictionary<KprMenu.MenuItem, Button>();
                _controlsCreated = new ManualResetEventSlim(false);
                _imageList = KprImageList.Instance;

                DoubleBuffered = true;
                CreateControls();
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
            }

            private void CreateControls()
            {
                Control control;
                var controls = new List<Control>();

                SuspendLayout();

                var contextMenuText = KprResourceManager.Instance["Context menu"];
                var toolbarText = KprResourceManager.Instance["Toolbar"];

                for (var i = 0; i < KprMenu.MenuItemValues.Count; i++)
                {
                    var menuItem = KprMenu.MenuItemValues[i];
                    var row = i + 1;

                    control = new Label
                    {
                        Text = menuItem.GetText(),
                        AutoEllipsis = true,
                        AutoSize = true,
                        Anchor = AnchorStyles.Left,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Margin = new Padding(3, 0, 3, 0)
                    };
                    control.SuspendLayout();
                    controls.Add(control);
                    Controls.Add(control, 0, row);

                    var b = new Button
                    {
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Anchor = AnchorStyles.Left,
                        Margin = new Padding(0, 0, 6, 0),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = SystemColors.Control,
                        UseVisualStyleBackColor = false,
                        Visible = false
                    };
                    b.FlatAppearance.BorderSize = 0;
                    b.FlatAppearance.MouseDownBackColor = SystemColors.Control;
                    b.FlatAppearance.MouseOverBackColor = SystemColors.Control;
                    control = b;
                    control.SuspendLayout();
                    controls.Add(control);
                    Controls.Add(_imageControlsCache[menuItem] = b, 1, row);

                    var tlp = new TableLayoutPanel
                    {
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Dock = DockStyle.Left,
                        ColumnCount = 2,
                        RowCount = 1
                    };
                    tlp.RowStyles.Clear();
                    tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                    tlp.ColumnStyles.Clear();
                    tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                    tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                    control = tlp;
                    control.SuspendLayout();
                    controls.Add(control);
                    var cb = new CheckBox
                    {
                        Text = contextMenuText,
                        AutoSize = true,
                        Dock = DockStyle.Left,
                        Margin = new Padding(3, 3, 0, 0)
                    };
                    cb.CheckedChanged += Checkbox_CheckedChanged;
                    control = cb;
                    control.SuspendLayout();
                    controls.Add(control);
                    tlp.Controls.Add(_contextMenuControlsCache[menuItem] = cb, 0, 0);
                    cb = new CheckBox
                    {
                        Text = toolbarText,
                        AutoSize = true,
                        Dock = DockStyle.Left,
                        Margin = new Padding(3, 3, 0, 0)
                    };
                    cb.CheckedChanged += Checkbox_CheckedChanged;
                    control = cb;
                    control.SuspendLayout();
                    controls.Add(control);
                    tlp.Controls.Add(_toolbarControlsCache[menuItem] = cb, 1, 0);
                    tlp.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
                    control = tlp;
                    control.SuspendLayout();
                    controls.Add(control);
                    Controls.Add(tlp, 2, row);
                    tlp.SetDoubleBuffered();

                    var khb = new KprHotkeyBox
                    {
                        AutoSize = true,
                        AcceptsReturn = true,
                        AcceptsTab = true,
                        Anchor = AnchorStyles.Left | AnchorStyles.Right,
                        Margin = new Padding(3, 3, 3, 2),
                        TabStop = false
                    };
                    khb.HotkeyChanged += HotkeyBox_HotkeyChanged;
                    control = khb;
                    control.SuspendLayout();
                    controls.Add(control);
                    Controls.Add(_hotkeyControlsCache[menuItem] = khb, 3, row);
                    khb.SetDoubleBuffered();

                    b = new Button
                    {
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Anchor = AnchorStyles.Left,
                        Margin = new Padding(0, 2, 2, 0),
                        Padding = new Padding(1),
                        ImageList = _imageList,
                        ImageKey = "Refresh",
                        UseVisualStyleBackColor = true
                    };
                    b.Click += HotkeyReset_Click;
                    control = b;
                    control.SuspendLayout();
                    controls.Add(control);
                    Controls.Add(_resetButtonControlsCache[menuItem] = b, 4, row);
                }

                _controlsCreated.Set();

                EventHandler handler = null;
                handler = (s, e) =>
                {
                    try
                    {
                        HandleCreated -= handler;
                    }
                    catch { }
                    Task.Factory.StartNew(() =>
                    {
                        if (!_controlsCreated.IsSet && !_controlsCreated.Wait(TimeSpan.FromSeconds(5)))
                            return;

                        Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                        {
                            var ksc = Parent as KprSettingsControl;
                            if (ksc is KprSettingsControl)
                                foreach (var menuItem in KprMenu.MenuItemValues)
                                {
                                    ksc.ttGeneric.SetToolTip(_contextMenuControlsCache[menuItem], "Toggle visibility in context menu");
                                    ksc.ttGeneric.SetToolTip(_toolbarControlsCache[menuItem], "Toggle visibility on toolbar");
                                    ksc.ttGeneric.SetToolTip(_resetButtonControlsCache[menuItem], "Reset shortcut to default");
                                }

                            foreach (var c in controls)
                            {
                                c.ResumeLayout(true);
                                c.PerformLayout();
                            }
                            ResumeLayout(true);
                            PerformLayout();
                        })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                    });
                };
                if (IsHandleCreated)
                    handler(null, EventArgs.Empty);
                else
                    HandleCreated += handler;
            }

            protected override void OnParentChanged(EventArgs e)
            {
                base.OnParentChanged(e);

                var ksc = Parent as KprSettingsControl;
                if (_controlsCreated.IsSet && ksc is KprSettingsControl)
                    foreach (var menuItem in KprMenu.MenuItemValues)
                    {
                        ksc.ttGeneric.SetToolTip(_contextMenuControlsCache[menuItem], "Toggle visibility in context menu");
                        ksc.ttGeneric.SetToolTip(_toolbarControlsCache[menuItem], "Toggle visibility on toolbar");
                        ksc.ttGeneric.SetToolTip(_resetButtonControlsCache[menuItem], "Reset shortcut to default");
                    }
            }

            new public void SuspendLayout()
            {
                base.SuspendLayout();
                if (_controlsCreated.IsSet && IsHandleCreated)
                {
                    Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                    {
                        foreach (var menuItem in KprMenu.MenuItemValues)
                        {
                            _imageControlsCache[menuItem].SuspendLayout();
                            _contextMenuControlsCache[menuItem].SuspendLayout();
                            _toolbarControlsCache[menuItem].SuspendLayout();
                            _hotkeyControlsCache[menuItem].SuspendLayout();
                            _resetButtonControlsCache[menuItem].SuspendLayout();
                        }
                    })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                }
            }

            new public void ResumeLayout(bool performLayout)
            {
                base.ResumeLayout(performLayout);
                if (_controlsCreated.IsSet && IsHandleCreated)
                {
                    Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                    {
                        foreach (var menuItem in KprMenu.MenuItemValues)
                        {
                            _imageControlsCache[menuItem].ResumeLayout(performLayout);
                            _contextMenuControlsCache[menuItem].ResumeLayout(performLayout);
                            _toolbarControlsCache[menuItem].ResumeLayout(performLayout);
                            _hotkeyControlsCache[menuItem].ResumeLayout(performLayout);
                            _resetButtonControlsCache[menuItem].ResumeLayout(performLayout);
                        }
                    })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                }
            }

            new public void ResumeLayout()
            {
                ResumeLayout(true);
            }

            private void HotkeyReset_Click(object sender, EventArgs e)
            {
                var b = sender as Button;
                var row = GetRow(b);
                var hotkeyBox = Controls.OfType<KprHotkeyBox>().FirstOrDefault(x => GetRow(x) == row);
                var menuItem = KprMenu.MenuItemValues[row - 1];
                switch (menuItem)
                {
                    case KprMenu.MenuItem.OpenRdpConnection:
                        hotkeyBox.Hotkey = KprMenu.DefaultOpenRdpConnectionShortcut;
                        break;
                    case KprMenu.MenuItem.OpenRdpConnectionAdmin:
                        hotkeyBox.Hotkey = KprMenu.DefaultOpenRdpConnectionAdminShortcut;
                        break;
                    default:
                        hotkeyBox.Hotkey = Keys.None;
                        break;
                }

                if (Parent is KprSettingsControl)
                    (Parent as KprSettingsControl).ResetActiveControl(sender as Control);
            }

            private void HotkeyBox_HotkeyChanged(object sender, EventArgs e)
            {
                var khb = sender as KprHotkeyBox;
                var menuItem = KprMenu.MenuItemValues[GetRow(khb) - 1];
                if (Hotkey_Changed != null)
                    Hotkey_Changed.Invoke(menuItem, khb.Hotkey);
            }

            private void Checkbox_CheckedChanged(object sender, EventArgs e)
            {
                var cb = sender as CheckBox;

                if (cb.Tag as bool? == true)
                {
                    cb.Tag = null;
                    return;
                }

                var tlp = cb.Parent as TableLayoutPanel;
                var menuItem = KprMenu.MenuItemValues[GetRow(tlp) - 1];
                if (tlp.GetColumn(cb) == 0)
                {
                    if (ContextMenu_CheckedChanged != null)
                        ContextMenu_CheckedChanged.Invoke(menuItem, cb.Checked);
                }
                else
                {
                    if (Toolbar_CheckedChanged != null)
                        Toolbar_CheckedChanged.Invoke(menuItem, cb.Checked);
                }
            }
        }
    }
}
