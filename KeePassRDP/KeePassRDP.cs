using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;

using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Collections;

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

            return true;
        }

        public override void Terminate()
        {
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
                var tsmiOpenRDPNoCred = new ToolStripMenuItem
                {
                    Text = "Open RDP connection without credentials"
                };
                tsmiOpenRDPNoCred.Click += OnOpenRDPNoCred_Click;
                tsmi.DropDownItems.Add(tsmiOpenRDPNoCred);

                // add the OpenRDPNoCredAdmin menu entry
                var tsmiOpenRDPNoCredAdmin = new ToolStripMenuItem
                {
                    Text = "Open RDP connection without credentials (/admin)"
                };
                tsmiOpenRDPNoCredAdmin.Click += OnOpenRDPNoCredAdmin_Click;
                tsmi.DropDownItems.Add(tsmiOpenRDPNoCredAdmin);

                // disable the entry menu when no database is opened
                tsmi.DropDownOpening += delegate (object sender, EventArgs e)
                {
                    var pd = m_host.Database;
                    bool dbOpen = ((pd != null) && pd.IsOpen);
                    tsmi.Enabled = dbOpen;
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

        private PwEntry SelectCred(PwEntry pe)
        {
            // create result for later use, see below
            DialogResult res = DialogResult.OK;

            // if selected entry is in a subgroup called "RDP", specified entries get collected and showed to the user for selection (see the RegEx in getDomainAccounts)
            if (InRdpSubgroup(pe))
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
                    var frmCredPick = new CredentialPickerForm
                    {
                        rdpAccountEntries = rdpAccountEntries,
                        connPE = pe
                    };

                    // show the dialog and get the result
                    res = frmCredPick.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        // use the selected PwEntry and reset the selected value of the dialog
                        pe = frmCredPick.returnPE;
                        frmCredPick.returnPE = null;
                    }
                }
            }

            if (res == DialogResult.OK)
            {
                return pe;
            }
            else
            {
                return null;
            }
        }

        private PwObjectList<PwEntry> GetRdpAccountEntries(PwGroup pwg)
        {
            // create PwObjectList and fill it with matching entries
            var rdpAccountEntries = new PwObjectList<PwEntry>();
            foreach (PwEntry pe in pwg.Entries)
            {
                string title = pe.Strings.ReadSafe(PwDefs.TitleField);

                string rePre = "(domain|domänen|local|lokaler|windows)";
                string rePost = "(admin|user|administrator|benutzer|nutzer)";
                string re = ".*" + rePre + ".*" + rePost + ".*";

                if (Regex.IsMatch(title, re, RegexOptions.IgnoreCase) && !Regex.IsMatch(title, ".*\\[rdpignore\\].*", RegexOptions.IgnoreCase))
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

        /// <summary>
        /// Checks if the ParentGroup of a PwEntry is named "RDP".
        /// </summary>
        /// <param name="pe"></param>
        /// <returns></returns>
        private bool InRdpSubgroup(PwEntry pe)
        {
            PwGroup pg = pe.ParentGroup;
            return pg.Name == "RDP";
        }

        private void TimerElapsed(object source, System.Timers.ElapsedEventArgs e, Process proc)
        {
            proc.Start();
        }

        // Check if action is a valid operation
        private bool IsValid(IPluginHost host)
        {
            if (!host.Database.IsOpen)
            {
                MessageBox.Show("You have to open a KeePass Database first", "KeePassRDP");
                return false;
            }
            if (host.MainWindow.GetSelectedEntry(true, true) == null)
            {
                MessageBox.Show("You have to select an Entry first", "KeePassRDP");
                return false;
            }
            return true;
        }
        
    }
}
