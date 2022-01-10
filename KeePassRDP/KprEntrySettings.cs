using KeePassLib.Security;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace KeePassRDP
{
    public class KprEntrySettings
    {
        [DefaultValue(false)]
        public bool Ignore { get; set; }
        [DefaultValue(true)]
        public bool UseCredpicker { get; set; }
        [DefaultValue(true)]
        public bool CpIncludeDefaultRegex { get; set; }
        [DefaultValue(true)]
        public bool CpRecurseGroups { get; set; }

        public bool ShouldSerializeCpGroupUUIDs() { return CpGroupUUIDs != null && CpGroupUUIDs.Count > 0; }
        public List<string> CpGroupUUIDs;
        public bool ShouldSerializeCpExcludedGroupUUIDs() { return CpExcludedGroupUUIDs != null && CpExcludedGroupUUIDs.Count > 0; }
        public List<string> CpExcludedGroupUUIDs;
        public bool ShouldSerializeCpRegExPatterns() { return CpRegExPatterns != null && CpRegExPatterns.Count > 0; }
        public List<string> CpRegExPatterns;
        public bool ShouldSerializeMstscParameters() { return MstscParameters != null && MstscParameters.Count > 0; }
        public List<string> MstscParameters;

        public KprEntrySettings()
        {
            Ignore = false;
            UseCredpicker = true;
            CpIncludeDefaultRegex = true;
            CpRecurseGroups = true;
            CpGroupUUIDs = new List<string>();
            CpExcludedGroupUUIDs = new List<string>();
            CpRegExPatterns = new List<string>();
            MstscParameters = new List<string>();
        }

        public ProtectedString ToProtectedJsonString()
        {
            var json = JsonConvert.SerializeObject(this, Util.jsonSerializerSettings);
            return new ProtectedString(false, json);
        }
    }
}
