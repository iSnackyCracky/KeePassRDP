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

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace KeePassRDP
{
    public static class KprMenu
    {
        #region Constants
        public const Keys DefaultOpenRdpConnectionShortcut = Keys.Control | Keys.M; //131149;
        public const Keys DefaultOpenRdpConnectionAdminShortcut = Keys.Control | Keys.Shift | Keys.M; //393293;

        public const MenuItem DefaultToolbarItems = MenuItem.OpenRdpConnection | MenuItem.OpenRdpConnectionNoCred;
        //public const MenuItem DefaultContextMenuItems = DefaultToolbarItems | MenuItem.OpenRdpConnectionAdmin | MenuItem.OpenRdpConnectionNoCredAdmin | MenuItem.IgnoreCredentials;
        public const MenuItem DefaultContextMenuItems = DefaultToolbarItems | MenuItem.ShadowSession | MenuItem.IgnoreCredentials;
        #endregion

        internal static readonly ReadOnlyCollection<MenuItem> MenuItemValues = Enum.GetValues(typeof(MenuItem))
                        .Cast<MenuItem>()
                        .Where(menu => menu > MenuItem.Empty && menu < MenuItem.Options).ToList().AsReadOnly();

        [Flags]
        public enum MenuItem : byte
        {
            Empty = 0,
            OpenRdpConnection = 1,
            OpenRdpConnectionAdmin = 2,
            OpenRdpConnectionNoCred = 4,
            OpenRdpConnectionNoCredAdmin = 8,
            ShadowSession = 16,
            ShadowSessionNoCred = 32,
            IgnoreCredentials = 64,
            Options = byte.MaxValue
        }

        public static string GetText(this MenuItem menuItem)
        {
            switch (menuItem)
            {
                case MenuItem.OpenRdpConnection:
                    return KprResourceManager.Instance["Open RDP connection"];
                case MenuItem.OpenRdpConnectionAdmin:
                    return KprResourceManager.Instance["Open RDP connection (/admin)"];
                case MenuItem.OpenRdpConnectionNoCred:
                    return KprResourceManager.Instance["Open RDP connection without credentials"];
                case MenuItem.OpenRdpConnectionNoCredAdmin:
                    return KprResourceManager.Instance["Open RDP connection without credentials (/admin)"];
                case MenuItem.ShadowSession:
                    return KprResourceManager.Instance["Shadow session over RDP connection (/shadow)"];
                case MenuItem.ShadowSessionNoCred:
                    return KprResourceManager.Instance["Shadow session over RDP connection without credentials (/shadow)"];
                case MenuItem.IgnoreCredentials:
                    return KprResourceManager.Instance["Ignore entry credentials"];
                case MenuItem.Options:
                    return KprResourceManager.Instance["KeePassRDP Options"];
                default:
                    return string.Empty;
            }
        }
    }
}