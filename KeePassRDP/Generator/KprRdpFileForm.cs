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
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP.Generator
{
    public partial class KprRdpFileForm : Form
    {
        private readonly Dictionary<string, Control> _cachedControls;
        private readonly bool _disposeRdpFile;
        private readonly RdpFile _rdpFile;

        public RdpFile RdpFile { get { return _rdpFile; } }
        public bool IsReadOnly { get; set; }

        public KprRdpFileForm(RdpFile rdpFile = null)
        {
            _cachedControls = new Dictionary<string, Control>();

            InitializeComponent();

            SuspendLayout();

            KprResourceManager.Instance.TranslateMany(
                this,
                cmdImport,
                cmdExport,
                cmdOk,
                cmdCancel
            );

            Util.SetDoubleBuffered(tblRdpFileForm);
            Util.SetDoubleBuffered(tcRdpFileSettings);

            Icon = IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, UIUtil.GetIconSize().Height);

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
                    Util.KeePassRDP + " - " + KPRes.FatalError,
                    VtdIcon.Error,
                    this,
                    null, 0, null, 0);
                return;
            }

            var tempCache = RdpFile.SettingsCache.Values.SelectMany(x => x).ToDictionary(x => x.Item2.Template.Split(new[] { ':' }, 2).FirstOrDefault(), x => x, StringComparer.OrdinalIgnoreCase);
            foreach (var line in File.ReadAllLines(fileName))
            {
                var template = line.Split(new[] { ':' }, 3);

                Tuple<PropertyInfo, RdpSettingAttribute> prop;
                if (tempCache.TryGetValue(template[0], out prop))
                    _rdpFile.SetProperty(prop, template);
            }

            KprRdpFileForm_Load(null, null);
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
                        Util.KeePassRDP + " - " + KPRes.FatalError,
                        VtdIcon.Error,
                        this,
                        null, 0, null, 0);
                }
            }
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
            tblRdpFileForm.Visible = false;

            UseWaitCursor = true;
            SuspendLayout();

            if (backgroundWorker1.IsBusy && !backgroundWorker1.CancellationPending && backgroundWorker1.WorkerSupportsCancellation)
                backgroundWorker1.CancelAsync();
            backgroundWorker1.RunWorkerAsync(width);
        }

        private void KprRdpFileForm_ResizeBegin(object sender, EventArgs e)
        {
            foreach (TabPage page in tcRdpFileSettings.TabPages)
            {
                if (page == tcRdpFileSettings.SelectedTab)
                    continue;

                if (page.Controls.Count > 0)
                {
                    _cachedControls[page.Name] = page.Controls[0];
                    page.Controls.Clear();
                }

                page.SuspendLayout();
            }
        }

        private void KprRdpFileForm_ResizeEnd(object sender, EventArgs e)
        {
            Control control;
            foreach (TabPage page in tcRdpFileSettings.TabPages)
            {
                if (_cachedControls.TryGetValue(page.Name, out control))
                    if (!page.Controls.Contains(control))
                        page.Controls.Add(control);
                if (page != tcRdpFileSettings.SelectedTab)
                    page.ResumeLayout(false);
            }
        }

        private void tcRdpFileSettings_Selected(object sender, TabControlEventArgs e)
        {
            foreach (TabPage page in tcRdpFileSettings.TabPages)
                if (page != tcRdpFileSettings.SelectedTab)
                    page.SuspendLayout();
            if (e.TabPage != null && e.TabPage == tcRdpFileSettings.SelectedTab && tcRdpFileSettings.TabPages.Contains(e.TabPage))
                e.TabPage.ResumeLayout(true);
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var font = new Font(Font.Name, 7.25f, Font.Style, Font.Unit);

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
                        Margin = new Padding(0)
                    };
                    Util.SetDoubleBuffered(tabPage);
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
                        Margin = new Padding(0),
                        Padding = new Padding(0, 2, 0, 1),
                        GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
                        ColumnCount = 1,
                        RowCount = g.Value.Count
                    };
                    Util.SetDoubleBuffered(tblTabPage);
                }

                tabPage.SuspendLayout();
                tabPage.Controls.Clear();
                tblTabPage.SuspendLayout();
                tblTabPage.Controls.Clear();

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
                            Margin = new Padding(0),
                            AutoSize = true,
                            AutoSizeMode = AutoSizeMode.GrowAndShrink,
                            Enabled = notIsReadOnly
                        };
                        tblTabPage.RowStyles.Add(new RowStyle(SizeType.Absolute, DpiUtil.ScaleIntY(22)));
                        tblTabPage.SetRow(panel, i);
                    }

                    var label = panel.Controls.Count > 0 ? panel.Controls[0] : new Label
                    {
                        Text = name,
                        TextAlign = ContentAlignment.MiddleRight,
                        Padding = new Padding(0),
                        Margin = new Padding(0, 3, 4, 0),
                        Anchor = AnchorStyles.Left,
                        BorderStyle = BorderStyle.None,
                        FlatStyle = FlatStyle.System,
                        Width = width - 10,
                        Height = DpiUtil.ScaleIntY(18)
                    };

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
                                Font = font,
                                Margin = new Padding(0),
                                Anchor = AnchorStyles.Left,
                                AutoCompleteMode = AutoCompleteMode.None,
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                Sorted = false,
                                Width = width - 15,
                                FlatStyle = FlatStyle.System
                            };
                            tempBox.Items.AddRange(Enum.GetNames(property.PropertyType));
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
                                Font = font,
                                Padding = new Padding(0),
                                Margin = new Padding(0),
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
                                Font = font,
                                Margin = new Padding(0),
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
                                Font = font,
                                Margin = new Padding(0),
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

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (UseWaitCursor)
            {
                tblRdpFileForm.Visible = true;

                ResumeLayout(false);
                UseWaitCursor = false;
            }
        }

        new public void Dispose()
        {
            _cachedControls.Clear();

            Icon.Dispose();

            base.Dispose();

            if (_disposeRdpFile && _rdpFile != null)
                _rdpFile.Dispose();
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
                        newValue = tbox.Text;
                    else if (property.PropertyType == typeof(int))
                        newValue = int.Parse(tbox.Text);
                }
                if (newValue == null)
                    property.SetValue(rdpFile, null, null);
                else /*if (newValue != oldValue)*/
                    property.SetValue(rdpFile, Convert.ChangeType(newValue, property.PropertyType), null);
            }
        }
    }
}
