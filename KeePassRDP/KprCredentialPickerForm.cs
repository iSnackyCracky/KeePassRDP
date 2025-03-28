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
using KeePass.Plugins;
using KeePass.UI;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Utility;
using KeePassRDP.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace KeePassRDP
{
    public partial class KprCredentialPickerForm : Form, IGwmWindow
    {
        private static readonly Lazy<IComparer<string>> _comparer = new Lazy<IComparer<string>>(
            () => new StrCmpLogicalWComparer(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        private class StrCmpLogicalWComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return StrUtil.CompareNaturally(x, y);
            }
        }

        public enum RowHeight : byte
        {
            Default,
            Large
        }

        public bool CanCloseWithoutDataLoss { get { return true; } }

        /// <summary>
        /// <see cref="IEnumerable{T}"/> with selectable entries.
        /// </summary>
        public IEnumerable<PwEntry> RdpAccountEntries { private get; set; }

        /// <summary>
        /// <see cref="PwEntry"/> that contains the URL for the connection.
        /// </summary>
        public PwEntry ConnPwEntry { private get; set; }

        /// <summary>
        /// <see cref="PwUuid"/> from the selected entry of <see cref="RdpAccountEntries"/>.
        /// </summary>
        public PwUuid ReturnUuid { get; private set; }

        private readonly KprConfig _config;
        private readonly IPluginHost _host;
        private readonly string _defaultTitle;

        private Font _font;
        private Font _lastFont;
        private RowHeight _currentRowHeight;
        private Dictionary<PwUuid, int> _iconCache;

        public KprCredentialPickerForm(KprConfig config, IPluginHost host)
        {
            _config = config;
            _host = host;

            RdpAccountEntries = null;
            ConnPwEntry = null;
            ReturnUuid = null;

            InitializeComponent();
            SuspendLayout();

            KprResourceManager.Instance.TranslateMany(
                cmdOk,
                cmdCancel
            );

            _defaultTitle = Text;

            columnPath.Name = "Path";
            columnTitle.Name = "Title";
            columnUserName.Name = "UserName";
            columnNotes.Name = "Notes";

            columnPath.Text = KprResourceManager.Instance[columnPath.Text];
            columnTitle.Text = KprResourceManager.Instance[columnTitle.Text];
            columnUserName.Text = KprResourceManager.Instance[columnUserName.Text];
            columnNotes.Text = KprResourceManager.Instance[columnNotes.Text];

            Util.EnableDoubleBuffered(
                tblCredentialPickerForm,
                lvEntries
            );

            lvEntries.HandleCreated += lvEntries_HandleCreated;

            _iconCache = _host.Database != null && _host.Database.CustomIcons != null && _host.Database.CustomIcons.Count > 0 ?
                _host.Database.CustomIcons.ToDictionary(x => x.Uuid, x => _host.Database.CustomIcons.IndexOf(x)) :
                new Dictionary<PwUuid, int>();

            var m_lvEntries = host.MainWindow.Controls.Find("m_lvEntries", true).FirstOrDefault() as CustomListViewEx;

            if (m_lvEntries != null)
            {
                _lastFont = m_lvEntries.Font;
                lvEntries.Font = (Font)_lastFont.Clone();
                lvEntries.SmallImageList = m_lvEntries.SmallImageList;
            }

            _font = lvEntries.Font;
            _currentRowHeight = RowHeight.Default;

            if (_config.CredPickerLargeRows)
                SetRowHeight(RowHeight.Large);

            // Keep font in sync with KeePass main form.
            // Keep image list in sync with KeePass main form.
            host.MainWindow.UIStateUpdated += KprCredentialPickerForm_UIStateUpdated;

            ResumeLayout(false);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                _host.MainWindow.UIStateUpdated -= KprCredentialPickerForm_UIStateUpdated;

                if (lvEntries.Font != _font)
                    _font.Dispose();
                lvEntries.SmallImageList = null;
                lvEntries.HandleCreated -= lvEntries_HandleCreated;
            }

            base.Dispose(disposing);
        }

        public new DialogResult ShowDialog(IWin32Window owner)
        {
            var control = owner as Control;
            if (control != null && control.InvokeRequired)
                ToolStripManager.Renderer = (ToolStripRenderer)control.Invoke(new Func<object>(() =>
                {
                    return ToolStripManager.Renderer;
                }));

            return base.ShowDialog(owner);
        }

        /// <summary>
        /// Clear <see cref="KprCredentialPickerForm"/> entries list.
        /// </summary>
        public void Clear()
        {
            lvEntries.BeginUpdate();
            lvEntries.Tag = null;
            var col = lvEntries.Columns.ContainsKey("UserName") ? lvEntries.Columns["UserName"].Index : -1;
            if (col >= 0)
                foreach (ListViewItem item in lvEntries.Items)
                    if (item.SubItems.Count >= col && !string.IsNullOrEmpty(item.SubItems[col].Text))
                        MemoryUtil.SecureZeroMemory(item.SubItems[col].Text);
            lvEntries.Items.Clear();
            lvEntries.Groups.Clear();
            lvEntries.EndUpdate();
        }

        /// <summary>
        /// Reset <see cref="KprCredentialPickerForm"/> for next usage.
        /// </summary>
        public void Reset()
        {
            Clear();
            RdpAccountEntries = null;
            ConnPwEntry = null;
            ReturnUuid = null;
            Icon = null;
            Hide();
            DestroyHandle();
            if (Container != null)
                Container.Remove(this);
        }

        /// <summary>
        /// Set row height for entries shown in <see cref="KprCredentialPickerForm"/>.
        /// </summary>
        /// <param name="rowHeight"></param>
        /// <param name="force"></param>
        public void SetRowHeight(RowHeight rowHeight, bool force = false)
        {
            switch(rowHeight)
            {
                case RowHeight.Large:
                    if (force || _currentRowHeight != rowHeight)
                    {
                        var font = _font ?? SystemFonts.DefaultFont;
                        var oldFont = lvEntries.Font;
                        lvEntries.Font = new Font(font.FontFamily, font.Size * 1.5f);
                        if (oldFont != _font)
                            oldFont.Dispose();
                        _currentRowHeight = rowHeight;
                    }
                    break;
                case RowHeight.Default:
                    if (force || _currentRowHeight != rowHeight)
                    {
                        var oldFont = lvEntries.Font;
                        lvEntries.Font = _font;
                        if (oldFont != _font)
                            oldFont.Dispose();
                        _currentRowHeight = rowHeight;
                    }
                    break;
            }

            if (Visible)
                lvEntries.Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (_config.CredPickerSecureDesktop)
                NativeMethods.SetWindowDisplayAffinity(Handle, NativeMethods.WDA_EXCLUDEFROMCAPTURE);
        }

        private void lvEntries_HandleCreated(object sender, EventArgs e)
        {
            try
            {
                if (UIUtil.VistaStyleListsSupported)
                    UIUtil.SetExplorerTheme(lvEntries.Handle);
            }
            catch { }
        }

        private void KprCredentialPickerForm_Load(object sender, EventArgs e)
        {
            // Set configured window size.
            Width = _config.CredPickerWidth;
            Height = _config.CredPickerHeight;

            UseWaitCursor = true;
            SuspendLayout();

            var pwEntry = ConnPwEntry;
            if (pwEntry != null)
            {
                var kprCpcg = _config.CredPickerCustomGroup;
                var kprCptr = _config.CredPickerTriggerRecursive;
                var titleGroup = pwEntry.ParentGroup;
                if (Util.InRdpSubgroup(pwEntry, kprCpcg, kprCptr))
                {
                    var rdpGroup = titleGroup;
                    var triggerGroup = !string.IsNullOrWhiteSpace(kprCpcg) ? kprCpcg : Util.DefaultTriggerGroup;
                    while (rdpGroup != null && !string.Equals(rdpGroup.Name, triggerGroup, StringComparison.Ordinal))
                        rdpGroup = rdpGroup.ParentGroup;
                    if (rdpGroup != null)
                        titleGroup = rdpGroup.ParentGroup;
                }

                // Add path of parent group to window title.
                var title = titleGroup != null ? titleGroup.GetFullPath(Util.GroupSeperator, false) : string.Empty;

                // Add title of current entry to window title.
                var connPwEntryTitle = pwEntry.Strings.ReadSafe(PwDefs.TitleField);
                if (string.IsNullOrWhiteSpace(connPwEntryTitle))
                    connPwEntryTitle = pwEntry.Strings.ReadSafe(PwDefs.UrlField);
                if (!string.IsNullOrWhiteSpace(connPwEntryTitle))
                {
                    connPwEntryTitle = SprEngine.Compile(connPwEntryTitle, new SprContext(pwEntry, _host.Database, SprCompileFlags.Deref)
                    {
                        ForcePlainTextPasswords = false
                    });
                    if (!string.IsNullOrWhiteSpace(connPwEntryTitle))
                        title += string.Format("{0}{1}", Util.GroupSeperator, connPwEntryTitle.Trim());
                }

                Text += title;
            }

            GlobalWindowManager.AddWindow(this);

            UIUtil.SetFocus(lvEntries, this, true);

            lvEntries.SuspendLayout();
            lvEntries.ShowGroups = _config.CredPickerShowInGroups;
            lvEntries.ListViewItemSorter = _config.CredPickerSortOrder;
            lvEntries.ResumeLayout(false);

            if (_config.CredPickerSecureDesktop)
                CenterToScreen();
            else
                CenterToParent();
            LoadListEntries();

            MessageFilter.ListViewGroupHeaderClickHandler.Enable(true);
        }

        private void KprCredentialPickerForm_Shown(object sender, EventArgs e)
        {
            Activate();
            KprCredentialPickerForm_Activated(null, EventArgs.Empty);
        }

        private void KprCredentialPickerForm_Activated(object sender, EventArgs e)
        {
            BringToFront();

            UIUtil.SetFocus(lvEntries, this, true);
            UIUtil.SetFocusedItem(lvEntries, lvEntries.SelectedItems.OfType<ListViewItem>().LastOrDefault() ?? lvEntries.Items.OfType<ListViewItem>().FirstOrDefault(), true);
        }

        private void KprCredentialPickerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorker1.IsBusy && !backgroundWorker1.CancellationPending && backgroundWorker1.WorkerSupportsCancellation)
                backgroundWorker1.CancelAsync();

            MessageFilter.ListViewGroupHeaderClickHandler.Enable(false);
        }

        private void KprCredentialPickerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
            Text = _defaultTitle;
        }

        private void KprCredentialPickerForm_UIStateUpdated(object sender, EventArgs e)
        {
            var m_lvEntries = _host.MainWindow.Controls.Find("m_lvEntries", true).FirstOrDefault() as CustomListViewEx;

            if (m_lvEntries == null)
                return;

            if (m_lvEntries.Font != _lastFont)
            {
                var oldFont = lvEntries.Font;
                lvEntries.Font = (Font)(_lastFont = m_lvEntries.Font).Clone();

                if (oldFont != _font)
                    oldFont.Dispose();
                _font.Dispose();

                _font = lvEntries.Font;
                SetRowHeight(_currentRowHeight, true);
            }

            if (m_lvEntries.SmallImageList != lvEntries.SmallImageList)
            {
                lvEntries.SmallImageList = m_lvEntries.SmallImageList;
                _iconCache = _host.Database != null && _host.Database.CustomIcons != null && _host.Database.CustomIcons.Count > 0 ?
                    _host.Database.CustomIcons.ToDictionary(x => x.Uuid, x => _host.Database.CustomIcons.IndexOf(x)) :
                    new Dictionary<PwUuid, int>();
            }
        }

        private void LoadListEntries()
        {
            if (backgroundWorker1.IsBusy && !backgroundWorker1.CancellationPending && backgroundWorker1.WorkerSupportsCancellation)
                backgroundWorker1.CancelAsync();
            backgroundWorker1.RunWorkerAsync();
        }

        private bool ConfirmDialog()
        {
            // Remember window size.
            if (_config.CredPickerRememberSize)
            {
                _config.CredPickerWidth = Width;
                _config.CredPickerHeight = Height;
            }

            if (_config.CredPickerRememberSortOrder)
                _config.CredPickerSortOrder = lvEntries.ListViewItemSorter as KprListSorter;

            if (lvEntries.SelectedItems.Count != 1)
            {
                VistaTaskDialog.ShowMessageBoxEx(
                    lvEntries.SelectedItems.Count > 1 ?
                        KprResourceManager.Instance["Please select exactly one credential from the list."] :
                        KprResourceManager.Instance["Please select a credential from the list."],
                    null,
                    Util.KeePassRDP,
                    VtdIcon.Information,
                    this,
                    null, 0, null, 0);
                return false;
            }

            ReturnUuid = (PwUuid)lvEntries.SelectedItems[0].Tag;

            return true;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            // Set ReturnPE to the selected account and close form on success.
            if (ConfirmDialog())
                Close();
        }

        private void lvEntries_ItemActivate(object sender, EventArgs e)
        {
            cmdOk.PerformClick();
        }

        private void lvEntries_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (lvEntries.UseWaitCursor)
                return;

            var listSorter = lvEntries.ListViewItemSorter as KprListSorter;
            var sortOrder = listSorter.SortOrder;

            if (lvEntries.Columns[e.Column].Name == listSorter.Column)
                switch (sortOrder)
                {
                    default:
                    case SortOrder.None:
                        sortOrder = SortOrder.Ascending;
                        break;
                    case SortOrder.Ascending:
                        sortOrder = SortOrder.Descending;
                        break;
                    case SortOrder.Descending:
                        sortOrder = SortOrder.None;
                        break;
                }
            else
                sortOrder = SortOrder.Ascending;

            listSorter.Column = sortOrder != SortOrder.None ? lvEntries.Columns[e.Column].Name : lvEntries.Columns[0].Name;
            listSorter.SortOrder = sortOrder;

            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
            {
                lvEntries.Sort();
                UIUtil.SetSortIcon(lvEntries, lvEntries.Columns[listSorter.Column].Index, listSorter.SortOrder);
            })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void lvEntries_KeyDown(object sender, KeyEventArgs e)
        {
            if (lvEntries.UseWaitCursor)
                return;

            var lvi = lvEntries.FocusedItem ?? lvEntries.SelectedItems.OfType<ListViewItem>().FirstOrDefault();

            if (lvi == null)
                return;

            int nextIndex;

            lvEntries.BeginUpdate();
            switch (e.KeyCode)
            {
                case Keys.Down:
                    nextIndex = lvi.Index + 1;
                    if (nextIndex >= lvEntries.Items.Count)
                        nextIndex = 0;
                    lvi.Focused = lvi.Selected = false;
                    lvi = lvEntries.Items.OfType<ListViewItem>().Single(x => x.Index == nextIndex);
                    lvi.Focused = lvi.Selected = true;
                    lvEntries.EnsureVisible(nextIndex);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
                case Keys.Up:
                    nextIndex = lvi.Index - 1;
                    if (nextIndex < 0)
                        nextIndex = lvEntries.Items.Count - 1;
                    lvi.Focused = lvi.Selected = false;
                    lvi = lvEntries.Items.OfType<ListViewItem>().Single(x => x.Index == nextIndex);
                    lvi.Focused = lvi.Selected = true;
                    lvEntries.EnsureVisible(nextIndex);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
            }
            lvEntries.EndUpdate();
        }

        private void lvEntries_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (lvEntries.UseWaitCursor)
                return;

            if (_currentRowHeight == RowHeight.Default)
            {
                e.DrawDefault = true;
                return;
            }

            TextRenderer.DrawText(
                e.Graphics,
                e.Header.Text,
                _font ?? SystemFonts.DefaultFont,
                new Rectangle(new Point(e.Bounds.X + 6, e.Bounds.Y + 3), new Size(e.Bounds.Width - 12, e.Bounds.Height - 6)),
                Color.Black,
                TextFormatFlags.EndEllipsis |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.Default |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.TextBoxControl |
                    TextFormatFlags.PreserveGraphicsClipping |
                    TextFormatFlags.PreserveGraphicsTranslateTransform);
        }

        private void lvEntries_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (lvEntries.UseWaitCursor)
                return;

            e.DrawDefault = true;
        }

        private void lvEntries_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (lvEntries.UseWaitCursor)
                return;

            e.DrawDefault = true;
        }

        private volatile bool _lvEntriesResizing = false;
        private void lvEntries_SizeChanged(object sender, EventArgs e)
        {
            if (_lvEntriesResizing || (lvEntries.UseWaitCursor && sender != null))
                return;

            _lvEntriesResizing = true;

            var columnsCount = lvEntries.Columns.Count - 1;

            if (columnsCount >= 0)
            {
                var wasActiveControl = ActiveControl == lvEntries;
                lvEntries.Visible = false;
                lvEntries.SuspendLayout();
                lvEntries.BeginUpdate();

                var oldFont = lvEntries.Font;
                if (_currentRowHeight == RowHeight.Large)
                    lvEntries.Font = _font;

                if (sender == null)
                {
                    if (lvEntries.Items.Count > 0)
                    {
                        lvEntries.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                        var minWidths = lvEntries.Columns.OfType<ColumnHeader>().Select(x => x.Width).ToArray();

                        lvEntries.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                        for (var i = columnsCount; i >= 0; i--)
                        {
                            var width = lvEntries.Columns[i].Width;
                            if (width > 0 && width < minWidths[i])
                                lvEntries.Columns[i].Width = minWidths[i];
                        }
                    }
                    else
                        lvEntries.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    lvEntries.Columns[columnsCount].Width = -2;
                }

                var emptyNotes = lvEntries.Items.OfType<ListViewItem>().All(x => string.IsNullOrEmpty(x.SubItems[columnNotes.Index].Text));

                lvEntries.Columns.Add(string.Empty, 0);

                var column = lvEntries.Columns[columnsCount];
                column.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                var minWidth = column.Width;
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                column.Width = Math.Max(minWidth, column.Width);

                lvEntries.Columns.RemoveAt(lvEntries.Columns.Count - 1);

                if (emptyNotes)
                    lvEntries.Columns[columnsCount].Width = 0;

                column = lvEntries.Columns[1];
                column.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                minWidth = column.Width;
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                var oldWidth = Math.Max(minWidth, column.Width);
                var allWidth = lvEntries.Columns.OfType<ColumnHeader>().Sum(x => x.Width);
                column.Width = Math.Max(oldWidth, lvEntries.Width - (allWidth - column.Width));

                if (!emptyNotes)
                    lvEntries.Columns[columnsCount].Width = -2;

                if (lvEntries.Items.Count == 0)
                {
                    allWidth = lvEntries.Columns.OfType<ColumnHeader>().Sum(x => x.Width);
                    if (allWidth > lvEntries.Width)
                        column.Width -= Math.Min(allWidth - lvEntries.Width, column.Width);
                }

                if (_currentRowHeight == RowHeight.Large)
                    lvEntries.Font = oldFont;

                if (ScrollbarUtil.GetVisibleScrollbars(lvEntries) >= ScrollBars.Vertical)
                    column.Width = Math.Max(oldWidth, column.Width - UIUtil.GetVScrollBarWidth());

                lvEntries.EndUpdate();
                lvEntries.ResumeLayout(false);
                lvEntries.Visible = true;
                if (wasActiveControl)
                    ActiveControl = lvEntries;
            }

            _lvEntriesResizing = false;
        }

        private void lvEntries_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (_lvEntriesResizing || lvEntries.UseWaitCursor)
                return;

            lvEntries_SizeChanged(sender, EventArgs.Empty);
            lvEntries.Invalidate();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var groups = new Dictionary<string, ListViewGroup>();
            var seperator = lvEntries.ShowGroups ? Util.GroupSeperator : @"\";

            var items = RdpAccountEntries
                .Distinct()
                .OrderBy(account => account.ParentGroup.GetFullPath(@"\", false), _comparer.Value)
                .Select((account, index) =>
                {
                    string title, username, notes;

                    var path = account.ParentGroup.GetFullPath(seperator, false);

                    ListViewGroup group;
                    if (!groups.TryGetValue(path, out group))
                        groups.Add(path, group = new ListViewGroup(path));

                    if (_config.KeePassShowResolvedReferences)
                    {
                        var ctx = new SprContext(account, _host.Database, SprCompileFlags.Deref)
                        {
                            ForcePlainTextPasswords = false
                        };
                        title = SprEngine.Compile(account.Strings.ReadSafe(PwDefs.TitleField), ctx);

                        var usernameChars = account.Strings.GetSafe(PwDefs.UserNameField).ReadChars();
                        var entryUsername = new string(usernameChars);
                        username = SprEngine.Compile(entryUsername, ctx);
                        if (username != entryUsername)
                            MemoryUtil.SecureZeroMemory(entryUsername);
                        MemoryUtil.SecureZeroMemory(usernameChars);

                        notes = SprEngine.Compile(account.Strings.ReadSafe(PwDefs.NotesField), ctx);
                    }
                    else
                    {
                        title = account.Strings.ReadSafe(PwDefs.TitleField);
                        var usernameChars = account.Strings.GetSafe(PwDefs.UserNameField).ReadChars();
                        username = new string(usernameChars);
                        MemoryUtil.SecureZeroMemory(usernameChars);
                        notes = account.Strings.ReadSafe(PwDefs.NotesField);
                    }

                    int iconIndex;
                    // Create ListViewItem from PwEntry.
                    var lvi = new ListViewItem
                    {
                        ForeColor = account.ForegroundColor,
                        BackColor = account.BackgroundColor,
                        Font = _font,
                        Text = lvEntries.ShowGroups ? title : path,
                        Tag = account.Uuid,
                        ImageIndex = account.CustomIconUuid.Equals(PwUuid.Zero) ?
                            (int)account.IconId :
                            (int)PwIcon.Count +
                            (_iconCache.TryGetValue(account.CustomIconUuid, out iconIndex) ? iconIndex : -1), //_host.Database.GetCustomIconIndex(account.CustomIconUuid),
                        UseItemStyleForSubItems = true,
                        Group = group
                    };
                    if (!lvEntries.ShowGroups)
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Text = title });
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Text = username });
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem { Text = StrUtil.MultiToSingleLine(notes) });

                    return lvi;
                })
                .ToArray();

            var groupsArray = groups.Values.ToArray();

            lvEntries.UseWaitCursor = true;
            lvEntries.BeginUpdate();
            lvEntries.Tag = items.ToList();

            var invoke = BeginInvoke(new Action(() =>
            {
                if (lvEntries.ShowGroups && lvEntries.Columns[0] == columnPath)
                    lvEntries.Columns.RemoveAt(0);

                if (!lvEntries.ShowGroups && lvEntries.Columns[0] != columnPath)
                    lvEntries.Columns.Insert(0, columnPath);

                if (items.Length > 0)
                {
                    lvEntries.Groups.AddRange(groupsArray);
                    lvEntries.Items.AddRange(items);
                }

                if (lvEntries.ListViewItemSorter == null)
                {
                    lvEntries.ListViewItemSorter = new KprListSorter();
                    UIUtil.SetSortIcon(lvEntries, -1, SortOrder.None);
                }
                else
                {
                    //lvEntries.Sort();
                    var kprListSorter = lvEntries.ListViewItemSorter as KprListSorter;
                    UIUtil.SetSortIcon(lvEntries, lvEntries.Columns[kprListSorter.Column].Index, kprListSorter.SortOrder);
                }

                if (lvEntries.Items.Count > 0)
                {
                    if (Program.Config.MainWindow.EntryListAlternatingBgColors)
                    {
                        var clrAlt = UIUtil.GetAlternateColorEx(lvEntries.BackColor);
                        UIUtil.SetAlternatingBgColors(lvEntries, clrAlt, Program.Config.MainWindow.EntryListAlternatingBgColors);
                    }

                    UIUtil.SetFocus(lvEntries, this, true);
                    UIUtil.SetFocusedItem(lvEntries, lvEntries.Items[0], true);
                }

                lvEntries.UseWaitCursor = false;
                lvEntries.EndUpdate();
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

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (UseWaitCursor)
            {
                ResumeLayout(false);
                UseWaitCursor = false;

                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    try
                    {
                        lvEntries_SizeChanged(null, EventArgs.Empty);
                    }
                    catch { }
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }
        }
    }

    [DataContract(IsReference = false)]
    public sealed class KprListSorter : IComparer, IEquatable<KprListSorter>
    {
        private const string DefaultColumn = "Title";

        [NonSerialized]
        private static readonly Lazy<DataContractSerializer> _serializer = new Lazy<DataContractSerializer>(
            () => new DataContractSerializer(typeof(KprListSorter)),
            LazyThreadSafetyMode.ExecutionAndPublication);

        [IgnoreDataMember]
        public static DataContractSerializer Serializer { get { return _serializer.Value; } }

        [DataMember(EmitDefaultValue = false, IsRequired = false, Order = 1)]
        public string Column { get; set; }

        [DataMember(EmitDefaultValue = false, IsRequired = false, Order = 2)]
        public SortOrder SortOrder { get; set; }

        public KprListSorter()
        {
            Column = DefaultColumn;
            SortOrder = SortOrder.None;
        }

        [OnSerializing]
        public void OnSerializing(StreamingContext context)
        {
            if (Column == DefaultColumn)
                Column = null;
        }

        [OnSerialized]
        public void OnSerialized(StreamingContext context)
        {
            if (Column == null)
                Column = DefaultColumn;
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (Column == null)
                Column = DefaultColumn;
        }

        public int Compare(object x, object y)
        {
            var listViewItem = (ListViewItem)x;
            var listViewItem2 = (ListViewItem)y;

            if (listViewItem.ListView != listViewItem2.ListView)
                return -1;

            if (SortOrder == SortOrder.None)
            {
                var items = listViewItem.ListView.Tag as List<ListViewItem>;
                return items.IndexOf(listViewItem) > items.IndexOf(listViewItem2) ? 1 : -1;
            }

            if (listViewItem.ListView.ShowGroups &&
                listViewItem.Group != null &&
                listViewItem2.Group != null &&
                listViewItem.Group != listViewItem2.Group)
                return StrUtil.CompareNaturally(listViewItem.Group.Header, listViewItem2.Group.Header);

            var column = listViewItem.ListView.Columns.ContainsKey(Column) ? listViewItem.ListView.Columns[Column].Index : 0;
            var flag = SortOrder == SortOrder.Descending;
            string text, text2;

            if (column < 0)
                column = 0;

            if (column <= 0 ||
                listViewItem.SubItems.Count <= column ||
                listViewItem2.SubItems.Count <= column)
            {
                text = listViewItem.Text;
                text2 = listViewItem2.Text;
            }
            else
            {
                text = listViewItem.SubItems[column].Text;
                text2 = listViewItem2.SubItems[column].Text;
            }

            var result = StrUtil.CompareNaturally(text, text2);
            var startColumn = column;

            // Compare surrounding columns when text is equal.
            if (result == 0)
            {
                flag = false;
                while (result == 0 && listViewItem.SubItems.Count - 1 > column && listViewItem2.SubItems.Count - 1 > column)
                {
                    column++;
                    text = listViewItem.SubItems[column].Text;
                    text2 = listViewItem2.SubItems[column].Text;
                    result = StrUtil.CompareNaturally(text, text2);
                }
            }

            if (result == 0 && startColumn > 0)
            {
                flag = false;
                column = startColumn;
                while (result == 0 && column > 0)
                {
                    column--;
                    text = listViewItem.SubItems[column].Text;
                    text2 = listViewItem2.SubItems[column].Text;
                    result = StrUtil.CompareNaturally(text, text2);
                }
            }

            return flag ? -result : result;
        }

        public KprListSorter Clone()
        {
            return new KprListSorter
            {
                Column = Column,
                SortOrder = SortOrder,
            };
        }

        public override int GetHashCode()
        {
            return Column.GetHashCode() ^ SortOrder.GetHashCode();
        }

        public string Serialize()
        {
            using (var ms = new MemoryStream())
            //using (var writer = XmlDictionaryWriter.CreateTextWriter(ms, Encoding.UTF8, true))
            {
                //Serializer.WriteObject(writer, this);
                Serializer.WriteObject(ms, this);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static KprListSorter Deserialize(string serialized)
        {
            return Deserialize(Encoding.UTF8.GetBytes(serialized));
        }

        public static KprListSorter Deserialize(byte[] serialized)
        {
            using (var reader = XmlDictionaryReader.CreateTextReader(serialized, 0, serialized.Length, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null))
                return (KprListSorter)Serializer.ReadObject(reader, false);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as KprListSorter);
        }

        public bool Equals(KprListSorter other)
        {
            return this == other;
        }

        public static bool operator ==(KprListSorter a, KprListSorter b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            if (ReferenceEquals(a, null))
                return false;
            if (ReferenceEquals(b, null))
                return false;

            return a.Column == b.Column && a.SortOrder == b.SortOrder;
        }

        public static bool operator !=(KprListSorter a, KprListSorter b)
        {
            return !(a == b);
        }
    }
}