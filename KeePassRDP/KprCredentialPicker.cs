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
using KeePassLib;
using KeePassLib.Utility;
using KeePassRDP.Extensions;
using KeePassRDP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace KeePassRDP
{
    internal class KprCredentialPicker : IDisposable
    {
        internal Lazy<KprCredentialPickerForm> CredentialPickerForm { get { return _credentialPickerForm; } }

        private readonly IPluginHost _host;
        private readonly KprConfig _config;
        private readonly Lazy<KprCredentialPickerForm> _credentialPickerForm;
        private readonly HashSet<PwUuid> _groupUUIDs;
        private readonly HashSet<PwUuid> _excludedGroupUUIDs;
        private readonly StringBuilder _regexBuilder;

        private readonly List<PwEntry> _pes;
        private KprEntrySettings _peSettings;
        private bool _defaultRegexIncluded;

        public KprCredentialPicker(IPluginHost host, KprConfig config)
        {
            _host = host;
            _config = config;
            _credentialPickerForm = new Lazy<KprCredentialPickerForm>(
                () => new KprCredentialPickerForm(_config, _host),
                LazyThreadSafetyMode.ExecutionAndPublication);
            _groupUUIDs = new HashSet<PwUuid>();
            _excludedGroupUUIDs = new HashSet<PwUuid>();
            _regexBuilder = new StringBuilder();
            _pes = new List<PwEntry>();
            _peSettings = null;
            _defaultRegexIncluded = false;
        }

        public void Dispose()
        {
            if (_credentialPickerForm.IsValueCreated)
                _credentialPickerForm.Value.Dispose();
        }

        /// <summary>
        /// Clear <see cref="KprCredentialPicker"/>.
        /// </summary>
        public void Clear()
        {
            _excludedGroupUUIDs.Clear();
            _groupUUIDs.Clear();
            _regexBuilder.Clear();
            _pes.Clear();
            _defaultRegexIncluded = false;
        }

        /// <summary>
        /// Reset <see cref="KprCredentialPicker"/> for next usage.
        /// </summary>
        public void Reset()
        {
            Clear();
            _peSettings = null;
        }

        /// <summary>
        /// Add <see cref="PwEntry"/> to select credentials for, respecting <see cref="KprEntrySettings"/>.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="entrySettings"></param>
        /// <param name="triggerGroup"></param>
        public void AddEntry(PwEntry entry, KprEntrySettings entrySettings, string triggerGroup = Util.DefaultTriggerGroup)
        {
            // When entry parent group is trigger group add trigger parent group for recursive search.
            if (entrySettings.CpRecurseGroups && Util.InRdpSubgroup(entry, triggerGroup))
                _groupUUIDs.Add(entry.ParentGroup.ParentGroup.Uuid);

            // Add unique list of included group UUIDs.
            _groupUUIDs.UnionWith(entrySettings.CpGroupUUIDs.Select(uuidString => new PwUuid(MemUtil.HexStringToByteArray(uuidString))));

            // Add unique list of excluded group UUIDs.
            _excludedGroupUUIDs.UnionWith(entrySettings.CpExcludedGroupUUIDs.Select(uuidString => new PwUuid(MemUtil.HexStringToByteArray(uuidString))));

            // Add recursive groups if enabled.
            if (entrySettings.CpRecurseGroups && _groupUUIDs.Count > 0)
                _groupUUIDs.UnionWith(_groupUUIDs
                    .Select(uuid => _host.Database.RootGroup.FindGroup(uuid, true))
                    .Where(group => group != null)
                    .SelectMany(childs => childs.GetFlatGroupList())
                    .Select(child => child.Uuid)
                    .ToList());

            _pes.Add(entry);
            _peSettings = entrySettings;

            AddRegEx();
        }

        /// <summary>
        /// Show <see cref="KprCredentialPickerForm"/> and return selected <see cref="PwEntry"/>.
        /// </summary>
        /// <param name="includeSelected">Switch for including selected <see cref="PwEntry"/> with credentials.</param>
        /// <returns><see cref="PwEntry"/>, <see langword="null"/></returns>
        public PwEntry GetCredentialEntry(bool includeSelected = false, bool resolveReferences = false)
        {
            PwEntry pe = null;

            // Remove excluded group UUIDs.
            _groupUUIDs.ExceptWith(_excludedGroupUUIDs);

            if (_groupUUIDs.Count > 0)
            {
                IEnumerable<PwEntry> accountEntries = null;
                var compiledRegex = _regexBuilder.Length > 0 ? new Regex(_regexBuilder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Singleline) : null;

                if (compiledRegex != null)
                    accountEntries = _groupUUIDs
                        .Select(uuid => _host.Database.RootGroup.FindGroup(uuid, true))
                        .Where(group => group != null)
                        .SelectMany(group => group.GetRdpAccountEntries(compiledRegex, resolveReferences ? _host.MainWindow.ActiveDatabase : null));

                if (includeSelected)
                {
                    var entries = _pes.Where(entry => !Util.IsEntryIgnored(entry) && !entry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty);
                    accountEntries = accountEntries == null ?
                        entries :
                        accountEntries.Concat(entries);
                }

                if (accountEntries != null && accountEntries.Any())
                {
                    // Create KprCredentialPickerForm with the entries found.
                    var frmCredPick = _credentialPickerForm.Value;
                    frmCredPick.RdpAccountEntries = accountEntries;
                    frmCredPick.ConnPE = _pes[0];

                    // Show KprCredentialPickerForm, get result and reset the dialog.
                    var res = frmCredPick.ShowDialog();
                    pe = frmCredPick.ReturnPE;
                    frmCredPick.Reset();
                    if (res != System.Windows.Forms.DialogResult.OK)
                        throw new OperationCanceledException();
                }
                else if (_pes.Count == 1)
                    pe = _pes[0];
            }

            return pe;
        }

        private void AddRegEx()
        {
            if (!_defaultRegexIncluded && _peSettings.CpIncludeDefaultRegex)
            {
                _defaultRegexIncluded = true;
                _regexBuilder
                    .Append(_regexBuilder.Length > 0 ? "|.*(" : ".*(")
                    .Append(_config.CredPickerRegExPre)
                    .Append(").*(")
                    .Append(_config.CredPickerRegExPost)
                    .Append(").*");
            }

            foreach (var regex in _peSettings.CpRegExPatterns)
            {
                if (_regexBuilder.Length > 0)
                    _regexBuilder.Append("|");
                _regexBuilder.Append(regex);
            }
        }
    }
}