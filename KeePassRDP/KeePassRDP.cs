using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;

using KeePass.Plugins;
using KeePassLib;

namespace KeePassRDP
{
    public sealed class KeePassRDPExt : Plugin
    {
        private IPluginHost m_host = null;
        private ToolStripSeparator m_tsSeperator = null;
        private ToolStripMenuItem m_tsmiKeePassRDP = null;
        private ToolStripMenuItem m_tsmiOpenRDP = null;
        private ToolStripMenuItem m_tsmiOpenRDPAdmin = null;
        private ToolStripMenuItem m_tsmiOpenRDPNoCred = null;
        private ToolStripMenuItem m_tsmiOpenRDPNoCredAdmin = null;

        public override string UpdateUrl
        {
            get { return "https://gitlab.com/iSnackyCracky/KeePassRDP/raw/master/KeePassRDP.ver"; }
        }

        public override bool Initialize(IPluginHost host)
        {
            Debug.Assert(host != null);
            if (host == null) return false;
            m_host = host;

            // create m_cmsMenu as reference to the context-menu
            ToolStripItemCollection m_cmsMenu = m_host.MainWindow.EntryContextMenu.Items;

            // add a separator to the menu
            m_tsSeperator = new ToolStripSeparator();
            m_cmsMenu.Add(m_tsSeperator);

            // add the KeePassRDP menu entry
            m_tsmiKeePassRDP = new ToolStripMenuItem();
            m_tsmiKeePassRDP.Text = "KeePassRDP";
            m_cmsMenu.Add(m_tsmiKeePassRDP);
            
            // add the OpenRDP menu entry
            m_tsmiOpenRDP = new ToolStripMenuItem();
            m_tsmiOpenRDP.ShortcutKeys = Keys.Control | Keys.M;
            m_tsmiOpenRDP.ShowShortcutKeys = true;
            m_tsmiOpenRDP.Text = "Open RDP connection";
            m_tsmiOpenRDP.Click += OnOpenRDP_Click;
            m_tsmiKeePassRDP.DropDownItems.Add(m_tsmiOpenRDP);

            // add the OpenRDPAdmin menu entry
            m_tsmiOpenRDPAdmin = new ToolStripMenuItem();
            m_tsmiOpenRDPAdmin.ShortcutKeys = Keys.Control | Keys.Alt | Keys.M;
            m_tsmiOpenRDPAdmin.ShowShortcutKeys = true;
            m_tsmiOpenRDPAdmin.Text = "Open RDP connection (/admin)";
            m_tsmiOpenRDPAdmin.Click += OnOpenRDPAdmin_Click;
            m_tsmiKeePassRDP.DropDownItems.Add(m_tsmiOpenRDPAdmin);

            // add the OpenRDPNoCred menu entry
            m_tsmiOpenRDPNoCred = new ToolStripMenuItem();
            m_tsmiOpenRDPNoCred.Text = "Open RDP connection without credentials";
            m_tsmiOpenRDPNoCred.Click += OnOpenRDPNoCred_Click;
            m_tsmiKeePassRDP.DropDownItems.Add(m_tsmiOpenRDPNoCred);

            // add the OpenRDPNoCredAdmin menu entry
            m_tsmiOpenRDPNoCredAdmin = new ToolStripMenuItem();
            m_tsmiOpenRDPNoCredAdmin.Text = "Open RDP connection without credentials (/admin)";
            m_tsmiOpenRDPNoCredAdmin.Click += OnOpenRDPNoCredAdmin_Click;
            m_tsmiKeePassRDP.DropDownItems.Add(m_tsmiOpenRDPNoCredAdmin);

            return true;
        }

        public override void Terminate()
        {
            // clean up the context-menu
            ToolStripItemCollection m_cmsMenu = m_host.MainWindow.EntryContextMenu.Items;
            m_cmsMenu.Remove(m_tsSeperator);
            m_tsmiOpenRDP.Click -= OnOpenRDP_Click;
            m_cmsMenu.Remove(m_tsmiOpenRDP);
            m_tsmiOpenRDPAdmin.Click -= OnOpenRDPAdmin_Click;
            m_cmsMenu.Remove(m_tsmiOpenRDPAdmin);
            m_tsmiOpenRDPNoCred.Click -= OnOpenRDPNoCred_Click;
            m_cmsMenu.Remove(m_tsmiOpenRDPNoCred);
            m_tsmiOpenRDPNoCredAdmin.Click -= OnOpenRDPNoCredAdmin_Click;
            m_cmsMenu.Remove(m_tsmiOpenRDPNoCredAdmin);
            m_cmsMenu.Remove(m_tsmiKeePassRDP);
        }

        private void OnOpenRDP_Click(object sender, EventArgs e)
        {
            if (isValid(m_host))
            {
                // get selected entry
                PwEntry pe = m_host.MainWindow.GetSelectedEntry(true, true);

                pe = SelectCred(pe);

                if (!(pe == null))
                {
                    // connect
                    connectRDPtoKeePassEntry(pe, false, true);
                }
            }
        }

        private void OnOpenRDPAdmin_Click(object sender, EventArgs e)
        {
            if (isValid(m_host))
            {
                // get selected entry
                PwEntry pe = m_host.MainWindow.GetSelectedEntry(true, true);

                pe = SelectCred(pe);

                if (!(pe == null))
                {
                    // connect
                    connectRDPtoKeePassEntry(pe, true, true);
                }
            }
        }
        
        private void OnOpenRDPNoCred_Click(object sender, EventArgs e)
        {
            if (isValid(m_host))
            {
                // get selected entry
                PwEntry pe = m_host.MainWindow.GetSelectedEntry(true, true);
                // connect
                connectRDPtoKeePassEntry(pe);
            }
        }
        
