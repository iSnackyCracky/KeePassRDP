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
        public const string ToolbarConnectBtnId = "KprConnect";

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
            if (pe.Strings.Exists(IgnoreEntryString) && !(pe.Strings.ReadSafe(IgnoreEntryString).ToLower() == Boolean.FalseString.ToLower()))
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

        /// <summary>
        /// Toggles the "rdpignore-flag" of a given PwEntry
        /// </summary>
        /// <param name="pe"></param>
        public static void ToggleEntryIgnored(PwEntry pe)
        {
            var pTrue = new ProtectedString(false, Boolean.TrueString.ToLower());
            var pFalse = new ProtectedString(false, Boolean.FalseString.ToLower());

            // Does a CustomField "rdpignore" exist?
            if (pe.Strings.Exists(IgnoreEntryString))
            {
                // Is the CustomField value set to "false"?
                if (pe.Strings.ReadSafe(IgnoreEntryString).ToLower() == Boolean.FalseString.ToLower())
                {
                    // Then set it to "true".
                    pe.Strings.Set(IgnoreEntryString, pTrue);
                } else
                {
                    // Else set it to "false" now.
                    pe.Strings.Set(IgnoreEntryString, pFalse);
                }
            }
            // Does the entry title contain "[rdpignore]"?
            else if (Regex.IsMatch(pe.Strings.ReadSafe(PwDefs.TitleField), ".*\\[" + IgnoreEntryString + "\\].*", RegexOptions.IgnoreCase))
            {
                // Then remove the flag from the title.
                string newTitle = Regex.Replace(pe.Strings.ReadSafe(PwDefs.TitleField), "\\[" + IgnoreEntryString + "\\]", "", RegexOptions.IgnoreCase);
                pe.Strings.Set(PwDefs.TitleField, new ProtectedString(false, newTitle));
            }
            // Else the entry currently has no "rdpignore-flags" set
            else
            {
                // So set the CustomField with "true" value.
                pe.Strings.Set(IgnoreEntryString, pTrue);
            }
        }
    }
}
