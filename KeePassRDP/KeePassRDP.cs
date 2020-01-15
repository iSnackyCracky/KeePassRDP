using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;

using KeePass.Ecas;
using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;

namespace KeePassRDP
{
    public sealed class KeePassRDPExt : Plugin
    {
        private IPluginHost m_host = null;
       
        public override string UpdateUrl
        {
            get { return "https://raw.githubusercontent.com/iSnackyCracky/KeePassRDP/master/KeePassRDP.ver"; }
        }

        public override bool Initialize(IPluginHost host)
        {
            Debug.Assert(host != null);
            if (host == null) return false;
            m_host = host;

            m_host.MainWindow.AddCustomToolBarButton(Util.ToolbarConnectBtnId, "💻", "Connect to entry via RDP using credentials.");
            m_host.TriggerSystem.RaisingEvent += TriggerSystem_RaisingEvent;

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
                    if (IsValid(m_host, false)) tsmiIgnoreCredEntry.Checked = Util.IsEntryIgnored(m_host.MainWindow.GetSelectedEntry(true, true));
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

        private void OnKPROptions_Click(object sender, EventArgs e)
        {
            UIUtil.ShowDialogAndDestroy(new KPROptionsForm(new KprConfig(m_host.CustomConfig)));
        }

        private void OnOpenRDP_Click(object sender, EventArgs e)
        {
            ConnectRDPtoKeePassEntry(false, true);
        }

        private void OnOpenRDPAdmin_Click(object sender, EventArgs e)
        {
            ConnectRDPtoKeePassEntry(true, true);
        }
        
        private void OnOpenRDPNoCred_Click(object sender, EventArgs e)
        {
            ConnectRDPtoKeePassEntry();
        }
        
        private void OnOpenRDPNoCredAdmin_Click(object sender, EventArgs e)
        {
            ConnectRDPtoKeePassEntry(true);
        }

        private void OnIgnoreCredEntry_Click(object sender, EventArgs e)
        {
            if (IsValid(m_host))
            {
                Util.ToggleEntryIgnored(m_host.MainWindow.GetSelectedEntry(true, true));
                var f = (MethodInvoker)delegate
                {
                    m_host.MainWindow.UpdateUI(false, null, false, null, true, null, true);
                };
                if (m_host.MainWindow.InvokeRequired)
                {
                    m_host.MainWindow.Invoke(f);
                } else
                {
                    f.Invoke();
                }
            }
        }

        private PwEntry SelectCred(PwEntry pe)
        {
            var retPE = new PwEntry(true, true);

            // if selected entry is in a subgroup called "RDP", specified entries get collected and showed to the user for selection (see the RegEx in getDomainAccounts)
            if (Util.InRdpSubgroup(pe))
            {
                // rdpPG is the parent-group of the "RDP" group
                PwGroup rdpPG = pe.ParentGroup.ParentGroup;

                // create PwObjectList with all matching entries directly inside the rdpPG
                PwObjectList<PwEntry> rdpAccountEntries = GetRdpAccountEntries(rdpPG);

                // extend the rdpAccountEntries list with matching entries in 1-level-subgroups of rdpPG
                foreach (PwGroup subPwG in rdpPG.Groups)
                {
                    rdpAccountEntries.Add(GetRdpAccountEntries(subPwG));
                }

                // if matching entries were found...
                if (rdpAccountEntries.UCount >= 1)
                {
                    // create a selection dialog with the matching entries
                    var frmCredPick = new CredentialPickerForm(new KprConfig(m_host.CustomConfig), m_host.Database)
                    {
                        rdpAccountEntries = rdpAccountEntries,
                        connPE = pe
                    };

                    // show the dialog and get the result
                    var res = frmCredPick.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        // use the selected PwEntry and reset the selected value of the dialog
                        retPE = frmCredPick.returnPE;
                        UIUtil.DestroyForm(frmCredPick);
                    }
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

                string rePre = "(domain|domänen|local|lokaler|windows)";
                string rePost = "(admin|user|administrator|benutzer|nutzer)";
                string re = ".*" + rePre + ".*" + rePost + ".*";

                if (!ignore && Regex.IsMatch(title, re, RegexOptions.IgnoreCase))
                {
                    rdpAccountEntries.Add(pe);
                }
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
                if (tmpUseCreds)
                {
                    connPwEntry = SelectCred(connPwEntry);
                }

                if (!(connPwEntry == null)) {
                    string URL = StripUrl(connPwEntry.Strings.ReadSafe(PwDefs.UrlField));

                    if (!String.IsNullOrEmpty(URL)) {
                        // instantiate config Object to get configured options
                        var kprConfig = new KprConfig(m_host.CustomConfig);

                        var credProcess = new Process();
                        var rdpProcess = new Process();

                        // if selected, save credentials into the Windows Credential Manager
                        if (tmpUseCreds) {
                            credProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\cmdkey.exe");
                            credProcess.StartInfo.Arguments = "/generic:" + StripUrl(URL, true) + " /user:" + connPwEntry.Strings.ReadSafe(PwDefs.UserNameField) + " /pass:" + connPwEntry.Strings.ReadSafe(PwDefs.PasswordField);
                            credProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            credProcess.Start();
                            System.Threading.Thread.Sleep(300);
                        }

                        // start RDP / mstsc.exe
                        rdpProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\mstsc.exe");
                        rdpProcess.StartInfo.Arguments = "/v:" + URL;
                        if (tmpMstscUseAdmin || kprConfig.MstscUseAdmin) {
                            rdpProcess.StartInfo.Arguments += " /admin";
                        }
                        if (kprConfig.MstscUseFullscreen) {
                            rdpProcess.StartInfo.Arguments += " /f";
                        }
                        if (kprConfig.MstscUseSpan) {
                            rdpProcess.StartInfo.Arguments += " /span";
                        }
                        if (kprConfig.MstscUseMultimon) {
                            rdpProcess.StartInfo.Arguments += " /multimon";
                        }
                        if (kprConfig.MstscWidth > 0) {
                            rdpProcess.StartInfo.Arguments += " /w:" + kprConfig.MstscWidth;
                        }
                        if (kprConfig.MstscHeight > 0) {
                            rdpProcess.StartInfo.Arguments += " /h:" + kprConfig.MstscHeight;
                        }
                        rdpProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        rdpProcess.Start();

                        // remove credentials from Windows Credential Manger (after about 10 seconds)
                        if (tmpUseCreds) {
                            credProcess.StartInfo.Arguments = "/delete:" + StripUrl(URL, true);
                            credProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            // create a timer to remove the credentials after a given time
                            // could use System.Threading.Sleep, but this would halt the whole KeePass precess for its duration
                            var t = new System.Timers.Timer {
                                Interval = 9500, // timer triggers after 9.5 seconds
                                AutoReset = false // timer should only trigger once
                            };
                            t.Elapsed += (sender, e) => TimerElapsed(sender, e, credProcess);
                            t.Start(); // start the timer
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes protocol "prefix" (i.e. http:// ; https:// ; ...) and optionally a following port (i.e. :8080) from a given string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string StripUrl(string text, bool stripPort = false)
        {
            text = Regex.Replace(text, @"^(?:http(?:s)?://)?(?:www(?:[0-9]+)?.)?", String.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"^(?:(?:s)?ftp://)?(?:ftp.)?", String.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"^(?:ssh://)", String.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"^(?:rdp://)", String.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"^(?:mailto:)", String.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"^(?:callto:)", String.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"^(?:tel:)", String.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"(?:/.*)?", String.Empty, RegexOptions.IgnoreCase);
            if (stripPort)
            {
                text = Regex.Replace(text, @"(?:\:[0-9]+)", String.Empty, RegexOptions.IgnoreCase);
            }
            return text;
        }


        private void TimerElapsed(object source, System.Timers.ElapsedEventArgs e, Process proc)
        {
            proc.Start();
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
