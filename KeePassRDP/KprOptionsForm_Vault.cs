/*
 *  Copyright (C) 2018 - 2024 iSnackyCracky, NETertainer
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

using KeePass.Resources;
using KeePass.UI;
using KeePassRDP.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP
{
    public partial class KprOptionsForm
    {
        private void LoadCredentials()
        {
            lvVault.UseWaitCursor = true;
            lvVault.BeginUpdate();

            var invoke = BeginInvoke(new Action(() =>
            {
                lvVault.Items.Clear();

                if (_credentials != null && _credentials.Length > 0)
                {
                    var placeholder = new string(SecureTextBoxEx.PasswordCharEx, 12);
                    var items = (chkSavedCredsShowAll.Checked ? _credentials : _credentials.Where(cred => cred.Comment == KprCredential.CredentialComment)).Select(cred =>
                    {
                        var lvi = new ListViewItem
                        {
                            Font = _font,
                            Text = cred.TargetName
                        };
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.TargetAlias });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.UserName });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _pwFont, Text = !string.IsNullOrEmpty(cred.CredentialBlob) ? placeholder : string.Empty });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.Type.ToString() });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.Persist.ToString() });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.Flags.ToString() });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.Comment });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.LastWritten.ToString() });
                        if (cred.Attributes.Count > 0)
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Text = cred.Attributes.Count + " " + string.Join(" / ", cred.Attributes.Select(x => x.Key + ": " + x.Value)) });
                        return lvi;
                    }).ToArray();
                    if (items.Length > 0)
                    {
                        items[0].Selected = items[0].Focused = true;
                        lvVault.Items.AddRange(items);
                        if (lvVault.Items.Count > 0)
                            lvVault.FocusedItem = lvVault.Items[0];
                    }
                }

                lvVault_SizeChanged(null, EventArgs.Empty);

                lvVault.EndUpdate();
                lvVault.UseWaitCursor = false;
            }));

            if (!invoke.IsCompleted)
                Task.Factory.FromAsync(
                    invoke,
                    endinvoke => EndInvoke(endinvoke),
                    TaskCreationOptions.AttachedToParent,
                    TaskScheduler.Default);
                /*new Task(() =>
                {
                    invoke.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), true);
                    EndInvoke(invoke);
                }, CancellationToken.None, TaskCreationOptions.PreferFairness | TaskCreationOptions.AttachedToParent)
                    .Start(TaskScheduler.Default);*/
            else
                EndInvoke(invoke);
        }

        private void CleanCredentials()
        {
            if (_credentials != null)
            {
                foreach (var cred in _credentials)
                    cred.ZeroMemory();
                _credentials = null;
            }
        }

        private void chkSavedCredsShowAll_CheckedChanged(object sender, EventArgs e)
        {
            LoadCredentials();
        }

        private void cmdRefreshCredentials_Click(object sender, EventArgs e)
        {
            CleanCredentials();

            try
            {
                NativeCredentials.CredEnumerate(null, out _credentials);
            }
            catch (Win32Exception ex)
            {
                VistaTaskDialog.ShowMessageBoxEx(
                    ex.Message,
                    null,
                    Util.KeePassRDP + " - " + KPRes.Warning,
                    VtdIcon.Warning,
                    null, null, 0, null, 0);
            }

            LoadCredentials();

            ResetActiveControl(sender as Control);
        }

        private void lvVault_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (!_tabVaultInitialized || lvVault.UseWaitCursor)
                return;

            e.DrawBackground();

            TextRenderer.DrawText(
                e.Graphics,
                e.Header.Text,
                _font,
                new Rectangle(new Point(e.Bounds.X + 3, e.Bounds.Y + 3), new Size(e.Bounds.Width - 6, e.Bounds.Height - 6)),
                Color.Black,
                TextFormatFlags.EndEllipsis |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.Default |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.TextBoxControl |
                    TextFormatFlags.PreserveGraphicsClipping |
                    TextFormatFlags.PreserveGraphicsTranslateTransform);
        }

        private void lvVault_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (!_tabVaultInitialized || lvVault.UseWaitCursor)
                return;

            var itemColor = e.Item.ForeColor;
            if (e.Item.Selected)
            {
                using (var bkBrush = new SolidBrush(SystemColors.Highlight))
                    e.Graphics.FillRectangle(bkBrush, e.Bounds);
                itemColor = SystemColors.HighlightText;
            }
            else
                e.DrawBackground();

            TextRenderer.DrawText(
                e.Graphics,
                e.Item.Text,
                e.Item.Font,
                new Rectangle(new Point(e.Bounds.X + 3, e.Bounds.Y + 3), new Size(e.Bounds.Width - 6, e.Bounds.Height - 6)),
                itemColor,
                TextFormatFlags.EndEllipsis |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.Default |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.TextBoxControl |
                    TextFormatFlags.PreserveGraphicsClipping |
                    TextFormatFlags.PreserveGraphicsTranslateTransform);

            e.DrawFocusRectangle();
        }

        private void lvVault_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (!_tabVaultInitialized || lvVault.UseWaitCursor)
                return;

            var itemColor = e.SubItem.ForeColor;
            if (e.Item.Selected && e.Item.ListView.FullRowSelect)
            {
                using (var bkBrush = new SolidBrush(SystemColors.Highlight))
                    e.Graphics.FillRectangle(bkBrush, e.Bounds);
                itemColor = SystemColors.HighlightText;
            }
            else
                e.DrawBackground();

            TextRenderer.DrawText(
                e.Graphics,
                e.SubItem.Text,
                e.Item.UseItemStyleForSubItems ? e.Item.Font : e.SubItem.Font,
                new Rectangle(new Point(e.Bounds.Location.X + 3, e.Bounds.Location.Y + 3), new Size(e.Bounds.Width - 6, e.Bounds.Height - 6)),
                itemColor,
                e.Header.Text != "Attributes" ?
                TextFormatFlags.EndEllipsis |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.Default |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.TextBoxControl |
                    TextFormatFlags.PreserveGraphicsClipping |
                    TextFormatFlags.PreserveGraphicsTranslateTransform :
                TextFormatFlags.WordBreak |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.Default |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.TextBoxControl |
                    TextFormatFlags.PreserveGraphicsClipping |
                    TextFormatFlags.PreserveGraphicsTranslateTransform);
        }

        private bool _lvVaultResizing = false;
        private void lvVault_SizeChanged(object sender, EventArgs e)
        {
            if (_lvVaultResizing || !_tabVaultInitialized || (lvVault.UseWaitCursor && sender != null))
                return;

            _lvVaultResizing = true;

            if (sender == null)
            {
                lvVault.SuspendLayout();
                var oldFont = lvVault.Font;
                lvVault.Font = _font;

                if (lvVault.Items.Count > 0)
                    lvVault.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                else
                    lvVault.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                lvVault.Columns[3].Width = -2;
                lvVault.Columns[lvVault.Columns.Count - 1].Width = -2;

                lvVault.Font = oldFont;
                lvVault.ResumeLayout(false);
            }
            /*else
            {
                lvVault.Columns[lvVault.Columns.Count - 1].AutoResize(ColumnHeaderAutoResizeStyle.None);
                lvVault.Columns[lvVault.Columns.Count - 1].Width += lvVault.Width -
                    lvVault.Columns.Cast<ColumnHeader>().Sum(column => column.Width) -
                    (ScrollbarUtil.GetVisibleScrollbars(lvVault).HasFlag(ScrollBars.Vertical) ? UIUtil.GetVScrollBarWidth() : 0) - 10;
            }*/

            _lvVaultResizing = false;
        }

        private void lvVault_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (!_tabVaultInitialized || lvVault.UseWaitCursor)
                return;

            lvVault.Invalidate();
        }

        private void lvVault_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_tabVaultInitialized || lvVault.UseWaitCursor)
                return;

            var item = lvVault.GetItemAt(e.X, e.Y);
            if (item != null && item.Tag == null)
            {
                item.Tag = "tagged";
                lvVault.Invalidate(item.Bounds);
            }
        }

        private void lvVault_Invalidated(object sender, InvalidateEventArgs e)
        {
            if (!_tabVaultInitialized || lvVault.UseWaitCursor)
                return;

            foreach (ListViewItem item in lvVault.Items)
            {
                if (item == null)
                    return;
                if (item.Tag as string == "tagged")
                    item.Tag = null;
            }
        }
    }
}