using System.Windows.Forms;

namespace KeePassRDP
{
    public static class KprMenu
    {
        public const ulong DefaultOpenRdpConnectionShortcut = 131149;
        public const ulong DefaultOpenRdpConnectionAdminShortcut = 393293;

        public enum MenuItem
        {
            OpenRdpConnection,
            OpenRdpConnectionAdmin,
            OpenRdpConnectionNoCred,
            OpenRdpConnectionNoCredAdmin,
            IgnoreCredentials,
            Options
        }

        public static string GetText(MenuItem item)
        {
            switch (item)
            {
                case MenuItem.OpenRdpConnection:
                    return "Open RDP connection";
                case MenuItem.OpenRdpConnectionAdmin:
                    return "Open RDP connection (/admin)";
                case MenuItem.OpenRdpConnectionNoCred:
                    return "Open RDP connection without credentials";
                case MenuItem.OpenRdpConnectionNoCredAdmin:
                    return "Open RDP connection without credentials (/admin)";
                case MenuItem.IgnoreCredentials:
                    return "Ignore these credentials";
                case MenuItem.Options:
                    return "KeePassRDP Options";
                default:
                    return string.Empty;
            }
        }

        public static Keys GetShortcut(MenuItem item, KprConfig config)
        {
            switch (item)
            {
                case MenuItem.OpenRdpConnection:
                    return (Keys)config.ShortcutOpenRdpConnection;
                case MenuItem.OpenRdpConnectionAdmin:
                    return (Keys)config.ShortcutOpenRdpConnectionAdmin;
                case MenuItem.OpenRdpConnectionNoCred:
                    return (Keys)config.ShortcutOpenRdpConnectionNoCred;
                case MenuItem.OpenRdpConnectionNoCredAdmin:
                    return (Keys)config.ShortcutOpenRdpConnectionNoCredAdmin;
                case MenuItem.IgnoreCredentials:
                    return (Keys)config.ShortcutIgnoreCredentials;
                default:
                    return Keys.None;
            }
        }

    }
}
