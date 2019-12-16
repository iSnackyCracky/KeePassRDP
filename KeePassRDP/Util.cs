using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using KeePass;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Security;

namespace KeePassRDP
{
    public static class Util
    {
        public const string IgnoreEntryString = "rdpignore";

        /// <summary>
        /// Checks if the ParentGroup of a PwEntry is named "RDP".
        /// </summary>
        /// <param name="pe"></param>
        /// <returns></returns>
        public static bool InRdpSubgroup(PwEntry pe)
        {
            PwGroup pg = pe.ParentGroup;
            return pg.Name == "RDP";
        }

        /// <summary>
        /// Uses the KeePass SprEngine to resolve field references.
        /// </summary>
        /// <param name="pe"></param>
        /// <param name="pd"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string ResolveReferences (PwEntry pe, PwDatabase pd, string field)
        {
            var ctx = new SprContext(pe, pd, SprCompileFlags.All);
            return SprEngine.Compile(pe.Strings.ReadSafe(field), ctx);
        }

        /// <summary>
        /// Checks if a given PwEntry has the "rdpignore-flag" set
        /// </summary>
        /// <param name="pe"></param>
        /// <returns></returns>
        public static bool IsEntryIgnored(PwEntry pe)
        {
            // Does a CustomField "rdpignore" exist and is the value NOT set to "false"?
            if (pe.Strings.Exists(IgnoreEntryString) && !(pe.Strings.ReadSafe(IgnoreEntryString) == Boolean.FalseString))
            {
                return true;
            }
            // Does the entry title contain "[rdpignore]"?
            else if (Regex.IsMatch(pe.Strings.ReadSafe(PwDefs.TitleField), ".*\\[" + IgnoreEntryString + "\\].*", RegexOptions.IgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
