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
using KeePassRDP.Extensions;
using KeePassRDP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP.Generator
{
    public partial class KprRdpFileForm : Form, IGwmWindow
    {
        public bool CanCloseWithoutDataLoss { get { return true; } }
        public RdpFile RdpFile { get { return _rdpFile; } }
        public bool IsReadOnly { get; set; }

        private readonly Dictionary<string, Control> _cachedControls;
        private readonly bool _disposeRdpFile;
        private readonly RdpFile _rdpFile;
        private readonly Font _font;

        public KprRdpFileForm(RdpFile rdpFile = null)
        {
            _cachedControls = new Dictionary<string, Control>();

            InitializeComponent();

            SuspendLayout();

            var tempFont = Font;
            _font = new Font(tempFont.FontFamily, 7.25f, tempFont.Style, tempFont.Unit);

            KprResourceManager.Instance.TranslateMany(
                this,
                cmdImport,
                cmdExport,
                cmdOk,
                cmdCancel
            );

            DoubleBuffered = true;

            Util.EnableDoubleBuffered(
                tblRdpFileForm,
                tcRdpFileSettings
            );

            Icon = IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, UIUtil.GetSmallIconSize().Height);

            _disposeRdpFile = rdpFile == null;
            _rdpFile = rdpFile ?? new RdpFile();

            ResumeLayout(false);
        }

        public void cmdImport_Click(object sender, EventArgs e)
        {
            var ofdRdp = UIUtil.CreateOpenFileDialog(
                string.Format("{0} {1}", Text, KprResourceManager.Instance["import"]),
                UIUtil.CreateFileTypeFilter("rdp", KprResourceManager.Instance["Remotedesktop (RDP) Files"], true),
                1,
                null,
                false,
                Util.KeePassRDP);

            var fileName = string.Empty;

            using (ofdRdp.FileDialog)
            {
                GlobalWindowManager.AddDialog(ofdRdp.FileDialog);
                var dr = ofdRdp.ShowDialog();
                GlobalWindowManager.RemoveDialog(ofdRdp.FileDialog);

                if (dr != DialogResult.OK)
                    return;

                fileName = ofdRdp.FileName;
            }

            if (string.IsNullOrWhiteSpace(fileName))
                return;

            if (!File.Exists(fileName))
            {
                VistaTaskDialog.ShowMessageBoxEx(
                    string.Format(KprResourceManager.Instance["File '{0}' not found."], fileName),
                    null,
                    string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                    VtdIcon.Error,
                    this,
                    null, 0, null, 0);
                return;
            }

            _rdpFile.Reset();

            var tempCache = RdpFile.SettingsCache.Values.SelectMany(x => x).ToDictionary(x => x.Item2.Template.Split(new[] { ':' }, 2).FirstOrDefault(), x => x, StringComparer.OrdinalIgnoreCase);
            foreach (var line in File.ReadAllLines(fileName))
            {
                var template = line.Split(new[] { ':' }, 3);

                Tuple<PropertyInfo, RdpSettingAttribute> prop;
                if (tempCache.TryGetValue(template[0], out prop))
                    _rdpFile.SetProperty(prop, template);
            }

            KprRdpFileForm_Load(null, EventArgs.Empty);
        }

        public void cmdExport_Click(object sender, EventArgs e)
        {
            var sfdRdp = UIUtil.CreateSaveFileDialog(
            string.Format("{0} {1}", Text, KprResourceManager.Instance["export"]),
                "Default.rdp",
                UIUtil.CreateFileTypeFilter("rdp", KprResourceManager.Instance["Remotedesktop (RDP) Files"], true),
                1,
                null,
                Util.KeePassRDP);

            var fileName = string.Empty;

            using (sfdRdp.FileDialog)
            {
                GlobalWindowManager.AddDialog(sfdRdp.FileDialog);
                var dr = sfdRdp.ShowDialog();
                GlobalWindowManager.RemoveDialog(sfdRdp.FileDialog);

                if (dr != DialogResult.OK)
                    return;

                fileName = sfdRdp.FileName;
            }

            if (string.IsNullOrWhiteSpace(fileName))
                return;

            /*if (File.Exists(fileName))
            {
                if (VistaTaskDialog.ShowMessageBoxEx(
                    string.Format(KprResourceManager.Instance["File already exists. Overwrite '{0}'?"], fileName),
                    null,
                    Util.KeePassRDP + " - " + KPRes.OverwriteExistingFileQuestion,
                    VtdIcon.Warning,
                    null, KPRes.Yes, 1, KPRes.No, 0) != 1)
                    return;
            }*/

            using (var rdpFile = new RdpFile(_rdpFile, true))
            {
                UpdateRdpFile(rdpFile);

                try
                {
                    File.Move(rdpFile.ToString(), fileName);
                }
                catch (Exception ex)
                {
                    VistaTaskDialog.ShowMessageBoxEx(
                        string.Format(KprResourceManager.Instance["Saving '{0}' failed: {1}"], fileName, ex.Message),
                        null,
                        string.Format("{0} - {1}", Util.KeePassRDP, KPRes.Error),
                        VtdIcon.Error,
                        this,
                        null, 0, null, 0);
                }
            }
        }

        private void cmdReset_Click(object sender, EventArgs e)
        {
            _rdpFile.Reset();
            KprRdpFileForm_Load(null, EventArgs.Empty);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cmdOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void KprRdpFileForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel)
                return;

            if (DialogResult != DialogResult.OK)
                return;

            UpdateRdpFile(_rdpFile);
        }

        public void KprRdpFileForm_Load(object sender, EventArgs e)
        {
            var width = (tblRdpFileForm.Width - SystemInformation.VerticalScrollBarWidth - 5) / 2;
            tcRdpFileSettings.Hide();

            UseWaitCursor = true;
            SuspendLayout();

            if (backgroundWorker1.IsBusy && backgroundWorker1.WorkerSupportsCancellation && !backgroundWorker1.CancellationPending)
                backgroundWorker1.CancelAsync();
            backgroundWorker1.RunWorkerAsync(width);
        }

        private volatile bool _isResizing = false;
        private volatile bool _isMoving = false;

        private void KprRdpFileForm_ResizeBegin(object sender, EventArgs e)
        {
            if (_isResizing)
                return;

            _isResizing = true;

            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
            {
                if (!_isResizing)
                    return;

                var selectedTab = tcRdpFileSettings.SelectedTab;
                foreach (TabPage page in tcRdpFileSettings.TabPages)
                {
                    if (page == selectedTab)
                        continue;

                    if (page.Controls.Count > 0)
                    {
                        _cachedControls[page.Name] = page.Controls[0];
                        page.Controls.Clear();
                    }

                    page.SuspendLayout();
                }
            })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void KprRdpFileForm_ResizeEnd(object sender, EventArgs e)
        {
            if (!_isResizing)
                return;

            _isResizing = false;

            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
            {
                if (_isResizing)
                    return;

                Control control;
                var selectedTab = tcRdpFileSettings.SelectedTab;
                foreach (TabPage page in tcRdpFileSettings.TabPages)
                {
                    if (_cachedControls.TryGetValue(page.Name, out control))
                        if (!page.Controls.Contains(control))
                            page.Controls.Add(control);
                    if (page != selectedTab)
                        page.ResumeLayout(false);
                }
            })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void tcRdpFileSettings_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage != null)
                e.TabPage.SuspendLayout();
            Task.Factory.FromAsync(BeginInvoke(new Action(() =>
            {
                var selectedTab = tcRdpFileSettings.SelectedTab;
                foreach (TabPage page in tcRdpFileSettings.TabPages)
                {
                    if (!page.Created)
                        page.CreateControl();
                    if (page != selectedTab)
                        page.SuspendLayout();
                }
                if (e.TabPage != null && e.TabPage == selectedTab && tcRdpFileSettings.TabPages.Contains(e.TabPage))
                    e.TabPage.ResumeLayout(true);
            })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var notIsReadOnly = !IsReadOnly;
            cmdOk.Enabled = cmdImport.Enabled = notIsReadOnly;

            var tabPages = tcRdpFileSettings.TabPages.OfType<TabPage>().ToArray();

            tcRdpFileSettings.SuspendLayout();
            tcRdpFileSettings.TabPages.Clear();

            var width = (int)e.Argument;

            var pages = RdpFile.SettingsCache.Select(g => BeginInvoke(new Func<TabPage>(() =>
            {
                var tabPage = tabPages.FirstOrDefault(x => x.Name == g.Key);
                if (tabPage == null)
                {
                    tabPage = new TabPage(KprResourceManager.Instance[g.Key])
                    {
                        Name = g.Key,
                        BackColor = Color.White,
                        Margin = Padding.Empty
                    };
                    tabPage.SetDoubleBuffered();
                }

                var tblTabPage = tabPage.Controls.Count > 0 ? tabPage.Controls[0] as TableLayoutPanel : null;
                if (tblTabPage == null)
                {
                    tblTabPage = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        AutoScroll = true,
                        Margin = Padding.Empty,
                        Padding = new Padding(0, 2, 0, 1),
                        GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
                        ColumnCount = 1,
                        RowCount = g.Value.Count
                    };
                    tblTabPage.SetDoubleBuffered();
                }

                tabPage.SuspendLayout();
                tabPage.Controls.Clear();
                tblTabPage.SuspendLayout();
                //tblTabPage.Controls.Clear();

                var controls = g.Value.Select((x, i) =>
                {
                    var property = x.Item1;
                    var attribute = x.Item2;

                    var template = attribute.Template.Split(new[] { ':' }, 3);
                    var name = template[0];
                    var type = template[1];
                    var propValue = property.GetValue(_rdpFile, null);

                    object defaultValue = null;
                    switch (type)
                    {
                        case "s":
                            defaultValue = property.PropertyType.IsEnum ?
                                template.Length > 2 ?
                                    !string.IsNullOrEmpty(template[2]) ?
                                        Enum.Parse(property.PropertyType, template[2]) :
                                        Enum.ToObject(property.PropertyType, 0) :
                                    string.Empty :
                                template.Length > 2 && !string.IsNullOrEmpty(template[2]) ? template[2] : string.Empty;
                            break;
                        case "i":
                            defaultValue = property.PropertyType.IsEnum ?
                                template.Length > 2 ?
                                    Enum.ToObject(property.PropertyType, !string.IsNullOrEmpty(template[2]) ? int.Parse(template[2]) : 0) :
                                    Enum.ToObject(property.PropertyType, 0) :
                                template.Length > 2 && !string.IsNullOrEmpty(template[2]) ? int.Parse(template[2]) : 0;
                            break;

                    }
                    if (propValue != null && propValue != Convert.ChangeType(defaultValue, property.PropertyType))
                        defaultValue = propValue;

                    var panel = tblTabPage.Controls.Count > i ? tblTabPage.Controls[i] : null;
                    if (panel == null)
                    {
                        panel = new FlowLayoutPanel
                        {
                            FlowDirection = FlowDirection.LeftToRight,
                            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                            Margin = Padding.Empty,
                            AutoSize = true,
                            AutoSizeMode = AutoSizeMode.GrowAndShrink,
                            Enabled = notIsReadOnly
                        };
                        panel.SetDoubleBuffered();
                        tblTabPage.RowStyles.Add(new RowStyle(SizeType.Absolute, DpiUtil.ScaleIntY(22)));
                        tblTabPage.SetRow(panel, i);
                    }

                    var label = panel.Controls.Count > 0 ? panel.Controls[0] : null;
                    if (label == null)
                    {
                        label = new Label
                        {
                            Text = name,
                            TextAlign = ContentAlignment.MiddleRight,
                            Padding = Padding.Empty,
                            Margin = new Padding(0, 3, 4, 0),
                            Anchor = AnchorStyles.Left,
                            BorderStyle = BorderStyle.None,
                            FlatStyle = FlatStyle.System,
                            Width = width - 10,
                            Height = DpiUtil.ScaleIntY(18)
                        };
                        var defaultValueAttribute = property.GetCustomAttributes(typeof(DefaultValueAttribute), false).OfType<DefaultValueAttribute>().FirstOrDefault();
                        var defaultValueString = defaultValueAttribute != null && defaultValueAttribute.Value != null ? defaultValueAttribute.Value.ToString() : string.Empty;
                        if (string.IsNullOrEmpty(defaultValueString))
                            defaultValueString = KprResourceManager.Instance["unset"];
                        ttGeneral.SetToolTip(label, string.Format(KprResourceManager.Instance["Default value: {0}"], defaultValueString));
                    }

                    var box = panel.Controls.Count > 1 ? panel.Controls[1] : null;

                    panel.SuspendLayout();
                    panel.Controls.Clear();

                    if (property.PropertyType.IsEnum)
                    {
                        var tempBox = box as ComboBox;
                        if (tempBox == null)
                        {
                            box = tempBox = new ComboBox
                            {
                                Font = _font,
                                Margin = Padding.Empty,
                                Anchor = AnchorStyles.Left,
                                AutoCompleteMode = AutoCompleteMode.None,
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                Sorted = false,
                                Width = width - 15,
                                FlatStyle = FlatStyle.System
                            };
                            tempBox.BeginUpdate();
                            tempBox.Items.AddRange(Enum.GetNames(property.PropertyType));
                            tempBox.EndUpdate();
                            label.Margin = new Padding(0, 4, 4, 0);
                        }
                        if (defaultValue != null)
                            tempBox.SelectedIndex = Math.Max(0, tempBox.Items.IndexOf(defaultValue.ToString()));
                        else
                            tempBox.SelectedIndex = 0;
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        var tempBox = box as CheckBox;
                        if (tempBox == null)
                        {
                            box = tempBox = new CheckBox
                            {
                                Font = _font,
                                Padding = Padding.Empty,
                                Margin = Padding.Empty,
                                Anchor = AnchorStyles.Left,
                                AutoSize = true
                            };
                            tblTabPage.RowStyles[i].Height = DpiUtil.ScaleIntY(18);
                            label.Margin = new Padding(0, 2, 4, 0);
                            label.Height = DpiUtil.ScaleIntY(17);
                        }
                        tempBox.Checked = (defaultValue is int && (int)defaultValue == 1) || (defaultValue is bool && (bool)defaultValue == true);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        var tempBox = box as NumericUpDown;
                        if (tempBox == null)
                        {
                            box = tempBox = new NumericUpDown
                            {
                                Font = _font,
                                Margin = Padding.Empty,
                                Anchor = AnchorStyles.Left,
                                Width = width - 15,
                                Minimum = 0,
                                Maximum = int.MaxValue,
                                DecimalPlaces = 0,
                                Increment = 1,
                                ThousandsSeparator = false,
                                BorderStyle = BorderStyle.FixedSingle
                            };
                            if (name == "desktopheight" || name == "desktopwidth")
                            {
                                tempBox.Minimum = 200;
                                tempBox.Maximum = 8192;
                            }
                        }
                        tempBox.Value = defaultValue is int ? (int)defaultValue : 0;
                    }
                    else
                    {
                        var tempBox = box as TextBox;
                        if (tempBox == null)
                            box = tempBox = new TextBox
                            {
                                Font = _font,
                                Margin = Padding.Empty,
                                Anchor = AnchorStyles.Left,
                                Width = width - 15,
                                BorderStyle = BorderStyle.FixedSingle
                            };
                        tempBox.Text = defaultValue != null ? defaultValue.ToString() : string.Empty;
                    }

                    panel.Controls.AddRange(new Control[] { label, box });
                    panel.ResumeLayout(false);

                    return panel;
                }).ToArray();

                tblTabPage.Controls.AddRange(controls);
                tblTabPage.ResumeLayout(false);

                tabPage.Controls.Add(tblTabPage);
                tabPage.ResumeLayout(false);

                return tabPage;
            }))).ToArray();

            var invoke = BeginInvoke(new Action(() =>
            {
                if (pages.Length > 0)
                    tcRdpFileSettings.TabPages.AddRange(pages.Select(x => (TabPage)EndInvoke(x)).ToArray());
                tcRdpFileSettings.ResumeLayout(false);
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
                tcRdpFileSettings.Show();

                ResumeLayout(false);
                UseWaitCursor = false;
            }
        }

        new public void Dispose()
        {
            _cachedControls.Clear();

            base.Dispose();
            Icon.Dispose();

            if (_disposeRdpFile && _rdpFile != null)
                _rdpFile.Dispose();

            _font.Dispose();
        }

        private void UpdateRdpFile(RdpFile rdpFile)
        {
            var tempCache = RdpFile.SettingsCache.Values.SelectMany(x => x).ToDictionary(x => x.Item2.Template.Split(new[] { ':' }, 2).FirstOrDefault(), x => x, StringComparer.OrdinalIgnoreCase);
            foreach (var control in tcRdpFileSettings.TabPages.OfType<TabPage>().SelectMany(x => x.Controls[0].Controls.OfType<FlowLayoutPanel>()))
            {
                var label = control.Controls[0] as Label;
                var prop = tempCache[label.Text];
                var property = prop.Item1;

                var box = control.Controls[1];
                //var oldValue = prop.Item1.GetValue(rdpFile, null);
                object newValue = null;
                if (box is ComboBox)
                {
                    var cbox = box as ComboBox;
                    var item = cbox.SelectedItem as string;
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        if (property.PropertyType.IsEnum)
                            newValue = Enum.Parse(property.PropertyType, item, true);
                    }
                }
                else if (box is CheckBox)
                {
                    var cbox = box as CheckBox;
                    if (property.PropertyType == typeof(bool))
                        newValue = cbox.Checked;
                    else if (property.PropertyType == typeof(int))
                        newValue = cbox.Checked ? 1 : 0;
                }
                else if (box is NumericUpDown)
                {
                    var nbox = box as NumericUpDown;
                    if (property.PropertyType == typeof(int))
                        newValue = Convert.ToInt32(nbox.Value);
                    else
                        newValue = nbox.Value;
                }
                else if (box is TextBox)
                {
                    var tbox = box as TextBox;
                    if (property.PropertyType == typeof(string))
                        newValue = tbox.Text.Trim();
                    else if (property.PropertyType == typeof(int))
                        newValue = int.Parse(tbox.Text.Trim());
                }
                if (newValue == null)
                    property.SetValue(rdpFile, null, null);
                else /*if (newValue != oldValue)*/
                    property.SetValue(rdpFile, Convert.ChangeType(newValue, property.PropertyType), null);
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_MOVING = 0x0216;
            if (m.Msg == WM_MOVING)
            {
                _isMoving = true;

                var page = tcRdpFileSettings.SelectedTab;
                page.SuspendLayout();

                base.WndProc(ref m);

                _isMoving = false;

                Task.Factory.FromAsync(BeginInvoke(new Action(() =>
                {
                    if (_isMoving)
                        return;

                    page.ResumeLayout(false);
                })), endInvoke => EndInvoke(endInvoke), TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
            }
            else
                base.WndProc(ref m);
        }
    }
}
