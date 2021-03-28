using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeePassRDP
{
    class KprEntrySettings
    {
        public bool Ignore { get; set; }
        public bool CpRecurse { get; set; }
        public List<string> CpGroupUUIDs;
        public List<string> CpExcludeGroupUUIDs;

        public KprEntrySettings()
        {
            CpGroupUUIDs = new List<string>();
            CpExcludeGroupUUIDs = new List<string>();
        }
    }
}
