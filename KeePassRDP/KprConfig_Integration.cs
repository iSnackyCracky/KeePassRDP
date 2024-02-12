/*
 *  Copyright (C) 2018 - 2024 iSnackyCracky, NETertainer
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

using KeePass.Util.Spr;
using System.Windows.Forms;

namespace KeePassRDP
{
    public partial class KprConfig
    {
        private bool _cachedKeePassSprCompileFlags = false;
        private SprCompileFlags _cachedKeePassSprCompileFlagsValue = SprCompileFlags.All;
        public SprCompileFlags KeePassSprCompileFlags
        {
            get
            {
                return EnumGetter(KeePassSprCompileFlagsKey, ref _cachedKeePassSprCompileFlags, ref _cachedKeePassSprCompileFlagsValue, SprCompileFlags.All);
            }
            set
            {
                EnumSetter(KeePassSprCompileFlagsKey, value, ref _cachedKeePassSprCompileFlags, ref _cachedKeePassSprCompileFlagsValue, SprCompileFlags.All);
            }
        }

        private bool _cachedKeePassConfirmOnClose = false;
        private bool _cachedKeePassConfirmOnCloseValue = true;
        public bool KeePassConfirmOnClose
        {
            get
            {
                return BoolGetter(KeePassConfirmOnCloseKey, ref _cachedKeePassConfirmOnClose, ref _cachedKeePassConfirmOnCloseValue, true);
            }
            set
            {
                BoolSetter(KeePassConfirmOnCloseKey, value, ref _cachedKeePassConfirmOnClose, ref _cachedKeePassConfirmOnCloseValue, true);
            }
        }

        private bool _cachedKeePassConnectToAll = false;
        private bool _cachedKeePassConnectToAllValue = true;
        public bool KeePassConnectToAll
        {
            get
            {
                return BoolGetter(KeePassConnectToAllKey, ref _cachedKeePassConnectToAll, ref _cachedKeePassConnectToAllValue, true);
            }
            set
            {
                BoolSetter(KeePassConnectToAllKey, value, ref _cachedKeePassConnectToAll, ref _cachedKeePassConnectToAllValue, true);
            }
        }


        private bool _cachedKeePassAlwaysConfirm = false;
        private bool _cachedKeePassAlwaysConfirmValue = true;
        public bool KeePassAlwaysConfirm
        {
            get
            {
                return BoolGetter(KeePassAlwaysConfirmKey, ref _cachedKeePassAlwaysConfirm, ref _cachedKeePassAlwaysConfirmValue);
            }
            set
            {
                BoolSetter(KeePassAlwaysConfirmKey, value, ref _cachedKeePassAlwaysConfirm, ref _cachedKeePassAlwaysConfirmValue);
            }
        }

        private bool _cachedKeePassContextMenuOnScreenKey = false;
        private bool _cachedKeePassContextMenuOnScreenKeyValue = true;
        public bool KeePassContextMenuOnScreen
        {
            get
            {
                return BoolGetter(KeePassContextMenuOnScreenKey, ref _cachedKeePassContextMenuOnScreenKey, ref _cachedKeePassContextMenuOnScreenKeyValue, true);
            }
            set
            {
                BoolSetter(KeePassContextMenuOnScreenKey, value, ref _cachedKeePassContextMenuOnScreenKey, ref _cachedKeePassContextMenuOnScreenKeyValue, true);
            }
        }

        private bool _cachedKeePassHotkeysRegisterLast = false;
        private bool _cachedKeePassHotkeysRegisterLastValue = false;
        public bool KeePassHotkeysRegisterLast
        {
            get
            {
                return BoolGetter(KeePassHotkeysRegisterLastKey, ref _cachedKeePassHotkeysRegisterLast, ref _cachedKeePassHotkeysRegisterLastValue);
            }
            set
            {
                BoolSetter(KeePassHotkeysRegisterLastKey, value, ref _cachedKeePassHotkeysRegisterLast, ref _cachedKeePassHotkeysRegisterLastValue);
            }
        }

        private bool _cachedKeePassContextMenuItems = false;
        private KprMenu.MenuItem _cachedKeePassContextMenuItemsValue = KprMenu.DefaultContextMenuItems;
        public KprMenu.MenuItem KeePassContextMenuItems
        {
            get
            {
                return EnumGetter(KeePassContextMenuItemsKey, ref _cachedKeePassContextMenuItems, ref _cachedKeePassContextMenuItemsValue, KprMenu.DefaultContextMenuItems);
            }
            set
            {
                EnumSetter(KeePassContextMenuItemsKey, value, ref _cachedKeePassContextMenuItems, ref _cachedKeePassContextMenuItemsValue, KprMenu.DefaultContextMenuItems);
            }
        }

        private bool _cachedKeePassToolbarItems = false;
        private KprMenu.MenuItem _cachedKeePassToolbarItemsValue = KprMenu.DefaultToolbarItems;
        public KprMenu.MenuItem KeePassToolbarItems
        {
            get
            {
                return EnumGetter(KeePassToolbarItemsKey, ref _cachedKeePassToolbarItems, ref _cachedKeePassToolbarItemsValue, KprMenu.DefaultToolbarItems);
            }
            set
            {
                EnumSetter(KeePassToolbarItemsKey, value, ref _cachedKeePassToolbarItems, ref _cachedKeePassToolbarItemsValue, KprMenu.DefaultToolbarItems);
            }
        }

        private bool _cachedCredVaultUseWindows = false;
        private bool _cachedCredVaultUseWindowsValue = true;
        public bool CredVaultUseWindows
        {
            get
            {
                return BoolGetter(CredVaultUseWindowsKey, ref _cachedCredVaultUseWindows, ref _cachedCredVaultUseWindowsValue, true);
            }
            set
            {
                BoolSetter(CredVaultUseWindowsKey, value, ref _cachedCredVaultUseWindows, ref _cachedCredVaultUseWindowsValue, true);
            }
        }

        private bool _cachedCredVaultOverwriteExisting = false;
        private bool _cachedCredVaultOverwriteExistingValue = false;
        public bool CredVaultOverwriteExisting
        {
            get
            {
                return BoolGetter(CredVaultOverwriteExistingKey, ref _cachedCredVaultOverwriteExisting, ref _cachedCredVaultOverwriteExistingValue);
            }
            set
            {
                BoolSetter(CredVaultOverwriteExistingKey, value, ref _cachedCredVaultOverwriteExisting, ref _cachedCredVaultOverwriteExistingValue);
            }
        }

        private bool _cachedCredVaultRemoveOnExit = false;
        private bool _cachedCredVaultRemoveOnExitValue = true;
        public bool CredVaultRemoveOnExit
        {
            get
            {
                return BoolGetter(CredVaultRemoveOnExitKey, ref _cachedCredVaultRemoveOnExit, ref _cachedCredVaultRemoveOnExitValue, true);
            }
            set
            {
                BoolSetter(CredVaultRemoveOnExitKey, value, ref _cachedCredVaultRemoveOnExit, ref _cachedCredVaultRemoveOnExitValue, true);
            }
        }

        private bool _cachedCredVaultTtl = false;
        private long _cachedCredVaultTtlValue = 10L;
        public int CredVaultTtl
        {
            get
            {
                return (int)LongGetter(CredVaultTtlKey, ref _cachedCredVaultTtl, ref _cachedCredVaultTtlValue, 10L);
            }
            set
            {
                LongSetter(CredVaultTtlKey, value, ref _cachedCredVaultTtl, ref _cachedCredVaultTtlValue, 10L);
            }
        }

        private bool _cachedCredVaultAdaptiveTtl = false;
        private bool _cachedCredVaultAdaptiveTtlValue = true;
        public bool CredVaultAdaptiveTtl
        {
            get
            {
                return BoolGetter(CredVaultAdaptiveTtlKey, ref _cachedCredVaultAdaptiveTtl, ref _cachedCredVaultAdaptiveTtlValue, true);
            }
            set
            {
                BoolSetter(CredVaultAdaptiveTtlKey, value, ref _cachedCredVaultAdaptiveTtl, ref _cachedCredVaultAdaptiveTtlValue, true);
            }
        }

        public Keys ShortcutOpenRdpConnection
        {
            get { return (Keys)_config.GetLong(ShortcutOpenRdpConnectionKey, (long)KprMenu.DefaultOpenRdpConnectionShortcut); }
            set
            {
                if (value == KprMenu.DefaultOpenRdpConnectionShortcut)
                {
                    if (!string.IsNullOrEmpty(_config.GetString(ShortcutOpenRdpConnectionKey)))
                        _config.SetString(ShortcutOpenRdpConnectionKey, null);
                }
                else
                    _config.SetLong(ShortcutOpenRdpConnectionKey, (long)value);
            }
        }

        public Keys ShortcutOpenRdpConnectionAdmin
        {
            get { return (Keys)_config.GetLong(ShortcutOpenRdpConnectionAdminKey, (long)KprMenu.DefaultOpenRdpConnectionAdminShortcut); }
            set
            {
                if (value == KprMenu.DefaultOpenRdpConnectionAdminShortcut)
                {
                    if (!string.IsNullOrEmpty(_config.GetString(ShortcutOpenRdpConnectionAdminKey)))
                        _config.SetString(ShortcutOpenRdpConnectionAdminKey, null);
                }
                else
                    _config.SetLong(ShortcutOpenRdpConnectionAdminKey, (long)value);
            }
        }

        public Keys ShortcutOpenRdpConnectionNoCred
        {
            get { return (Keys)_config.GetLong(ShortcutOpenRdpConnectionNoCredKey, (long)Keys.None); }
            set
            {
                if (value == Keys.None)
                {
                    if (!string.IsNullOrEmpty(_config.GetString(ShortcutOpenRdpConnectionNoCredKey)))
                        _config.SetString(ShortcutOpenRdpConnectionNoCredKey, null);
                }
                else
                    _config.SetLong(ShortcutOpenRdpConnectionNoCredKey, (long)value);
            }
        }

        public Keys ShortcutOpenRdpConnectionNoCredAdmin
        {
            get { return (Keys)_config.GetLong(ShortcutOpenRdpConnectionNoCredAdminKey, (long)Keys.None); }
            set
            {
                if (value == Keys.None)
                {
                    if (!string.IsNullOrEmpty(_config.GetString(ShortcutOpenRdpConnectionNoCredAdminKey)))
                        _config.SetString(ShortcutOpenRdpConnectionNoCredAdminKey, null);
                }
                else
                    _config.SetLong(ShortcutOpenRdpConnectionNoCredAdminKey, (long)value);
            }
        }

        public Keys ShortcutIgnoreCredentials
        {
            get { return (Keys)_config.GetLong(ShortcutIgnoreCredentialsKey, (long)Keys.None); }
            set
            {
                if (value == Keys.None)
                {
                    if (!string.IsNullOrEmpty(_config.GetString(ShortcutIgnoreCredentialsKey)))
                        _config.SetString(ShortcutIgnoreCredentialsKey, null);
                }
                else
                    _config.SetLong(ShortcutIgnoreCredentialsKey, (long)value);
            }
        }
    }
}