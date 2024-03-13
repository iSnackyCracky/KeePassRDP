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

using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Utility;
using KeePassRDP.Generator;
using KeePassRDP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace KeePassRDP
{
    [ToolboxItem(false)]
    public partial class KprEntrySettingsTab : UserControl
    {
        public KprEntrySettings EntrySettings { get { return _pwEntrySettings; } }

        public bool IsReadOnly { get { return _pwEntrySettings.IsReadOnly; } }

        private readonly KprEntrySettings _pwEntrySettings;
        private readonly Timer _txtGroupUUIDsTooltipTimer;
        private readonly Lazy<Size> _txtGroupUUIDsCursorSize;
        private readonly BindingList<string> _cpGroupUUIDs;
        private readonly BindingList<string> _cpExcludedGroupUUIDs;
        private readonly BindingList<string> _cpRegExPatterns;
        private readonly BindingList<string> _mstscParameters;

        private Point? _lastTooltipMousePosition;

        public KprEntrySettingsTab(KprEntrySettings kprEntrySettings)
        {
            _pwEntrySettings = kprEntrySettings;

            _txtGroupUUIDsTooltipTimer = new Timer
            {
                Interval = 500,
                Enabled = false,
            };

            _txtGroupUUIDsCursorSize = new Lazy<Size>(() =>
            {
                Size size;// = Util.GetIconSize(Cursor, Cursor.Handle);
                if (CursorUtil.GetIconSize(out size))
                    return size;
                return Size.Empty;
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            _lastTooltipMousePosition = null;

            InitializeComponent();

            cmsMore.Items.AddRange(new ToolStripItem[] {
                new ToolStripControlHost(new CheckBox
                {
                    Name = "cbCpIncludeDefaultRegex",
                    Text = KprResourceManager.Instance["Include default RegEx"],
                })
                {
                    ToolTipText = KprResourceManager.Instance["Include RegEx from main KeePassRDP options."]
                },
                new ToolStripControlHost(new CheckBox
                {
                    Name = "cbIncludeDefaultParameters",
                    Text = KprResourceManager.Instance["Include default parameters"],
                })
                {
                    ToolTipText = KprResourceManager.Instance["Include mstsc.exe parameters from main KeePassRDP options."]
                },
                new ToolStripControlHost(new CheckBox
                {
                    Name = "cbForceLocalUser",
                    Text = KprResourceManager.Instance["Force local user"],
                })
                {
                    ToolTipText = KprResourceManager.Instance["Force authenticating as local user when username starts with target hostname,\r\nor when hostname is empty."]
                },
                new ToolStripControlHost(new CheckBox
                {
                    Name = "cbForceUpn",
                    Text = KprResourceManager.Instance["Force UPN"],
                })
                {
                    ToolTipText = KprResourceManager.Instance["Force rewriting username to UPN format when possible."]
                },
                new ToolStripControlHost(new CheckBox
                {
                    Name = "cbRetryOnce",
                    Text = KprResourceManager.Instance["Retry credentials once"],
                })
                {
                    ToolTipText = KprResourceManager.Instance["Try to reconnect with same credentials one time on connection losses.\r\nWarning: Will need to keep the password in (encrypted) memory."]
                },
                new ToolStripControlHost(new CheckBox
                {
                    Name = "cbInherit",
                    Text = KprResourceManager.Instance["Inheritance"],
                    ThreeState = true
                })
                {
                    ToolTipText = KprResourceManager.Instance["Define level of inheritance:\r\nUnchecked = Hide all settings from children\r\nChecked = Force all settings on children\r\nIndeterminate = Allow inheritance from parent settings (default)"]
                }
            });

            // Autosize after adding controls.
            cmsMore.Size = cmsMore.ClientSize = Size.Empty;

            foreach (var item in cmsMore.Items)
            {
                if (item is ToolStripControlHost)
                {
                    var tsch = item as ToolStripControlHost;
                    if (tsch.Control is CheckBox)
                        (tsch.Control as CheckBox).CheckedChanged += cb_CheckedChanged;
                }
            }

            KprResourceManager.Instance.TranslateMany(
                cbIgnore,
                cbUseCredpicker,
                cbCpRecurseGroups,
                btnRdpFile,
                btnMore,
                lblCpGroupUUIDs,
                lblCpExcludedGroupUUIDs,
                lblCpRegExPatterns,
                lblMstscParameters
            );

            cmsMore.Opening += (s, e) =>
            {
                var size = cmsMore.PreferredSize;
                cmsMore.MaximumSize = cmsMore.MinimumSize = new Size(size.Width - 18, size.Height);
            };

            cmsMore.Closed += (s, e) =>
            {
                cmsMore.MaximumSize = cmsMore.MinimumSize = Size.Empty;
            };

            lstCpGroupUUIDs.Tag = cmdCpGroupUUIDsRemove;
            txtCpGroupUUIDs.Tag = cmdCpGroupUUIDsRemove.Tag = cmdCpGroupUUIDsReset.Tag = lstCpGroupUUIDs;
            cmdCpGroupUUIDsAdd.Tag = txtCpGroupUUIDs;
            lstCpExcludedGroupUUIDs.Tag = cmdCpExcludedGroupUUIDsRemove;
            txtCpExcludedGroupUUIDs.Tag = cmdCpExcludedGroupUUIDsRemove.Tag = cmdCpExcludedGroupUUIDsReset.Tag = lstCpExcludedGroupUUIDs;
            cmdCpExcludedGroupUUIDsAdd.Tag = txtCpExcludedGroupUUIDs;
            lstCpRegExPatterns.Tag = cmdCpRegExPatternsRemove;
            txtCpRegExPatterns.Tag = cmdCpRegExPatternsRemove.Tag = cmdCpRegExPatternsReset.Tag = lstCpRegExPatterns;
            cmdCpRegExPatternsAdd.Tag = txtCpRegExPatterns;
            lstMstscParameters.Tag = cmdMstscParametersRemove;
            txtMstscParameters.Tag = cmdMstscParametersRemove.Tag = cmdMstscParametersReset.Tag = lstMstscParameters;
            cmdMstscParametersAdd.Tag = txtMstscParameters;

            Util.SetDoubleBuffered(tblKprEntrySettingsTab);
            Util.SetDoubleBuffered(lstCpGroupUUIDs);
            Util.SetDoubleBuffered(lstCpExcludedGroupUUIDs);
            Util.SetDoubleBuffered(lstCpRegExPatterns);
            Util.SetDoubleBuffered(lstMstscParameters);

            var cpGroupUUIDsList = _pwEntrySettings.CpGroupUUIDs.ToList();
            _cpGroupUUIDs = new BindingList<string>(cpGroupUUIDsList);
            if (!IsReadOnly)
                _cpGroupUUIDs.ListChanged += (s, e) =>
                {
                    switch (e.ListChangedType)
                    {
                        case ListChangedType.ItemAdded:
                            _pwEntrySettings.CpGroupUUIDs.Add(cpGroupUUIDsList[e.NewIndex]);
                            break;
                        case ListChangedType.ItemDeleted:
                            _pwEntrySettings.CpGroupUUIDs.Remove(_pwEntrySettings.CpGroupUUIDs.ElementAt(e.NewIndex));
                            break;
                    }
                    cmdCpGroupUUIDsReset.Enabled = cmdCpGroupUUIDsRemove.Enabled = _cpGroupUUIDs.Count > 0;
                };

            var cpExcludedGroupUUIDs = _pwEntrySettings.CpExcludedGroupUUIDs.ToList();
            _cpExcludedGroupUUIDs = new BindingList<string>(cpExcludedGroupUUIDs);
            if (!IsReadOnly)
                _cpExcludedGroupUUIDs.ListChanged += (s, e) =>
                {
                    switch (e.ListChangedType)
                    {
                        case ListChangedType.ItemAdded:
                            _pwEntrySettings.CpExcludedGroupUUIDs.Add(cpExcludedGroupUUIDs[e.NewIndex]);
                            break;
                        case ListChangedType.ItemDeleted:
                            _pwEntrySettings.CpExcludedGroupUUIDs.Remove(_pwEntrySettings.CpExcludedGroupUUIDs.ElementAt(e.NewIndex));
                            break;
                    }
                    cmdCpExcludedGroupUUIDsReset.Enabled = cmdCpExcludedGroupUUIDsRemove.Enabled = _cpExcludedGroupUUIDs.Count > 0;
                };

            var cpRegExPatterns = _pwEntrySettings.CpRegExPatterns.ToList();
            _cpRegExPatterns = new BindingList<string>(cpRegExPatterns);
            if (!IsReadOnly)
                _cpRegExPatterns.ListChanged += (s, e) =>
                {
                    switch (e.ListChangedType)
                    {
                        case ListChangedType.ItemAdded:
                            _pwEntrySettings.CpRegExPatterns.Add(cpRegExPatterns[e.NewIndex]);
                            break;
                        case ListChangedType.ItemDeleted:
                            _pwEntrySettings.CpRegExPatterns.Remove(_pwEntrySettings.CpRegExPatterns.ElementAt(e.NewIndex));
                            break;
                    }
                    cmdCpRegExPatternsReset.Enabled = cmdCpRegExPatternsRemove.Enabled = _cpRegExPatterns.Count > 0;
                };

            var mstscParameters = _pwEntrySettings.MstscParameters.ToList();
            _mstscParameters = new BindingList<string>(mstscParameters);
            if (!IsReadOnly)
                _mstscParameters.ListChanged += (s, e) =>
                {
                    switch (e.ListChangedType)
                    {
                        case ListChangedType.ItemAdded:
                            _pwEntrySettings.MstscParameters.Add(mstscParameters[e.NewIndex]);
                            break;
                        case ListChangedType.ItemDeleted:
                            _pwEntrySettings.MstscParameters.Remove(_pwEntrySettings.MstscParameters.ElementAt(e.NewIndex));
                            break;
                    }
                    cmdMstscParametersReset.Enabled = cmdMstscParametersRemove.Enabled = _mstscParameters.Count > 0;
                };

            if (!IsReadOnly)
            {
                var cms = new ContextMenuStrip();
                cms.Opening += (s, e) =>
                {
                    MessageFilter.ToolStripDropDownMouseWheelHandler.Enable(true);
                };
                cms.Closing += (s, e) =>
                {
                    MessageFilter.ToolStripDropDownMouseWheelHandler.Enable(false);
                };
                cms.Opened += (s, e) =>
                {
                    (s as ContextMenuStrip).SourceControl.Focus();
                };
                txtCpGroupUUIDs.ContextMenuStrip = txtCpExcludedGroupUUIDs.ContextMenuStrip = cms;
            }
        }

        new public void Dispose()
        {
            cmsMore.Items.Clear();
            if (txtCpGroupUUIDs.ContextMenuStrip != null)
                using (txtCpGroupUUIDs.ContextMenuStrip)
                    txtCpGroupUUIDs.ContextMenuStrip.Items.Clear();
            base.Dispose();
        }

        public void UpdateContextMenu(IPluginHost host)
        {
            if (IsReadOnly)
                return;

            var cms = txtCpGroupUUIDs.ContextMenuStrip;
            if (cms == null)
                return;

            EventHandler click = delegate (object s, EventArgs e)
            {
                var tsi = s as ToolStripItem;
                var owner = tsi.Owner as ContextMenuStrip;
                owner.SourceControl.Text = ((PwGroup)tsi.Tag).Uuid.ToHexString();
            };

            MouseEventHandler firstOpen = null;

            firstOpen = delegate (object s, MouseEventArgs e)
            {
                if (firstOpen == null)
                    return;

                if (firstOpen != null)
                {
                    txtCpGroupUUIDs.MouseDown -= firstOpen;
                    txtCpExcludedGroupUUIDs.MouseDown -= firstOpen;
                    firstOpen = null;
                }

                cms.Items.Clear();

                var pgRoot = host.Database.RootGroup;
                var iconCache = host.Database.CustomIcons.ToDictionary(x => x.Uuid, x => host.Database.CustomIcons.IndexOf(x));
                var recycleBinEnabled = host.Database.RecycleBinEnabled;
                var clientIcons = host.MainWindow.ClientIcons;
                int iconIndex;

                var items = new List<ToolStripMenuItem>();

                //var lAvailKeys = new List<char>(PwCharSet.LowerCase + PwCharSet.Digits);
                GroupHandler gh = delegate (PwGroup pg)
                {
                    if (recycleBinEnabled && pg.Uuid.Equals(host.Database.RecycleBinUuid))
                        return true;

                    var strName = StrUtil.EncodeMenuText(pg.Name);
                    //strName = StrUtil.AddAccelerator(strName, lAvailKeys);
                    strName = strName.PadLeft(((int)pg.GetDepth() * 4) + strName.Length);

                    var nIconID = pg.CustomIconUuid.Equals(PwUuid.Zero) ?
                        (int)pg.IconId :
                    (int)PwIcon.Count +
                        (iconCache.TryGetValue(pg.CustomIconUuid, out iconIndex) ? iconIndex : -1);

                    var tsmi = new ToolStripMenuItem(strName)
                    {
                        Tag = pg,
                        Image = clientIcons.Images[nIconID]
                    };
                    tsmi.Click += click;
                    items.Add(tsmi);

                    return true;
                };

                gh(pgRoot);
                pgRoot.TraverseTree(TraversalMethod.PreOrder, gh, null);

                cms.Items.AddRange(items.ToArray());
            };

            txtCpGroupUUIDs.MouseDown += firstOpen;
            txtCpExcludedGroupUUIDs.MouseDown += firstOpen;
        }

        private void KprEntrySettingsTab_Load(object sender, EventArgs e)
        {
            SuspendLayout();

            cbIgnore.Checked = _pwEntrySettings.Ignore;
            cbUseCredpicker.Checked = _pwEntrySettings.UseCredpicker;
            cbCpRecurseGroups.Checked = _pwEntrySettings.CpRecurseGroups;

            foreach (var item in cmsMore.Items)
            {
                if (item is ToolStripControlHost)
                {
                    var tsch = item as ToolStripControlHost;
                    if (tsch.Control is CheckBox)
                    {
                        var checkBox = tsch.Control as CheckBox;
                        var property = _pwEntrySettings.GetType().GetProperty(checkBox.Name.Substring(2), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                        if (property.PropertyType.IsEnum)
                            checkBox.CheckState = (CheckState)property.GetValue(_pwEntrySettings, null);
                        else
                            checkBox.Checked = (bool)property.GetValue(_pwEntrySettings, null);
                    }
                }
            }

            lstCpGroupUUIDs.DataSource = _cpGroupUUIDs;
            cmdCpGroupUUIDsReset.Enabled = cmdCpGroupUUIDsRemove.Enabled = _cpGroupUUIDs.Count > 0;
            lstCpExcludedGroupUUIDs.DataSource = _cpExcludedGroupUUIDs;
            cmdCpExcludedGroupUUIDsReset.Enabled = cmdCpExcludedGroupUUIDsRemove.Enabled = _cpExcludedGroupUUIDs.Count > 0;
            lstCpRegExPatterns.DataSource = _cpRegExPatterns;
            cmdCpRegExPatternsReset.Enabled = cmdCpRegExPatternsRemove.Enabled = _cpRegExPatterns.Count > 0;
            lstMstscParameters.DataSource = _mstscParameters;
            cmdMstscParametersReset.Enabled = cmdMstscParametersRemove.Enabled = _mstscParameters.Count > 0;

            if (IsReadOnly)
            {
                foreach (ToolStripItem item in cmsMore.Items)
                    item.Enabled = false;

                cbIgnore.Enabled =
                    cbUseCredpicker.Enabled =
                    cbCpRecurseGroups.Enabled =
                    txtCpGroupUUIDs.Enabled =
                    txtCpExcludedGroupUUIDs.Enabled =
                    txtCpRegExPatterns.Enabled =
                    txtMstscParameters.Enabled =
                    cmdCpGroupUUIDsAdd.Enabled =
                    cmdCpGroupUUIDsRemove.Enabled =
                    cmdCpGroupUUIDsReset.Enabled =
                    cmdCpExcludedGroupUUIDsAdd.Enabled =
                    cmdCpExcludedGroupUUIDsRemove.Enabled =
                    cmdCpExcludedGroupUUIDsReset.Enabled =
                    cmdCpRegExPatternsAdd.Enabled =
                    cmdCpRegExPatternsRemove.Enabled =
                    cmdCpRegExPatternsReset.Enabled =
                    cmdMstscParametersAdd.Enabled =
                    cmdMstscParametersRemove.Enabled =
                    cmdMstscParametersReset.Enabled = false;
            }

            (Parent.Parent as TabControl).Selecting += (s, ee) =>
            {
                if (ee.TabPage != Parent)
                    if (cmsMore.Visible)
                        cmsMore.Close();
            };

            ParentForm.Deactivate += (s, ee) =>
            {
                if (cmsMore.Visible && !cmsMore.RectangleToScreen(cmsMore.DisplayRectangle).Contains(Cursor.Position))
                    cmsMore.Close();
            };

            ParentForm.Move += (s, ee) =>
            {
                if (cmsMore.Visible)
                    cmsMore.Close();
            };

            ResumeLayout(false);
        }

        private void cbIgnore_CheckedChanged(object sender, EventArgs e)
        {
            if (IsReadOnly)
                return;

            _pwEntrySettings.Ignore = cbIgnore.Checked;
        }

        private void cbUseCredpicker_CheckedChanged(object sender, EventArgs e)
        {
            if (IsReadOnly)
                return;

            _pwEntrySettings.UseCredpicker = cbUseCredpicker.Checked;
        }

        private void cbCpRecurseGroups_CheckedChanged(object sender, EventArgs e)
        {
            if (IsReadOnly)
                return;

            _pwEntrySettings.CpRecurseGroups = cbCpRecurseGroups.Checked;
        }

        private void btnRdpFile_Click(object sender, EventArgs e)
        {
            var disposeRdpFile = _pwEntrySettings.RdpFile != null;
            var rdpFileForm = new KprRdpFileForm(_pwEntrySettings.RdpFile)
            {
                IsReadOnly = _pwEntrySettings.IsReadOnly
            };

            GlobalWindowManager.AddWindow(rdpFileForm);
            var dr = UIUtil.ShowDialogAndDestroy(rdpFileForm);
            GlobalWindowManager.RemoveWindow(rdpFileForm);

            if (dr == DialogResult.OK)
            {
                _pwEntrySettings.RdpFile = new RdpFile(rdpFileForm.RdpFile, false);
                if (disposeRdpFile)
                    rdpFileForm.RdpFile.Dispose();
            }
        }

        private void btnMore_Click(object sender, EventArgs e)
        {
            if (cmsMore.Visible)
                cmsMore.Close();
            else
                cmsMore.Show(btnMore, new Point(-(cmsMore.PreferredSize.Width - 18) + btnMore.Width, btnMore.Height + 1), ToolStripDropDownDirection.BelowRight);

            if (ActiveControl == sender)
                ActiveControl = null;
        }

        private void cb_CheckedChanged(object sender, EventArgs e)
        {
            if (IsReadOnly)
                return;

            var checkBox = sender as CheckBox;
            var property = _pwEntrySettings.GetType().GetProperty(checkBox.Name.Substring(2), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if (property.PropertyType.IsEnum)
                property.SetValue(_pwEntrySettings, Enum.ToObject(property.PropertyType, (int)checkBox.CheckState), null);
            else
                property.SetValue(_pwEntrySettings, checkBox.Checked, null);
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            var button = sender as Button;

            var textBox = button.Tag as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
                return;

            var listBox = textBox.Tag as ListBox;
            var list = listBox.DataSource as BindingList<string>;

            if (!list.Contains(textBox.Text))
                list.Add(textBox.Text);

            textBox.ResetText();
        }

        private void cmdRemove_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var listBox = button.Tag as ListBox;
            var list = listBox.DataSource as BindingList<string>;

            listBox.BeginUpdate();
            foreach (var i in listBox.SelectedIndices.Cast<int>().OrderByDescending(x => x))
                list.RemoveAt(i);
            listBox.EndUpdate();
        }

        private void cmdReset_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var listBox = button.Tag as ListBox;
            var list = listBox.DataSource as BindingList<string>;

            listBox.BeginUpdate();
            list.Clear();
            listBox.EndUpdate();
        }

        private void txt_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Modifiers == Keys.None)
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                    case Keys.Escape:
                        var textBox = sender as TextBox;
                        if (string.IsNullOrEmpty(textBox.Text))
                            return;
                        e.IsInputKey = true;
                        break;
                }
        }

        private void lst_KeyDown(object sender, KeyEventArgs e)
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

        private void txtCpGroupUUIDs_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
                return;
            if (!Util.ClickButtonOnEnter(cmdCpGroupUUIDsAdd, e))
                Util.ResetTextOnEscape(textBox, e);
        }

        private void txtCpExcludedGroupUUIDs_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
                return;
            if (!Util.ClickButtonOnEnter(cmdCpExcludedGroupUUIDsAdd, e))
                Util.ResetTextOnEscape(textBox, e);
        }

        private void txtCpRegExPatterns_KeyDown(object sender, KeyEventArgs e)
        {
            txtGroupUUIDs_Enter(sender, EventArgs.Empty);
            var textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
                return;
            if (!Util.ClickButtonOnEnter(cmdCpRegExPatternsAdd, e))
                Util.ResetTextOnEscape(textBox, e);
        }

        private void txtMstscParameters_KeyDown(object sender, KeyEventArgs e)
        {
            txtGroupUUIDs_Enter(sender, EventArgs.Empty);
            var textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
                return;
            if (!Util.ClickButtonOnEnter(cmdMstscParametersAdd, e))
                Util.ResetTextOnEscape(textBox, e);
        }

        private void txtCpGroupUUIDs_TextChanged(object sender, EventArgs e)
        {
            cmdCpGroupUUIDsAdd.Enabled = !string.IsNullOrEmpty(txtCpGroupUUIDs.Text);
        }

        private void txtCpExcludedGroupUUIDs_TextChanged(object sender, EventArgs e)
        {
            cmdCpExcludedGroupUUIDsAdd.Enabled = !string.IsNullOrEmpty(txtCpExcludedGroupUUIDs.Text);
        }

        private void txtCpRegExPatterns_TextChanged(object sender, EventArgs e)
        {
            cmdCpRegExPatternsAdd.Enabled = !string.IsNullOrEmpty(txtCpRegExPatterns.Text);
        }

        private void txtMstscParameters_TextChanged(object sender, EventArgs e)
        {
            cmdMstscParametersAdd.Enabled = !string.IsNullOrEmpty(txtMstscParameters.Text);
        }

        private void txtGroupUUIDs_ShowToolTip(object sender, EventArgs e)
        {
            var timer = sender as Timer;
            timer.Enabled = false;

            var control = timer.Tag as Control;
            if (!string.IsNullOrEmpty(ttGroupPicker.GetToolTip(control)))
                return;

            var point = control.PointToClient(Cursor.Position);
            var size = _txtGroupUUIDsCursorSize.Value;

            if (!size.IsEmpty)
                point.Y += size.Height / 2;

            point.X += 2;
            point.Y += 1;

            ttGroupPicker.Show(
                KprResourceManager.Instance["Press right mouse button to open group picker,\r\nor enter hexstring of group UUID."],
                control,
                point,
                ttGroupPicker.AutoPopDelay);
        }

        private void txtGroupUUIDs_MouseEnter(object sender, EventArgs e)
        {
            _txtGroupUUIDsTooltipTimer.Tag = sender;
            _txtGroupUUIDsTooltipTimer.Tick += txtGroupUUIDs_ShowToolTip;
            if (_txtGroupUUIDsTooltipTimer.Enabled)
                _txtGroupUUIDsTooltipTimer.Enabled = false;
            _txtGroupUUIDsTooltipTimer.Enabled = !_lastTooltipMousePosition.HasValue;
        }

        private void txtGroupUUIDs_MouseLeave(object sender, EventArgs e)
        {
            _txtGroupUUIDsTooltipTimer.Tick -= txtGroupUUIDs_ShowToolTip;
            if (_txtGroupUUIDsTooltipTimer.Enabled)
                _txtGroupUUIDsTooltipTimer.Enabled = false;
            ttGroupPicker.Hide(sender as Control);
            _lastTooltipMousePosition = null;
        }

        private void txtGroupUUIDs_MouseMove(object sender, MouseEventArgs e)
        {
            if (_lastTooltipMousePosition.HasValue && _lastTooltipMousePosition.Value == e.Location)
                return;
            _lastTooltipMousePosition = e.Location;
            if (_txtGroupUUIDsTooltipTimer.Enabled)
                _txtGroupUUIDsTooltipTimer.Enabled = false;
            _txtGroupUUIDsTooltipTimer.Enabled = true;
        }

        private void txtGroupUUIDs_Enter(object sender, EventArgs e)
        {
            if (_txtGroupUUIDsTooltipTimer.Enabled)
                _txtGroupUUIDsTooltipTimer.Enabled = false;
            ttGroupPicker.Hide(sender as Control);
        }

        private void txtGroupUUIDs_Leave(object sender, EventArgs e)
        {
            if (_txtGroupUUIDsTooltipTimer.Enabled)
                _txtGroupUUIDsTooltipTimer.Enabled = false;
            ttGroupPicker.Hide(sender as Control);
            _lastTooltipMousePosition = null;
        }
    }
}