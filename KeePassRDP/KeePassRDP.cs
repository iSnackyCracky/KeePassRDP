/*
 *  Copyright (C) 2018-2020 iSnackyCracky
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
 *  along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using CredentialManagement;
using KeePass.Ecas;
using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace KeePassRDP
{
    public sealed class KeePassRDPExt : Plugin
    {
        private IPluginHost m_host = null;
        private CredentialManager _credManager = null;
        private KprConfig _config = null;

        public override string UpdateUrl { get { return "https://raw.githubusercontent.com/iSnackyCracky/KeePassRDP/master/KeePassRDP.ver"; } }

        public override bool Initialize(IPluginHost host)
        {
            Debug.Assert(host != null);
            if (host == null) { return false; }
            m_host = host;

            m_host.MainWindow.AddCustomToolBarButton(Util.ToolbarConnectBtnId, "💻", "Connect to entry via RDP using credentials.");
            m_host.TriggerSystem.RaisingEvent += TriggerSystem_RaisingEvent;

            _credManager = new CredentialManager();
            _config = new KprConfig(m_host.CustomConfig);

            return true;
        }

        private void TriggerSystem_RaisingEvent(object sender, EcasRaisingEventArgs e)
        {
            EcasPropertyDictionary dict = e.Properties;
            if (dict == null) { Debug.Assert(false); return; }

            string command = dict.Get<String>(EcasProperty.CommandID);

            if (command != null && command.Equals(Util.ToolbarConnectBtnId))
            {
                ConnectRDPtoKeePassEntry(false, true);
            }
        }

        public override void Terminate()
        {
            _credManager.RemoveAll();
            m_host.MainWindow.RemoveCustomToolBarButton(Util.ToolbarConnectBtnId);
        }

        public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
        {
            // init a "null" menu item
            ToolStripMenuItem tsmi = null;

            if (t == PluginMenuType.Entry)
            {
                // create entry menu item
                tsmi = new ToolStripMenuItem("KeePassRDP");

                // add the OpenRDP menu entry
                var tsmiOpenRDP = new ToolStripMenuItem
                {
                    ShortcutKeys = Keys.Control | Keys.M,
                    ShowShortcutKeys = true,
                    Text = "Open RDP connection"
                };
                tsmiOpenRDP.Click += OnOpenRDP_Click;
                tsmi.DropDownItems.Add(tsmiOpenRDP);

                // add the OpenRDPAdmin menu entry
                var tsmiOpenRDPAdmin = new ToolStripMenuItem
                {
                    ShortcutKeys = Keys.Control | Keys.Alt | Keys.M,
                    ShowShortcutKeys = true,
                    Text = "Open RDP connection (/admin)"
                };
                tsmiOpenRDPAdmin.Click += OnOpenRDPAdmin_Click;
                tsmi.DropDownItems.Add(tsmiOpenRDPAdmin);

                // add the OpenRDPNoCred menu entry
                var tsmiOpenRDPNoCred = new ToolStripMenuItem("Open RDP connection without credentials");
                tsmiOpenRDPNoCred.Click += OnOpenRDPNoCred_Click;
                tsmi.DropDownItems.Add(tsmiOpenRDPNoCred);

                // add the OpenRDPNoCredAdmin menu entry
                var tsmiOpenRDPNoCredAdmin = new ToolStripMenuItem("Open RDP connection without credentials (/admin)");
                tsmiOpenRDPNoCredAdmin.Click += OnOpenRDPNoCredAdmin_Click;
                tsmi.DropDownItems.Add(tsmiOpenRDPNoCredAdmin);

                // add the IgnoreCredEntry menu entry
                var tsmiIgnoreCredEntry = new ToolStripMenuItem("Ignore these credentials");
                tsmiIgnoreCredEntry.Click += OnIgnoreCredEntry_Click;
                tsmi.DropDownItems.Add(tsmiIgnoreCredEntry);

                // disable the entry menu when no database is opened
                tsmi.DropDownOpening += delegate (object sender, EventArgs e)
                {
                    if (IsValid(m_host, false)) { tsmiIgnoreCredEntry.Checked = Util.IsEntryIgnored(m_host.MainWindow.GetSelectedEntry(true, true)); }
                };
            }
            else if (t == PluginMenuType.Main)
            {
                // create the main menu options item
                tsmi = new ToolStripMenuItem("KeePassRDP Options");
                tsmi.Click += OnKPROptions_Click;
            }

            return tsmi;
        }

        private void OnKPROptions_Click(object sender, EventArgs e) { UIUtil.ShowDialogAndDestroy(new KPROptionsForm(_config)); }

        private void OnOpenRDP_Click(object sender, EventArgs e) { ConnectRDPtoKeePassEntry(false, true); }

        private void OnOpenRDPAdmin_Click(object sender, EventArgs e) { ConnectRDPtoKeePassEntry(true, true); }

        private void OnOpenRDPNoCred_Click(object sender, EventArgs e) { ConnectRDPtoKeePassEntry(); }

        private void OnOpenRDPNoCredAdmin_Click(object sender, EventArgs e) { ConnectRDPtoKeePassEntry(true); }

        private void OnIgnoreCredEntry_Click(object sender, EventArgs e)
        {
            if (IsValid(m_host))
            {
                Util.ToggleEntryIgnored(m_host.MainWindow.GetSelectedEntry(true, true));
                var f = (MethodInvoker)delegate { m_host.MainWindow.UpdateUI(false, null, false, null, true, null, true); };
                if (m_host.MainWindow.InvokeRequired) { m_host.MainWindow.Invoke(f); }
                else { f.Invoke(); }
            }
        }

        private PwEntry SelectCred(PwEntry pe)
        {
            var retPE = new PwEntry(true, true);

            // if selected entry is in a subgroup called "RDP", specified entries get collected and showed to the user for selection (see the RegEx in GetRdpAccountEntries)
            if (Util.InRdpSubgroup(pe))
            {
                // rdpPG is the parent-group of the "RDP" group
                PwGroup rdpPG = pe.ParentGroup.ParentGroup;

                // create PwObjectList with all matching entries directly inside the rdpPG
                PwObjectList<PwEntry> rdpAccountEntries = GetRdpAccountEntries(rdpPG);

                // extend the rdpAccountEntries list with matching entries in 1-level-subgroups of rdpPG
                foreach (PwGroup subPwG in rdpPG.Groups) { rdpAccountEntries.Add(GetRdpAccountEntries(subPwG)); }

                // if matching entries were found...
                if (rdpAccountEntries.UCount >= 1)
                {
                    // create a selection dialog with the matching entries
                    var frmCredPick = new CredentialPickerForm(_config, m_host.Database)
                    {
                        rdpAccountEntries = rdpAccountEntries,
                        connPE = pe
                    };

                    // show the dialog and get the result
                    var res = frmCredPick.ShowDialog();
                    if (res == System.Windows.Forms.DialogResult.OK)
                    {
                        // use the selected PwEntry and reset the selected value of the dialog
                        retPE = frmCredPick.returnPE;
                        UIUtil.DestroyForm(frmCredPick);
                    }
                }
                // if no matching entries were found...
                else
                {
                    // fall back to using the currently selected entry
                    retPE.Strings.Set(PwDefs.UserNameField, pe.Strings.GetSafe(PwDefs.UserNameField));
                    retPE.Strings.Set(PwDefs.PasswordField, pe.Strings.GetSafe(PwDefs.PasswordField));
                    retPE.Strings.Set(PwDefs.UrlField, pe.Strings.GetSafe(PwDefs.UrlField));
                }
            }
            else
            {
                retPE.Strings.Set(PwDefs.UserNameField, pe.Strings.GetSafe(PwDefs.UserNameField));
                retPE.Strings.Set(PwDefs.PasswordField, pe.Strings.GetSafe(PwDefs.PasswordField));
                retPE.Strings.Set(PwDefs.UrlField, pe.Strings.GetSafe(PwDefs.UrlField));
            }

            // resolve References in entry fields
            retPE.Strings.Set(PwDefs.UserNameField, new ProtectedString(false, Util.ResolveReferences(retPE, m_host.Database, PwDefs.UserNameField)));
            retPE.Strings.Set(PwDefs.PasswordField, new ProtectedString(true, Util.ResolveReferences(retPE, m_host.Database, PwDefs.PasswordField)));
            retPE.Strings.Set(PwDefs.UrlField, new ProtectedString(false, Util.ResolveReferences(retPE, m_host.Database, PwDefs.UrlField)));

            return retPE;
        }

        private PwObjectList<PwEntry> GetRdpAccountEntries(PwGroup pwg)
        {
            // create PwObjectList and fill it with matching entries
            var rdpAccountEntries = new PwObjectList<PwEntry>();
            foreach (PwEntry pe in pwg.Entries)
            {
                string title = pe.Strings.ReadSafe(PwDefs.TitleField);
                bool ignore = Util.IsEntryIgnored(pe);

                string re = ".*(" + _config.CredPickerRegExPre + ").*(" + _config.CredPickerRegExPost + ").*";

                if (!ignore && Regex.IsMatch(title, re, RegexOptions.IgnoreCase)) { rdpAccountEntries.Add(pe); }
            }
            return rdpAccountEntries;
        }

        private void ConnectRDPtoKeePassEntry(bool tmpMstscUseAdmin = false, bool tmpUseCreds = false)
        {
            if (IsValid(m_host))
            {
                // get selected entry for connection
                var connPwEntry = m_host.MainWindow.GetSelectedEntry(true, true);

                // get credentials for connection
                if (tmpUseCreds) { connPwEntry = SelectCred(connPwEntry); }

                if (!(connPwEntry == null))
                {
                    string URL = Util.StripUrl(connPwEntry.Strings.ReadSafe(PwDefs.UrlField));

                    if (!String.IsNullOrEmpty(URL))
                    {
                        var rdpProcess = new Process();

                        // if selected, save credentials into the Windows Credential Manager
                        if (tmpUseCreds)
                        {

                            // First instantiate a new KprCredential object.
                            var cred = new KprCredential(
                                connPwEntry.Strings.ReadSafe(PwDefs.UserNameField),
                                connPwEntry.Strings.ReadSafe(PwDefs.PasswordField),
                                _config.CredVaultUseWindows ? "TERMSRV/" + Util.StripUrl(URL, true) : Util.StripUrl(URL, true),
                                _config.CredVaultUseWindows ? CredentialType.DomainPassword : CredentialType.Generic,
                                Convert.ToInt32(_config.CredVaultTtl)
                            );

                            // Then give the KprCredential to the CredentialManager for managing the Windows Vault.
                            _credManager.Add(cred);

                            System.Threading.Thread.Sleep(300);
                        }

                        // start RDP / mstsc.exe
                        rdpProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\mstsc.exe");
                        rdpProcess.StartInfo.Arguments = "/v:" + URL;
                        if (tmpMstscUseAdmin || _config.MstscUseAdmin) { rdpProcess.StartInfo.Arguments += " /admin"; }
                        if (_config.MstscUseFullscreen) { rdpProcess.StartInfo.Arguments += " /f"; }
                        if (_config.MstscUseSpan) { rdpProcess.StartInfo.Arguments += " /span"; }
                        if (_config.MstscUseMultimon) { rdpProcess.StartInfo.Arguments += " /multimon"; }
                        if (_config.MstscWidth > 0) { rdpProcess.StartInfo.Arguments += " /w:" + _config.MstscWidth; }
                        if (_config.MstscHeight > 0) { rdpProcess.StartInfo.Arguments += " /h:" + _config.MstscHeight; }
                        rdpProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        rdpProcess.Start();
                    }
                }
            }
        }

        // Check if action is a valid operation
        private bool IsValid(IPluginHost host, bool showMsg = true)
        {
            if (!host.Database.IsOpen)
            {
                if (showMsg) MessageBox.Show("You have to open a KeePass Database first", "KeePassRDP");
                return false;
            }
            if (host.MainWindow.GetSelectedEntry(true, true) == null)
            {
                if (showMsg) MessageBox.Show("You have to select an Entry first", "KeePassRDP");
                return false;
            }
            return true;
        }

    }
}
