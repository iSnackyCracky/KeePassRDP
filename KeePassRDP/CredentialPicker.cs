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
 *  along with KeePassRDP.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using KeePass.UI;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KeePassRDP
{
    internal class CredentialPicker
    {
        public PwObjectList<PwGroup> CredentialRoots { get; set; }
        public bool RecurseGroups { get; set; }
        public List<string> RegexPatterns { get; set; }
        public PwObjectList<PwGroup> ExcludedGroups { get; set; }

        public CredentialPicker(PwObjectList<PwGroup> CredentialRoots, List<string> RegexPatterns, bool RecurseGroups = true)
        {
            this.CredentialRoots = CredentialRoots;
            this.RegexPatterns = RegexPatterns;
            this.RecurseGroups = RecurseGroups;
            this.ExcludedGroups = new PwObjectList<PwGroup>();
        }
        public CredentialPicker(PwObjectList<PwGroup> CredentialRoots, List<string> RegexPatterns, PwObjectList<PwGroup> ExcludedGroups, bool RecurseGroups = true)
        {
            this.CredentialRoots = CredentialRoots;
            this.RegexPatterns = RegexPatterns;
            this.RecurseGroups = RecurseGroups;
            this.ExcludedGroups = ExcludedGroups;
        }

        public PwEntry GetCredentialEntry()
        {
            PwEntry pe = null;

            return pe;
        }

        //private PwEntry SelectCred(PwEntry pe)
        //{
        //    var retPE = new PwEntry(true, true);

        //    // if selected entry is in a subgroup called "RDP", specified entries get collected and showed to the user for selection (see the RegEx in GetRdpAccountEntries)
        //    if (Util.InRdpSubgroup(pe))
        //    {
        //        // rdpPG is the parent-group of the "RDP" group
        //        PwGroup rdpPG = pe.ParentGroup.ParentGroup;

        //        // create PwObjectList with all matching entries directly inside the rdpPG
        //        PwObjectList<PwEntry> rdpAccountEntries = GetRdpAccountEntries(rdpPG);

        //        // extend the rdpAccountEntries list with matching entries in 1-level-subgroups of rdpPG
        //        foreach (PwGroup subPwG in rdpPG.Groups) { rdpAccountEntries.Add(GetRdpAccountEntries(subPwG)); }

        //        // if matching entries were found...
        //        if (rdpAccountEntries.UCount >= 1)
        //        {
        //            // create a selection dialog with the matching entries
        //            var frmCredPick = new CredentialPickerForm(_config, m_host.Database)
        //            {
        //                rdpAccountEntries = rdpAccountEntries,
        //                connPE = pe
        //            };

        //            // show the dialog and get the result
        //            var res = frmCredPick.ShowDialog();
        //            if (res == System.Windows.Forms.DialogResult.OK)
        //            {
        //                // use the selected PwEntry and reset the selected value of the dialog
        //                retPE = frmCredPick.returnPE;
        //                UIUtil.DestroyForm(frmCredPick);
        //            }
        //            else
        //            {
        //                retPE = null;
        //                UIUtil.DestroyForm(frmCredPick);
        //            }
        //        }
        //        // if no matching entries were found...
        //        else
        //        {
        //            // fall back to using the currently selected entry
        //            retPE.Strings.Set(PwDefs.UserNameField, pe.Strings.GetSafe(PwDefs.UserNameField));
        //            retPE.Strings.Set(PwDefs.PasswordField, pe.Strings.GetSafe(PwDefs.PasswordField));
        //            retPE.Strings.Set(PwDefs.UrlField, pe.Strings.GetSafe(PwDefs.UrlField));
        //        }
        //    }
        //    else
        //    {
        //        retPE.Strings.Set(PwDefs.UserNameField, pe.Strings.GetSafe(PwDefs.UserNameField));
        //        retPE.Strings.Set(PwDefs.PasswordField, pe.Strings.GetSafe(PwDefs.PasswordField));
        //        retPE.Strings.Set(PwDefs.UrlField, pe.Strings.GetSafe(PwDefs.UrlField));
        //    }

        //    if (!(retPE == null))
        //    {
        //        // resolve References in entry fields
        //        retPE.Strings.Set(PwDefs.UserNameField, new ProtectedString(false, Util.ResolveReferences(retPE, m_host.Database, PwDefs.UserNameField)));
        //        retPE.Strings.Set(PwDefs.PasswordField, new ProtectedString(true, Util.ResolveReferences(retPE, m_host.Database, PwDefs.PasswordField)));
        //        retPE.Strings.Set(PwDefs.UrlField, new ProtectedString(false, Util.ResolveReferences(retPE, m_host.Database, PwDefs.UrlField)));
        //    }

        //    return retPE;
        //}

        //private PwObjectList<PwEntry> GetRdpAccountEntries(PwGroup pwg)
        //{
        //    // create PwObjectList and fill it with matching entries
        //    var rdpAccountEntries = new PwObjectList<PwEntry>();
        //    foreach (PwEntry pe in pwg.Entries)
        //    {
        //        string title = pe.Strings.ReadSafe(PwDefs.TitleField);
        //        bool ignore = Util.IsEntryIgnored(pe);

        //        string re = ".*(" + _config.CredPickerRegExPre + ").*(" + _config.CredPickerRegExPost + ").*";

        //        if (!ignore && Regex.IsMatch(title, re, RegexOptions.IgnoreCase)) { rdpAccountEntries.Add(pe); }
        //    }
        //    return rdpAccountEntries;
        //}
    }
}
