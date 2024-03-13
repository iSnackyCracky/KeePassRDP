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
    public partial class KprCredentialPickerForm : Form
    {
        private static readonly Lazy<IComparer<string>> _comparer = new Lazy<IComparer<string>>(
            () => new StrCmpLogicalWComparer(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Icon> _icon = new Lazy<Icon>(
            () => IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, UIUtil.GetIconSize().Height),
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

        private readonly KprConfig _config;
        private readonly IPluginHost _host;
        private readonly string _defaultTitle;

        private Font _font;
        private RowHeight _currentRowHeight;
        private Dictionary<PwUuid, int> _iconCache;

        /// <summary>
        /// <see cref="IEnumerable{T}"/> with selectable entries.
        /// </summary>
        public IEnumerable<PwEntry> RdpAccountEntries { private get; set; }

        /// <summary>
        /// <see cref="PwEntry"/> that contains the URL for the connection.
        /// </summary>
        public PwEntry ConnPE { private get; set; }

        /// <summary>
        /// <see cref="PwEntry"/> created for the connection (URL from <see cref="ConnPE"/>, username and password from selected <see cref="RdpAccountEntries"/>).
        /// </summary>
        public PwEntry ReturnPE { get; private set; }

        public KprCredentialPickerForm(KprConfig config, IPluginHost host)
        {
            _config = config;
            _host = host;

            InitializeComponent();

            RdpAccountEntries = null;
            ConnPE = ReturnPE = null;

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

            Util.SetDoubleBuffered(tblCredentialPickerForm);
            Util.SetDoubleBuffered(lvEntries);

            Icon = _icon.Value;

            _iconCache = _host.Database != null && _host.Database.CustomIcons != null && _host.Database.CustomIcons.Count > 0 ?
                _host.Database.CustomIcons.ToDictionary(x => x.Uuid, x => _host.Database.CustomIcons.IndexOf(x)) :
                new Dictionary<PwUuid, int>();

            var m_lvEntries = host.MainWindow.Controls.Find("m_lvEntries", true)[0] as CustomListViewEx;
            var lastFont = m_lvEntries.Font;

            lvEntries.Font = (Font)lastFont.Clone();
            lvEntries.SmallImageList = m_lvEntries.SmallImageList;

            _font = lvEntries.Font;
            _currentRowHeight = RowHeight.Default;

            if (_config.CredPickerLargeRows)
                SetRowHeight(RowHeight.Large);

            // Keep font in sync with KeePass main form.
            // Keep image list in sync with KeePass main form.
            host.MainWindow.UIStateUpdated += (s, e) =>
            {
                if (m_lvEntries.Font != lastFont)
                {
                    var oldFont = lvEntries.Font;
                    lvEntries.Font = (Font)(lastFont = m_lvEntries.Font).Clone();

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
            };

            ResumeLayout(false);
        }

        public new void Dispose()
        {
            if (lvEntries.Font != _font)
                _font.Dispose();
            lvEntries.SmallImageList = null;
            base.Dispose();
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
            ConnPE = ReturnPE = null;
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
            lvEntries.Invalidate();
        }

        private void KprCredentialPickerForm_Load(object sender, EventArgs e)
        {
            // Set configured window size.
            Width = _config.CredPickerWidth;
            Height = _config.CredPickerHeight;

            UseWaitCursor = true;
            SuspendLayout();

            // Add path of current proccessed group to window title.
            Text += (Util.InRdpSubgroup(ConnPE) ? ConnPE.ParentGroup.ParentGroup : ConnPE.ParentGroup).GetFullPath(Util.GroupSeperator, false);

            GlobalWindowManager.AddWindow(this);

            if (UIUtil.VistaStyleListsSupported)
                UIUtil.SetExplorerTheme(lvEntries.Handle);
            UIUtil.SetFocus(lvEntries, this);

            lvEntries.SuspendLayout();
            lvEntries.ShowGroups = _config.CredPickerShowInGroups;
            lvEntries.ListViewItemSorter = _config.CredPickerSortOrder;
            lvEntries.ResumeLayout(false);

            CenterToParent();
            LoadListEntries();

            MessageFilter.ListViewGroupHeaderClickHandler.Enable(true);
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

            try
            {
                ReturnPE = RdpAccountEntries.First(account =>
                {
                    return account.Uuid == (PwUuid)lvEntries.SelectedItems[0].Tag;
                });
            }
            catch
            {
                VistaTaskDialog.ShowMessageBoxEx(
                    KprResourceManager.Instance["Please select a credential from the list."],
                    null,
                    Util.KeePassRDP,
                    VtdIcon.Information,
                    this,
                    null, 0, null, 0);
                return false;
            }

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

            lvEntries.Sort();
            UIUtil.SetSortIcon(lvEntries, lvEntries.Columns[listSorter.Column].Index, listSorter.SortOrder);
        }

        private void lvEntries_KeyDown(object sender, KeyEventArgs e)
        {
            if (lvEntries.UseWaitCursor)
                return;

            var lvi = lvEntries.FocusedItem ?? lvEntries.SelectedItems[0];

            if (lvi == null)
                return;

            int nextIndex;

            switch (e.KeyCode)
            {
                case Keys.Down:
                    nextIndex = lvi.Index + 1;
                    if (nextIndex >= lvEntries.Items.Count)
                        nextIndex = 0;
                    lvi.Focused = lvi.Selected = false;
                    lvi = lvEntries.Items.Cast<ListViewItem>().Single(x => x.Index == nextIndex);
                    lvi.Focused = lvi.Selected = true;
                    lvEntries.EnsureVisible(nextIndex);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
                case Keys.Up:
                    nextIndex = lvi.Index - 1;
                    if (nextIndex < 0)
                        nextIndex = lvEntries.Items.Count - 1;
                    lvi.Focused = lvi.Selected = false;
                    lvi = lvEntries.Items.Cast<ListViewItem>().Single(x => x.Index == nextIndex);
                    lvi.Focused = lvi.Selected = true;
                    lvEntries.EnsureVisible(nextIndex);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
            }
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

            e.DrawBackground();

            TextRenderer.DrawText(
                e.Graphics,
                e.Header.Text, _font ?? SystemFonts.DefaultFont,
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

                        var entryUsername = new string(account.Strings.GetSafe(PwDefs.UserNameField).ReadChars());
                        username = SprEngine.Compile(entryUsername, ctx);
                        if (username != entryUsername)
                            MemoryUtil.SecureZeroMemory(entryUsername);

                        notes = SprEngine.Compile(account.Strings.ReadSafe(PwDefs.NotesField), ctx);
                    }
                    else
                    {
                        title = account.Strings.ReadSafe(PwDefs.TitleField);
                        username = new string(account.Strings.GetSafe(PwDefs.UserNameField).ReadChars());
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
                    UIUtil.SetFocusedItem(lvEntries, lvEntries.Items[0], true);
                    lvEntries.Select();
                }

                lvEntries.EndUpdate();
                lvEntries.UseWaitCursor = false;
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

                lvEntries.SuspendLayout();

                var oldFont = lvEntries.Font;
                if (_currentRowHeight == RowHeight.Large)
                    lvEntries.Font = _font;

                if (lvEntries.Items.Count > 0)
                    lvEntries.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                else
                    lvEntries.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                lvEntries.Columns[lvEntries.Columns.Count - 1].Width = -2;

                if (_currentRowHeight == RowHeight.Large)
                    lvEntries.Font = oldFont;

                lvEntries.ResumeLayout(false);
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