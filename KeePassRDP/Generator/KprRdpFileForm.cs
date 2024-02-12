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
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP.Generator
{
    public partial class KprRdpFileForm : Form
    {
        private static readonly Lazy<Icon> _icon = new Lazy<Icon>(
            () => IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, UIUtil.GetIconSize().Height),
            LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly bool _disposeRdpFile;
        private readonly RdpFile _rdpFile;

        public RdpFile RdpFile { get { return _rdpFile; } }
        public bool IsReadOnly { get; set; }

        public KprRdpFileForm(RdpFile rdpFile = null)
        {
            InitializeComponent();

            SuspendLayout();

            KprResourceManager.Instance.TranslateMany(
                cmdImport,
                cmdOk,
                cmdCancel
            );

            Util.SetDoubleBuffered(tblRdpFileForm);
            Util.SetDoubleBuffered(flpRdpFileSettings);

            Icon = _icon.Value;

            _disposeRdpFile = rdpFile == null;
            _rdpFile = rdpFile ?? new RdpFile();

            ResumeLayout(false);
        }

        public void cmdImport_Click(object sender, EventArgs e)
        {
            var ofdRdp = UIUtil.CreateOpenFileDialog(
                Text,
                UIUtil.CreateFileTypeFilter("rdp", KprResourceManager.Instance["Remotedesktop (RDP) Files"], true),
                1,
                null,
                false,
                Util.KeePassRDP);

            GlobalWindowManager.AddDialog(ofdRdp.FileDialog);
            var dr = ofdRdp.ShowDialog();
            GlobalWindowManager.RemoveDialog(ofdRdp.FileDialog);

            if (dr != DialogResult.OK)
                return;

            var fileName = ofdRdp.FileName;

            if (!File.Exists(fileName))
            {
                VistaTaskDialog.ShowMessageBoxEx(
                    string.Format(KprResourceManager.Instance["File '{0}' not found."], fileName),
                    null,
                    Util.KeePassRDP + " - " + KPRes.FatalError,
                    VtdIcon.Error,
                    null, null, 0, null, 0);
                return;
            }

            var tempCache = RdpFile.AttributeCache.ToDictionary(x => x.Item2.Template.Split(new[] { ':' }, 2).FirstOrDefault(), x => x, StringComparer.OrdinalIgnoreCase);
            foreach (var line in File.ReadAllLines(fileName))
            {
                var template = line.Split(new[] { ':' });
                if (template.Length != 3)
                    continue;

                Tuple<PropertyInfo, RdpSettingAttribute> prop;
                if (tempCache.TryGetValue(template[0], out prop))
                {
                    var type = template[1];

                    object defaultValue = null;
                    switch (type)
                    {
                        case "s":
                            defaultValue = prop.Item1.PropertyType.IsEnum ?
                                Enum.Parse(prop.Item1.PropertyType, template.Length > 2 ? template[2] : string.Empty) :
                                template.Length > 2 ? template[2] : string.Empty;
                            break;
                        case "i":
                            defaultValue = prop.Item1.PropertyType.IsEnum ?
                                Enum.ToObject(prop.Item1.PropertyType, template.Length > 2 ? int.Parse(template[2]) : 0) :
                                template.Length > 2 ? int.Parse(template[2]) : 0;
                            break;
                    }

                    if (defaultValue != null)
                        prop.Item1.SetValue(_rdpFile, Convert.ChangeType(defaultValue, prop.Item1.PropertyType), null);
                }
            }

            KprRdpFileForm_Load(null, null);
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

            var tempCache = RdpFile.AttributeCache.ToDictionary(x => x.Item2.Template.Split(new[] { ':' }, 2).FirstOrDefault(), x => x, StringComparer.OrdinalIgnoreCase);
            foreach (FlowLayoutPanel control in flpRdpFileSettings.Controls)
            {
                var label = control.Controls[0] as Label;
                var prop = tempCache[label.Text];

                var box = control.Controls[1];
                //var oldValue = prop.Item1.GetValue(_rdpFile, null);
                object newValue = null;
                if (box is ComboBox)
                {
                    var cbox = box as ComboBox;
                    var item = cbox.SelectedItem as string;
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        if (prop.Item1.PropertyType.IsEnum)
                            newValue = Enum.Parse(prop.Item1.PropertyType, item, true);
                    }
                }
                else if (box is CheckBox)
                {
                    var cbox = box as CheckBox;
                    if (prop.Item1.PropertyType == typeof(bool))
                        newValue = cbox.Checked;
                    else if (prop.Item1.PropertyType == typeof(int))
                        newValue = cbox.Checked ? 1 : 0;
                }
                else if (box is TextBox)
                {
                    var tbox = box as TextBox;
                    if (prop.Item1.PropertyType == typeof(string))
                        newValue = tbox.Text;
                    else if (prop.Item1.PropertyType == typeof(int))
                        newValue = int.Parse(tbox.Text);
                }
                if (newValue == null)
                    prop.Item1.SetValue(_rdpFile, null, null);
                else /*if (newValue != oldValue)*/
                    prop.Item1.SetValue(_rdpFile, Convert.ChangeType(newValue, prop.Item1.PropertyType), null);
            }
        }

        public void KprRdpFileForm_Load(object sender, EventArgs e)
        {
            var notIsReadOnly = !IsReadOnly;
            cmdOk.Enabled = cmdImport.Enabled = notIsReadOnly;

            flpRdpFileSettings.UseWaitCursor = true;

            var invoke = BeginInvoke(new Func<Control[]>(() =>
            {
                var width = flpRdpFileSettings.Width / 2;
                var font = new Font(Font.Name, 7.25f);

                return RdpFile.AttributeCache
                    .Select((x, i) =>
                    {
                        var property = x.Item1;
                        var attribute = x.Item2;

                        var template = attribute.Template.Split(new[] { ':' }, 3);
                        var type = template[1];
                        var propValue = property.GetValue(_rdpFile, null);

                        object defaultValue = null;
                        switch (type)
                        {
                            case "s":
                                defaultValue = property.PropertyType.IsEnum ?
                                    Enum.Parse(property.PropertyType, template.Length > 2 ? template[2] : string.Empty) :
                                    template.Length > 2 ? template[2] : string.Empty;
                                break;
                            case "i":
                                defaultValue = property.PropertyType.IsEnum ?
                                    Enum.ToObject(property.PropertyType, template.Length > 2 ? int.Parse(template[2]) : 0) :
                                    template.Length > 2 ? int.Parse(template[2]) : 0;
                                break;
                        }
                        if (propValue != null && propValue != Convert.ChangeType(defaultValue, property.PropertyType))
                            defaultValue = propValue;

                        var panel = flpRdpFileSettings.Controls.Count > i ? flpRdpFileSettings.Controls[i] : new FlowLayoutPanel
                        {
                            FlowDirection = FlowDirection.LeftToRight,
                            Anchor = AnchorStyles.Left | AnchorStyles.Right,
                            Margin = new Padding(0),
                            AutoSize = true,
                            AutoSizeMode = AutoSizeMode.GrowAndShrink,
                            Enabled = notIsReadOnly
                        };

                        var label = panel.Controls.Count > 0 ? panel.Controls[0] : new Label
                        {
                            Text = template[0],
                            TextAlign = ContentAlignment.MiddleRight,
                            Margin = new Padding(0),
                            Anchor = AnchorStyles.Left,
                            Width = width - 10
                        };

                        Control box;
                        if (property.PropertyType.IsEnum)
                        {
                            var tempBox = new ComboBox
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
                            if (defaultValue != null)
                                tempBox.SelectedIndex = Math.Max(0, tempBox.Items.IndexOf(defaultValue.ToString()));
                            box = tempBox;
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            box = new CheckBox
                            {
                                Checked = (defaultValue is int && (int)defaultValue == 1) || (defaultValue is bool && (bool)defaultValue == true),
                                Font = font,
                                Margin = new Padding(0, 1, 0, 0),
                                Anchor = AnchorStyles.Left,
                                AutoSize = true
                            };
                        }
                        else
                        {
                            box = new TextBox
                            {
                                Text = defaultValue != null ? defaultValue.ToString() : string.Empty,
                                Font = font,
                                Margin = new Padding(0),
                                Anchor = AnchorStyles.Left,
                                Width = width - 15,
                                BorderStyle = BorderStyle.FixedSingle
                            };
                        }

                        panel.SuspendLayout();
                        panel.Controls.Clear();
                        panel.Controls.AddRange(new Control[] { label, box });
                        panel.ResumeLayout(false);

                        return panel;
                    }).ToArray();
            }));

            if (!invoke.IsCompleted)
                Task.Factory.FromAsync(
                    invoke,
                    endinvoke =>
                    {
                        var controls = EndInvoke(endinvoke) as Control[];
                        if (controls != null)
                        {
                            Invoke(new Action(() =>
                            {
                                flpRdpFileSettings.SuspendLayout();
                                flpRdpFileSettings.Controls.Clear();
                                flpRdpFileSettings.Controls.AddRange(controls);
                                flpRdpFileSettings.ResumeLayout(true);
                                flpRdpFileSettings.UseWaitCursor = false;
                            }));
                        }
                        else
                        {
                            Invoke(new Action(() =>
                            {
                                flpRdpFileSettings.UseWaitCursor = false;
                            }));
                        }
                    },
                    TaskCreationOptions.AttachedToParent,
                    TaskScheduler.Default);
            else
            {
                var controls = EndInvoke(invoke) as Control[];
                if (controls != null)
                {
                    flpRdpFileSettings.SuspendLayout();
                    flpRdpFileSettings.Controls.Clear();
                    flpRdpFileSettings.Controls.AddRange(controls);
                    flpRdpFileSettings.ResumeLayout(false);
                }
                flpRdpFileSettings.UseWaitCursor = false;
            }
        }

        new public void Dispose()
        {
            base.Dispose();
            if (_disposeRdpFile && _rdpFile != null)
                _rdpFile.Dispose();
        }
    }
}
