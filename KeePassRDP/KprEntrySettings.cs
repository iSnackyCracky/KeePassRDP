/*
 *  Copyright (C) 2018 - 2023 iSnackyCracky, NETertainer
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

using KeePassRDP.Extensions;
using KeePassRDP.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace KeePassRDP
{
    /// <summary>
    /// KeePassRDP settings for a single <see cref="KeePassLib.PwEntry"/>.
    /// </summary>
    public class KprEntrySettings : IDisposable
    {
        public static readonly KprEntrySettings Empty = new KprEntrySettings(true);

        [JsonIgnore]
        public bool IsReadOnly { get { return _isReadOnly; } set { if (_isReadOnly) throw new InvalidOperationException(); _isReadOnly = value; } }

        [DefaultValue(false)]
        public bool Ignore { get { return _ignore; } set { if (_isReadOnly) throw new InvalidOperationException(); _ignore = value; } }

        [DefaultValue(true)]
        public bool UseCredpicker { get { return _useCredpicker; } set { if (_isReadOnly) throw new InvalidOperationException(); _useCredpicker = value; } }

        [DefaultValue(true)]
        public bool CpRecurseGroups { get { return _cpRecurseGroups; } set { if (_isReadOnly) throw new InvalidOperationException(); _cpRecurseGroups = value; } }

        [DefaultValue(true)]
        public bool CpIncludeDefaultRegex { get { return _cpIncludeDefaultRegex; } set { if (_isReadOnly) throw new InvalidOperationException(); _cpIncludeDefaultRegex = value; } }

        [DefaultValue(true)]
        public bool IncludeDefaultParameters { get { return _includeDefaultParameters; } set { if (_isReadOnly) throw new InvalidOperationException(); _includeDefaultParameters = value; } }

        [DefaultValue(false)]
        public bool ForceLocalUser { get { return _forceLocalUser; } set { if (_isReadOnly) throw new InvalidOperationException(); _forceLocalUser = value; } }

        public bool ShouldSerializeCpGroupUUIDs() { return _cpGroupUUIDs != null && _cpGroupUUIDs.Count > 0; }
        public ICollection<string> CpGroupUUIDs { get { return IsReadOnly && !_cpGroupUUIDs.IsReadOnly ? _cpGroupUUIDs.AsReadOnly() : _cpGroupUUIDs; } }

        public bool ShouldSerializeCpExcludedGroupUUIDs() { return _cpExcludedGroupUUIDs != null && _cpExcludedGroupUUIDs.Count > 0; }
        public ICollection<string> CpExcludedGroupUUIDs { get { return IsReadOnly && !_cpExcludedGroupUUIDs.IsReadOnly ? _cpExcludedGroupUUIDs.AsReadOnly() : _cpExcludedGroupUUIDs; } }

        public bool ShouldSerializeCpRegExPatterns() { return _cpRegExPatterns != null && _cpRegExPatterns.Count > 0; }
        public ICollection<string> CpRegExPatterns { get { return IsReadOnly && !_cpRegExPatterns.IsReadOnly ? _cpRegExPatterns.AsReadOnly() : _cpRegExPatterns; } }

        public bool ShouldSerializeMstscParameters() { return _mstscParameters != null && _mstscParameters.Count > 0; }
        public ICollection<string> MstscParameters { get { return IsReadOnly && !_mstscParameters.IsReadOnly ? _mstscParameters.AsReadOnly() : _mstscParameters; } }

        private readonly ISet<string> _cpGroupUUIDs;
        private readonly ISet<string> _cpExcludedGroupUUIDs;
        private readonly IList<string> _cpRegExPatterns;
        private readonly IList<string> _mstscParameters;

        private bool _isReadOnly;
        private bool _ignore;
        private bool _useCredpicker;
        private bool _cpRecurseGroups;
        private bool _cpIncludeDefaultRegex;
        private bool _includeDefaultParameters;
        private bool _forceLocalUser;

        public KprEntrySettings()
        {
            _cpGroupUUIDs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _cpExcludedGroupUUIDs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _cpRegExPatterns = new List<string>();
            _mstscParameters = new List<string>();
            _isReadOnly = false;
            _ignore = false;
            _useCredpicker = true;
            _cpRecurseGroups = true;
            _cpIncludeDefaultRegex = true;
            _includeDefaultParameters = true;
            _forceLocalUser = false;
        }

        private KprEntrySettings(bool readOnly) : this()
        {
            if (readOnly)
            {
                _cpGroupUUIDs = _cpGroupUUIDs.AsReadOnly();
                _cpExcludedGroupUUIDs = _cpExcludedGroupUUIDs.AsReadOnly();
                _cpRegExPatterns = _cpRegExPatterns.AsReadOnly();
                _mstscParameters = _mstscParameters.AsReadOnly();
                _isReadOnly = true;
            }
        }

        public override string ToString()
        {
            return Regex.Replace(
                JsonConvert.SerializeObject(this, Util.JsonSerializerSettings),
                "^{}$",
                string.Empty,
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        }

        public void Clear()
        {
            if (!_cpGroupUUIDs.IsReadOnly)
                _cpGroupUUIDs.Clear();
            if (!_cpExcludedGroupUUIDs.IsReadOnly)
                _cpExcludedGroupUUIDs.Clear();
            if (!_cpRegExPatterns.IsReadOnly)
                _cpRegExPatterns.Clear();
            if (!_mstscParameters.IsReadOnly)
                _mstscParameters.Clear();
        }

        public void Dispose()
        {
            Clear();
        }

        public static implicit operator string(KprEntrySettings kprEntrySettings)
        {
            return kprEntrySettings.ToString();
        }
    }
}