        private void OnOpenRDPNoCredAdmin_Click(object sender, EventArgs e)
        {
            if (isValid(m_host))
            {
                // get selected entry
                PwEntry pe = m_host.MainWindow.GetSelectedEntry(true, true);
                // connect
                connectRDPtoKeePassEntry(pe, true);
            }
        }

        private KeePassLib.PwEntry SelectCred(PwEntry pe)
        {
            // create result for later use, see below
            DialogResult res = DialogResult.OK;

            // if selected entry is in a subgroup called "RDP", specified entries get collected and showed to the user for selection (see the RegEx in getDomainAccounts)
            if (inRdpSubgroup(pe))
            {
                // rdpPG is the parent-group of the "RDP" group
                PwGroup rdpPG = pe.ParentGroup.ParentGroup;

                // create PwObjectList with all matching entries directly inside the rdpPG
                KeePassLib.Collections.PwObjectList<PwEntry> rdpAccountEntries = getRdpAccountEntries(rdpPG);

                // extend the rdpAccountEntries list with matching entries in 1-level-subgroups of rdpPG
                foreach (PwGroup subPwG in rdpPG.Groups)
                {
                    rdpAccountEntries.Add(getRdpAccountEntries(subPwG));
                }

                // if matching entries were found...
                if (rdpAccountEntries.UCount >= 1)
                {
                    // create a selection dialog with the matching entries
                    CredentialPickerForm frmCredPick = new CredentialPickerForm();
                    frmCredPick.rdpAccountEntries = rdpAccountEntries;
                    frmCredPick.connPE = pe;
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

        private KeePassLib.Collections.PwObjectList<PwEntry> getRdpAccountEntries(PwGroup pwg)
        {
            // create PwObjectList and fill it with matching entries
            KeePassLib.Collections.PwObjectList<PwEntry> rdpAccountEntries = new KeePassLib.Collections.PwObjectList<PwEntry>();
            foreach (PwEntry pe in pwg.Entries)
            {
                string title = pe.Strings.ReadSafe(PwDefs.TitleField);

                string rePre = "(domain|domänen|lokaler)";
                string rePost = "(admin|user|administrator|benutzer)";
                string re = ".*" + rePre + ".*" + rePost + ".*";

                if (Regex.IsMatch(title, re, RegexOptions.IgnoreCase) && !Regex.IsMatch(title, ".*\\[rdpignore\\].*", RegexOptions.IgnoreCase))
                {
                    rdpAccountEntries.Add(pe);
                }
            }
            return rdpAccountEntries;
        }

        private void connectRDPtoKeePassEntry(PwEntry pe, bool useAdmin = false, bool useCreds = false)
        {
            String URL = stripURL(pe.Strings.ReadSafe(PwDefs.UrlField));
            if (!String.IsNullOrEmpty(URL))
            {
                Process credProcess = new Process();
                Process rdpProcess = new Process();
                // if selected, save credentials into the Windows Credential Manager
                if (useCreds)
                {
                    credProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\cmdkey.exe");
                    credProcess.StartInfo.Arguments = "/generic:" + stripURL(URL, true) + " /user:" + pe.Strings.ReadSafe(PwDefs.UserNameField) + " /pass:" + pe.Strings.ReadSafe(PwDefs.PasswordField);
                    credProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    credProcess.Start();
                    System.Threading.Thread.Sleep(300); 
                }
                // start RDP / mstsc.exe
                rdpProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\mstsc.exe");
                rdpProcess.StartInfo.Arguments = "/v:" + URL;
                if (useAdmin)
                {
                    rdpProcess.StartInfo.Arguments += " /admin";
                }
                rdpProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                rdpProcess.Start();
                // remove credentials from Windows Credential Manger (after about 10 seconds)
                if (useCreds)
                {
                    credProcess.StartInfo.Arguments = "/delete:" + URL;
                    credProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    // create a timer to remove the credentials after a given time
                    // could use System.Threading.Sleep, but this would halt the whole KeePass precess for its duration
                    System.Timers.Timer t = new System.Timers.Timer();
                    t.Interval = 9500; // timer triggers after 9.5 seconds
                    t.AutoReset = false; // timer should only trigger once
                    t.Elapsed += (sender, e) => timerElapsed(sender, e, credProcess);
                    t.Start(); // start the timer
                }
            }
        }

        /// <summary>
        /// Removes protocol "prefix" (i.e. http:// ; https:// ; ...) and optionally a following port (i.e. :8080) from a given string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string stripURL(string text, bool stripPort = false)
        {
            text = Regex.Replace(text, @"^(?:http(?:s)?://)?(?:www(?:[0-9]+)?.)?", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"^(?:(?:s)?ftp://)?(?:ftp.)?", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"^(?:ssh://)", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"^(?:rdp://)", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"(?:/.*)?", string.Empty, RegexOptions.IgnoreCase);
            if (stripPort)
            {
                text = Regex.Replace(text, @"(?:\:[0-9]+)?", string.Empty, RegexOptions.IgnoreCase);
            }
            return text;
        }

        /// <summary>
        /// Checks if the ParentGroup of a PwEntry is named "RDP".
        /// </summary>
        /// <param name="pe"></param>
        /// <returns></returns>
        private bool inRdpSubgroup(PwEntry pe)
        {
            PwGroup pg = pe.ParentGroup;
            return pg.Name == "RDP";
        }

        private void timerElapsed(object source, System.Timers.ElapsedEventArgs e, Process proc)
        {
            proc.Start();
        }

        // Check if action is a valid operation
        private bool isValid(IPluginHost host)
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
