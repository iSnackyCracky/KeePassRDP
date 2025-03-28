/*
 *  Copyright (C) 2018 - 2025 iSnackyCracky, NETertainer
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
using KeePassRDP.Generator;
using KeePassRDP.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace KeePassRDP
{
    /// <summary>
    /// KeePassRDP settings for a single <see cref="KeePassLib.PwEntry"/>.
    /// </summary>
    public class KprEntrySettings : IDisposable
    {
        public static readonly KprEntrySettings Empty = new KprEntrySettings(true);

        public enum Inheritance
        {
            // Hide all settings from children
            Hide = CheckState.Unchecked,
            // Force all settings on children
            Force = CheckState.Checked,
            // Allow inheritance from parent settings
            Default = CheckState.Indeterminate
        }

        [JsonIgnore]
        public bool IsReadOnly { get { return _isReadOnly; } }

        [DefaultValue(false)]
        public bool Ignore { get { return _ignore; } set { if (_isReadOnly) throw new InvalidOperationException(); _ignore = value; } }

        [DefaultValue(true)]
        public bool UseCredpicker { get { return _useCredpicker; } set { if (_isReadOnly) throw new InvalidOperationException(); _useCredpicker = value; } }

        [DefaultValue(null)]
        public RdpFile RdpFile { get { return _rdpFile; } set { if (_isReadOnly) throw new InvalidOperationException(); _rdpFile = value; } }
        public bool ShouldSerializeRdpFile() { return _rdpFile != null && !_rdpFile.IsEmpty; }

        [DefaultValue(true)]
        public bool CpRecurseGroups { get { return _cpRecurseGroups; } set { if (_isReadOnly) throw new InvalidOperationException(); _cpRecurseGroups = value; } }

        [DefaultValue(true)]
        public bool CpIncludeDefaultRegex { get { return _cpIncludeDefaultRegex; } set { if (_isReadOnly) throw new InvalidOperationException(); _cpIncludeDefaultRegex = value; } }

        [DefaultValue(true)]
        public bool IncludeDefaultParameters { get { return _includeDefaultParameters; } set { if (_isReadOnly) throw new InvalidOperationException(); _includeDefaultParameters = value; } }

        [DefaultValue(false)]
        public bool ForceLocalUser { get { return _forceLocalUser; } set { if (_isReadOnly) throw new InvalidOperationException(); _forceLocalUser = value; } }

        [DefaultValue(false)]
        public bool ForceUpn { get { return _forceUpn; } set { if (_isReadOnly) throw new InvalidOperationException(); _forceUpn = value; } }

        [DefaultValue(false)]
        public bool RetryOnce { get { return _retryOnce; } set { if (_isReadOnly) throw new InvalidOperationException(); _retryOnce = value; } }

        [DefaultValue(Inheritance.Default)]
        public Inheritance Inherit { get { return _inherit; } set { if (_isReadOnly) throw new InvalidOperationException(); _inherit = value; } }

        public ICollection<string> CpGroupUUIDs
        {
            get { return _isReadOnly && !_cpGroupUUIDs.IsReadOnly ? _cpGroupUUIDs.AsReadOnly() : _cpGroupUUIDs; }
            set { if (_isReadOnly) throw new InvalidOperationException(); _cpGroupUUIDs = new HashSet<string>(value, StringComparer.OrdinalIgnoreCase); }
        }

        public ICollection<string> CpExcludedGroupUUIDs
        {
            get { return _isReadOnly && !_cpExcludedGroupUUIDs.IsReadOnly ? _cpExcludedGroupUUIDs.AsReadOnly() : _cpExcludedGroupUUIDs; }
            set { if (_isReadOnly) throw new InvalidOperationException(); _cpGroupUUIDs = new HashSet<string>(value, StringComparer.OrdinalIgnoreCase); }
        }

        public ICollection<string> CpRegExPatterns
        {
            get { return _isReadOnly && !_cpRegExPatterns.IsReadOnly ? _cpRegExPatterns.AsReadOnly() : _cpRegExPatterns; }
            set { if (_isReadOnly) throw new InvalidOperationException(); _cpRegExPatterns = new List<string>(value); }
        }

        public ICollection<string> MstscParameters
        {
            get { return _isReadOnly && !_mstscParameters.IsReadOnly ? _mstscParameters.AsReadOnly() : _mstscParameters; }
            set { if (_isReadOnly) throw new InvalidOperationException(); _mstscParameters = new List<string>(value); }
        }

        public bool ShouldSerializeCpGroupUUIDs() { return _cpGroupUUIDs != null && _cpGroupUUIDs.Count > 0; }
        public bool ShouldSerializeCpExcludedGroupUUIDs() { return _cpExcludedGroupUUIDs != null && _cpExcludedGroupUUIDs.Count > 0; }
        public bool ShouldSerializeCpRegExPatterns() { return _cpRegExPatterns != null && _cpRegExPatterns.Count > 0; }
        public bool ShouldSerializeMstscParameters() { return _mstscParameters != null && _mstscParameters.Count > 0; }

        private ISet<string> _cpGroupUUIDs;
        private ISet<string> _cpExcludedGroupUUIDs;
        private IList<string> _cpRegExPatterns;
        private IList<string> _mstscParameters;

        private bool _isReadOnly;
        private bool _ignore;
        private bool _useCredpicker;
        private RdpFile _rdpFile;
        private bool _cpRecurseGroups;
        private bool _cpIncludeDefaultRegex;
        private bool _includeDefaultParameters;
        private bool _forceLocalUser;
        private bool _forceUpn;
        private bool _retryOnce;
        private Inheritance _inherit;

        public KprEntrySettings()
        {
            _isReadOnly = false;
            _cpGroupUUIDs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _cpExcludedGroupUUIDs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _cpRegExPatterns = new List<string>();
            _mstscParameters = new List<string>();
            _ignore = false;
            _useCredpicker = true;
            _rdpFile = null;
            _cpRecurseGroups = true;
            _cpIncludeDefaultRegex = true;
            _includeDefaultParameters = true;
            _forceLocalUser = false;
            _forceUpn = false;
            _retryOnce = false;
            _inherit = Inheritance.Default;
        }

        private KprEntrySettings(bool readOnly) : this()
        {
            if (readOnly)
            {
                _isReadOnly = true;
                _cpGroupUUIDs = _cpGroupUUIDs.AsReadOnly();
                _cpExcludedGroupUUIDs = _cpExcludedGroupUUIDs.AsReadOnly();
                _cpRegExPatterns = _cpRegExPatterns.AsReadOnly();
                _mstscParameters = _mstscParameters.AsReadOnly();
            }
        }

        internal KprEntrySettings(KprEntrySettings kprEntrySettings, bool readOnly = false)
        {
            _isReadOnly = readOnly;
            _cpGroupUUIDs = new HashSet<string>(kprEntrySettings._cpGroupUUIDs, StringComparer.OrdinalIgnoreCase);
            _cpExcludedGroupUUIDs = new HashSet<string>(kprEntrySettings._cpExcludedGroupUUIDs, StringComparer.OrdinalIgnoreCase);
            _cpRegExPatterns = new List<string>(kprEntrySettings._cpRegExPatterns);
            _mstscParameters = new List<string>(kprEntrySettings._mstscParameters);
            if (readOnly)
            {
                _cpGroupUUIDs = _cpGroupUUIDs.AsReadOnly();
                _cpExcludedGroupUUIDs = _cpExcludedGroupUUIDs.AsReadOnly();
                _cpRegExPatterns = _cpRegExPatterns.AsReadOnly();
                _mstscParameters = _mstscParameters.AsReadOnly();
            }
            _ignore = kprEntrySettings._ignore;
            _useCredpicker = kprEntrySettings._useCredpicker;
            _rdpFile = kprEntrySettings._rdpFile;
            _cpRecurseGroups = kprEntrySettings._cpRecurseGroups;
            _cpIncludeDefaultRegex = kprEntrySettings._cpIncludeDefaultRegex;
            _includeDefaultParameters = kprEntrySettings._includeDefaultParameters;
            _forceLocalUser = kprEntrySettings._forceLocalUser;
            _forceUpn = kprEntrySettings._forceUpn;
            _retryOnce = kprEntrySettings._retryOnce;
            _inherit = kprEntrySettings._inherit;
        }

        public KprEntrySettings AsReadOnly()
        {
            if (_isReadOnly)
                return this;
            return new KprEntrySettings(this, true);
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
            if (_rdpFile != null)
                _rdpFile.Dispose();
        }

        public override string ToString()
        {
            if (_rdpFile != null && _rdpFile.IsEmpty)
            {
                using (_rdpFile)
                    _rdpFile = null;
            }

            return Regex.Replace(
                JsonConvert.SerializeObject(this, Util.JsonSerializerSettings),
                "^{}$",
                string.Empty,
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        }

        public static implicit operator string(KprEntrySettings kprEntrySettings)
        {
            return kprEntrySettings.ToString();
        }
    }
}