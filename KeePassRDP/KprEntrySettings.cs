using KeePassLib.Security;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace KeePassRDP
{
    public class KprEntrySettings
    {
        [DefaultValue(false)]
        public bool Ignore { get; set; } = false;
        [DefaultValue(true)]
        public bool UseCredpicker { get; set; } = true;
        [DefaultValue(true)]
        public bool CpIncludeDefaultRegex { get; set; } = true;
        [DefaultValue(false)]
        public bool CpRecurseGroups { get; set; } = false;

        public bool ShouldSerializeCpGroupUUIDs() => CpGroupUUIDs != null && CpGroupUUIDs.Count > 0;
        public List<string> CpGroupUUIDs;
        public bool ShouldSerializeCpExcludedGroupUUIDs() => CpExcludedGroupUUIDs != null && CpExcludedGroupUUIDs.Count > 0;
        public List<string> CpExcludedGroupUUIDs;
        public bool ShouldSerializeCpRegExPatterns() => CpRegExPatterns != null && CpRegExPatterns.Count > 0;
        public List<string> CpRegExPatterns;
        public bool ShouldSerializeMstscParameters() => MstscParameters != null && MstscParameters.Count > 0;
        public List<string> MstscParameters;

        public KprEntrySettings()
        {
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
