using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
