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
        private void Init_TabVault()
        {
            if (_tabVaultInitialized)
                return;

            _tabVaultInitialized = true;

            tabVault.UseWaitCursor = true;
            tabVault.SuspendLayout();
            lvVault.SuspendLayout();

            lvVault.Font = new Font(_font.FontFamily, _font.Size * 2f);
            lvVault.Columns.AddRange(new[]
            {
                new ColumnHeader { Text = "TargetName" },
                new ColumnHeader { Text = "TargetAlias" },
                new ColumnHeader { Text = "UserName" },
                new ColumnHeader { Text = "CredentialBlob" },
                new ColumnHeader { Text = "Type" },
                new ColumnHeader { Text = "Persist" },
                new ColumnHeader { Text = "Flags" },
                new ColumnHeader { Text = "Comment" },
                new ColumnHeader { Text = "LastWritten" },
                new ColumnHeader { Text = "Attributes" }
            });
            lvVault.Invalidated += lvVault_Invalidated;

            KprResourceManager.Instance.TranslateMany(
                chkSavedCredsShowAll
            );

            tabVault.ResumeLayout(false);
            tabVault.UseWaitCursor = false;
            lvVault.ResumeLayout(false);

            if (!tabVault.Created)
                tabVault.CreateControl();

            tblVault.Visible = true;

            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
            {
                lvVault_SizeChanged(null, EventArgs.Empty);
            })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

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
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _pwFont, Text = /*!string.IsNullOrEmpty(cred.CredentialBlob)*/ cred.CredentialBlob != null && cred.CredentialBlob.Length > 0 ? placeholder : string.Empty });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.Type.ToString() });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.Persist.ToString() });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.Flags.ToString() });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.Comment });
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Font = _font, Text = cred.LastWritten.ToString() });
                        if (cred.Attributes.Count > 0)
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Text = cred.Attributes.Count + " " + string.Join(" / ", cred.Attributes.Select(x => x.Key + ": " + x.Value)) });
                        else
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Text = string.Empty });
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

                lvVault.UseWaitCursor = false;
                lvVault.EndUpdate();
            }));

            if (!invoke.IsCompleted)
                Task.Factory.FromAsync(
                    invoke,
                    endinvoke => EndInvoke(endinvoke),
                    TaskCreationOptions.AttachedToParent,
                    TaskScheduler.Default);
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
                    string.Format(KprResourceManager.Instance["Failed to load credentials from vault: {0}"], ex.Message),
                    null,
                    Util.KeePassRDP + " - " + KPRes.Warning,
                    VtdIcon.Warning,
                    this,
                    null, 0, null, 0);
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

            var tf = TextFormatFlags.EndEllipsis |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.Default |
                TextFormatFlags.NoPadding |
                TextFormatFlags.TextBoxControl |
                TextFormatFlags.PreserveGraphicsClipping |
                TextFormatFlags.PreserveGraphicsTranslateTransform;

            if (e.Header.Text == "Attributes")
                tf |= TextFormatFlags.WordBreak;

            TextRenderer.DrawText(
                e.Graphics,
                e.SubItem.Text,
                e.Item.UseItemStyleForSubItems ? e.Item.Font : e.SubItem.Font,
                new Rectangle(new Point(e.Bounds.Location.X + 3, e.Bounds.Location.Y + 3), new Size(e.Bounds.Width - 6, e.Bounds.Height - 6)),
                itemColor,
                tf);
        }

        private volatile bool _lvVaultResizing = false;
        private void lvVault_SizeChanged(object sender, EventArgs e)
        {
            if (_lvVaultResizing || !_tabVaultInitialized || (lvVault.UseWaitCursor && sender != null))
                return;

            _lvVaultResizing = true;

            var columnsCount = lvVault.Columns.Count - 1;

            if (columnsCount >= 0)
            {
                var wasActiveControl = ActiveControl == lvVault;
                lvVault.Visible = false;
                lvVault.SuspendLayout();
                lvVault.BeginUpdate();

                var oldFont = lvVault.Font;
                lvVault.Font = _font;

                if (sender == null)
                {
                    if (lvVault.Items.Count > 0)
                    {
                        lvVault.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                        var minWidths = lvVault.Columns.OfType<ColumnHeader>().Select(x => x.Width).ToArray();

                        lvVault.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                        for (var i = columnsCount; i >= 0; i--)
                        {
                            var width = lvVault.Columns[i].Width;
                            if (width < minWidths[i])
                                lvVault.Columns[i].Width = minWidths[i];
                        }
                    }
                    else
                        lvVault.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    lvVault.Columns[columnsCount].Width = -2;
                }

                lvVault.Columns.Add(string.Empty, 0);

                var column = lvVault.Columns[columnsCount];
                column.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                var minWidth = column.Width;
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                column.Width = Math.Max(minWidth, column.Width);

                lvVault.Columns.RemoveAt(lvVault.Columns.Count - 1);

                column = lvVault.Columns[3];
                column.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                minWidth = column.Width;
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                var oldWidth = Math.Max(minWidth, column.Width);
                var allWidth = lvVault.Columns.OfType<ColumnHeader>().Sum(x => x.Width);
                column.Width = Math.Max(oldWidth, lvVault.Width - (allWidth - column.Width));

                lvVault.Columns[columnsCount].Width = -2;

                if (lvVault.Items.Count == 0)
                {
                    allWidth = lvVault.Columns.OfType<ColumnHeader>().Sum(x => x.Width);
                    if (allWidth > lvVault.Width)
                        column.Width -= Math.Min(allWidth - lvVault.Width, column.Width);
                }

                lvVault.Font = oldFont;

                if (ScrollbarUtil.GetVisibleScrollbars(lvVault) >= ScrollBars.Vertical)
                    column.Width = Math.Max(oldWidth, column.Width - UIUtil.GetVScrollBarWidth());

                lvVault.EndUpdate();
                lvVault.ResumeLayout(false);
                lvVault.Visible = true;
                if (wasActiveControl)
                    ActiveControl = lvVault;
            }

            _lvVaultResizing = false;
        }

        private void lvVault_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (_lvVaultResizing || !_tabVaultInitialized || lvVault.UseWaitCursor)
                return;

            lvVault_SizeChanged(sender, EventArgs.Empty);
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