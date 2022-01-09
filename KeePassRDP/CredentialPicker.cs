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
using KeePassLib.Utility;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KeePassRDP
{
    internal class CredentialPicker
    {
        private readonly PwEntry _pe;
        private readonly KprEntrySettings _peSettings;
        private readonly PwDatabase _database;
        private readonly KprConfig _config;
        private List<PwUuid> _GroupUUIDs;
        private List<PwUuid> _ExcludedGroupUUIDs;

        public CredentialPicker(PwEntry pe, KprEntrySettings peSettings, PwDatabase database, KprConfig config)
        {
            _pe = pe;
            _peSettings = peSettings;
            _database = database;
            _config = config;
        }

        public PwEntry GetCredentialEntry()
        {
            PwEntry pe = null;

            // build a list of excluded group UUIDs
            _ExcludedGroupUUIDs = new List<PwUuid>();
            foreach (string uuidString in _peSettings.CpExcludedGroupUUIDs)
            {
                byte[] uuidBytes = MemUtil.HexStringToByteArray(uuidString);
                if (uuidBytes != null) { _ExcludedGroupUUIDs.Add(new PwUuid(uuidBytes)); }
            }

            // build a list of included group UUIDs (do not add if it's excluded)
            _GroupUUIDs = new List<PwUuid>();
            foreach (string uuidString in _peSettings.CpGroupUUIDs)
            {
                byte[] uuidBytes = MemUtil.HexStringToByteArray(uuidString);
                if (uuidBytes != null) { AddUuidToList(new PwUuid(uuidBytes)); }
            }

            // include rdp parent group if given and not excluded
            if (Util.InRdpSubgroup(_pe)) { AddUuidToList(_pe.ParentGroup.ParentGroup.Uuid); }

            if (_GroupUUIDs.Count >= 1)
            {
                var accountEntries = new PwObjectList<PwEntry>();
                foreach (PwUuid uuid in _GroupUUIDs)
                {
                    var group = _database.RootGroup.FindGroup(uuid, true);
                    accountEntries.Add(GetRdpAccountEntries(group));
                }

                if (accountEntries.UCount >= 1)
                {
                    // create a selection dialog with the matching entries
                    var frmCredPick = new CredentialPickerForm(_config, _database)
                    {
                        RdpAccountEntries = accountEntries,
                        ConnPE = pe
                    };

                    // show the dialog and get the result
                    var res = frmCredPick.ShowDialog();
                    if (res == System.Windows.Forms.DialogResult.OK)
                    {
                        // use the selected PwEntry and reset the selected value of the dialog
                        pe = frmCredPick.ReturnPE;
                        UIUtil.DestroyForm(frmCredPick);
                    }
                    else { UIUtil.DestroyForm(frmCredPick); }

                }
                else { pe = _pe; }

            }

            return pe;
        }

        private void AddUuidToList(PwUuid uuid)
        {
            if (_ExcludedGroupUUIDs.Contains(uuid)) { return; }
            if (_GroupUUIDs.Contains(uuid)) { return; }

            _GroupUUIDs.Add(uuid);
        }

        private PwObjectList<PwEntry> GetRdpAccountEntries(PwGroup pwg)
        {
            // create PwObjectList and fill it with matching entries
            var rdpAccountEntries = new PwObjectList<PwEntry>();
            foreach (PwEntry pe in pwg.Entries)
            {
                string title = pe.Strings.ReadSafe(PwDefs.TitleField);
                bool ignore = Util.IsEntryIgnored(pe);

                string re = string.Empty;
                if (_peSettings.CpIncludeDefaultRegex) { re = ".*(" + _config.CredPickerRegExPre + ").*(" + _config.CredPickerRegExPost + ").*"; }

                foreach (string regex in _peSettings.CpRegExPatterns)
                {
                    re += string.IsNullOrEmpty(re) ? string.Empty : "|";
                    re += regex;
                }

                if (!ignore && Regex.IsMatch(title, re, RegexOptions.IgnoreCase)) { rdpAccountEntries.Add(pe); }
            }
            return rdpAccountEntries;
        }
    }
}
