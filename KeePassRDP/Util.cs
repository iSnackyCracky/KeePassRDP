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
        public static string ResolveReferences (PwEntry pe, PwDatabase pd, string field)
        {
            var ctx = new SprContext(pe, pd, SprCompileFlags.All);
            return SprEngine.Compile(pe.Strings.ReadSafe(field), ctx);
        }
    }
}
