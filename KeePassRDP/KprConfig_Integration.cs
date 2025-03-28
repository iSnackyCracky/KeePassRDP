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

using KeePass.Util.Spr;
using System.Windows.Forms;

namespace KeePassRDP
{
    public partial class KprConfig
    {
        private bool _cachedKeePassSprCompileFlags = false;
        private SprCompileFlags _cachedKeePassSprCompileFlagsValue = Default.KeePassSprCompileFlags;
        public SprCompileFlags KeePassSprCompileFlags
        {
            get
            {
                return EnumGetter(Key.KeePassSprCompileFlags, ref _cachedKeePassSprCompileFlags, ref _cachedKeePassSprCompileFlagsValue, Default.KeePassSprCompileFlags);
            }
            set
            {
                EnumSetter(Key.KeePassSprCompileFlags, value, ref _cachedKeePassSprCompileFlags, ref _cachedKeePassSprCompileFlagsValue, Default.KeePassSprCompileFlags);
            }
        }

        private bool _cachedKeePassConfirmOnClose = false;
        private bool _cachedKeePassConfirmOnCloseValue = Default.KeePassConfirmOnClose;
        public bool KeePassConfirmOnClose
        {
            get
            {
                return BoolGetter(Key.KeePassConfirmOnClose, ref _cachedKeePassConfirmOnClose, ref _cachedKeePassConfirmOnCloseValue, Default.KeePassConfirmOnClose);
            }
            set
            {
                BoolSetter(Key.KeePassConfirmOnClose, value, ref _cachedKeePassConfirmOnClose, ref _cachedKeePassConfirmOnCloseValue, Default.KeePassConfirmOnClose);
            }
        }

        private bool _cachedKeePassConnectToAll = false;
        private bool _cachedKeePassConnectToAllValue = Default.KeePassConnectToAll;
        public bool KeePassConnectToAll
        {
            get
            {
                return BoolGetter(Key.KeePassConnectToAll, ref _cachedKeePassConnectToAll, ref _cachedKeePassConnectToAllValue, Default.KeePassConnectToAll);
            }
            set
            {
                BoolSetter(Key.KeePassConnectToAll, value, ref _cachedKeePassConnectToAll, ref _cachedKeePassConnectToAllValue, Default.KeePassConnectToAll);
            }
        }


        private bool _cachedKeePassAlwaysConfirm = false;
        private bool _cachedKeePassAlwaysConfirmValue = Default.KeePassAlwaysConfirm;
        public bool KeePassAlwaysConfirm
        {
            get
            {
                return BoolGetter(Key.KeePassAlwaysConfirm, ref _cachedKeePassAlwaysConfirm, ref _cachedKeePassAlwaysConfirmValue, Default.KeePassAlwaysConfirm);
            }
            set
            {
                BoolSetter(Key.KeePassAlwaysConfirm, value, ref _cachedKeePassAlwaysConfirm, ref _cachedKeePassAlwaysConfirmValue, Default.KeePassAlwaysConfirm);
            }
        }


        private bool _cacheKeePassDefaultEntryAction = false;
        private bool _cacheKeePassDefaultEntryActionValue = Default.KeePassDefaultEntryAction;
        public bool KeePassDefaultEntryAction
        {
            get
            {
                return BoolGetter(Key.KeePassDefaultEntryAction, ref _cacheKeePassDefaultEntryAction, ref _cacheKeePassDefaultEntryActionValue, Default.KeePassDefaultEntryAction);
            }
            set
            {
                BoolSetter(Key.KeePassDefaultEntryAction, value, ref _cacheKeePassDefaultEntryAction, ref _cacheKeePassDefaultEntryActionValue, Default.KeePassDefaultEntryAction);
            }
        }

        private bool _cachedKeePassContextMenuOnScreenKey = false;
        private bool _cachedKeePassContextMenuOnScreenKeyValue = Default.KeePassContextMenuOnScreen;
        public bool KeePassContextMenuOnScreen
        {
            get
            {
                return BoolGetter(Key.KeePassContextMenuOnScreen, ref _cachedKeePassContextMenuOnScreenKey, ref _cachedKeePassContextMenuOnScreenKeyValue, Default.KeePassContextMenuOnScreen);
            }
            set
            {
                BoolSetter(Key.KeePassContextMenuOnScreen, value, ref _cachedKeePassContextMenuOnScreenKey, ref _cachedKeePassContextMenuOnScreenKeyValue, Default.KeePassContextMenuOnScreen);
            }
        }

        private bool _cachedKeePassHotkeysRegisterLast = false;
        private bool _cachedKeePassHotkeysRegisterLastValue = Default.KeePassHotkeysRegisterLast;
        public bool KeePassHotkeysRegisterLast
        {
            get
            {
                return BoolGetter(Key.KeePassHotkeysRegisterLast, ref _cachedKeePassHotkeysRegisterLast, ref _cachedKeePassHotkeysRegisterLastValue, Default.KeePassHotkeysRegisterLast);
            }
            set
            {
                BoolSetter(Key.KeePassHotkeysRegisterLast, value, ref _cachedKeePassHotkeysRegisterLast, ref _cachedKeePassHotkeysRegisterLastValue, Default.KeePassHotkeysRegisterLast);
            }
        }

        private bool _cachedKeePassContextMenuItems = false;
        private KprMenu.MenuItem _cachedKeePassContextMenuItemsValue = Default.KeePassContextMenuItems;
        public KprMenu.MenuItem KeePassContextMenuItems
        {
            get
            {
                return EnumGetter(Key.KeePassContextMenuItems, ref _cachedKeePassContextMenuItems, ref _cachedKeePassContextMenuItemsValue, Default.KeePassContextMenuItems);
            }
            set
            {
                EnumSetter(Key.KeePassContextMenuItems, value, ref _cachedKeePassContextMenuItems, ref _cachedKeePassContextMenuItemsValue, Default.KeePassContextMenuItems);
            }
        }

        private bool _cachedKeePassToolbarItems = false;
        private KprMenu.MenuItem _cachedKeePassToolbarItemsValue = Default.KeePassToolbarItems;
        public KprMenu.MenuItem KeePassToolbarItems
        {
            get
            {
                return EnumGetter(Key.KeePassToolbarItems, ref _cachedKeePassToolbarItems, ref _cachedKeePassToolbarItemsValue, Default.KeePassToolbarItems);
            }
            set
            {
                EnumSetter(Key.KeePassToolbarItems, value, ref _cachedKeePassToolbarItems, ref _cachedKeePassToolbarItemsValue, Default.KeePassToolbarItems);
            }
        }

        private bool _cachedCredVaultUseWindows = false;
        private bool _cachedCredVaultUseWindowsValue = Default.CredVaultUseWindows;
        public bool CredVaultUseWindows
        {
            get
            {
                return BoolGetter(Key.CredVaultUseWindows, ref _cachedCredVaultUseWindows, ref _cachedCredVaultUseWindowsValue, Default.CredVaultUseWindows);
            }
            set
            {
                BoolSetter(Key.CredVaultUseWindows, value, ref _cachedCredVaultUseWindows, ref _cachedCredVaultUseWindowsValue, Default.CredVaultUseWindows);
            }
        }

        private bool _cachedCredVaultOverwriteExisting = false;
        private bool _cachedCredVaultOverwriteExistingValue = Default.CredVaultOverwriteExisting;
        public bool CredVaultOverwriteExisting
        {
            get
            {
                return BoolGetter(Key.CredVaultOverwriteExisting, ref _cachedCredVaultOverwriteExisting, ref _cachedCredVaultOverwriteExistingValue, Default.CredVaultOverwriteExisting);
            }
            set
            {
                BoolSetter(Key.CredVaultOverwriteExisting, value, ref _cachedCredVaultOverwriteExisting, ref _cachedCredVaultOverwriteExistingValue, Default.CredVaultOverwriteExisting);
            }
        }

        private bool _cachedCredVaultRemoveOnExit = false;
        private bool _cachedCredVaultRemoveOnExitValue = Default.CredVaultRemoveOnExit;
        public bool CredVaultRemoveOnExit
        {
            get
            {
                return BoolGetter(Key.CredVaultRemoveOnExit, ref _cachedCredVaultRemoveOnExit, ref _cachedCredVaultRemoveOnExitValue, Default.CredVaultRemoveOnExit);
            }
            set
            {
                BoolSetter(Key.CredVaultRemoveOnExit, value, ref _cachedCredVaultRemoveOnExit, ref _cachedCredVaultRemoveOnExitValue, Default.CredVaultRemoveOnExit);
            }
        }

        private bool _cachedCredVaultTtl = false;
        private long _cachedCredVaultTtlValue = Default.CredVaultTtl;
        public int CredVaultTtl
        {
            get
            {
                return (int)LongGetter(Key.CredVaultTtl, ref _cachedCredVaultTtl, ref _cachedCredVaultTtlValue, Default.CredVaultTtl);
            }
            set
            {
                LongSetter(Key.CredVaultTtl, value, ref _cachedCredVaultTtl, ref _cachedCredVaultTtlValue, Default.CredVaultTtl);
            }
        }

        private bool _cachedCredVaultAdaptiveTtl = false;
        private bool _cachedCredVaultAdaptiveTtlValue = Default.CredVaultAdaptiveTtl;
        public bool CredVaultAdaptiveTtl
        {
            get
            {
                return BoolGetter(Key.CredVaultAdaptiveTtl, ref _cachedCredVaultAdaptiveTtl, ref _cachedCredVaultAdaptiveTtlValue, Default.CredVaultAdaptiveTtl);
            }
            set
            {
                BoolSetter(Key.CredVaultAdaptiveTtl, value, ref _cachedCredVaultAdaptiveTtl, ref _cachedCredVaultAdaptiveTtlValue, Default.CredVaultAdaptiveTtl);
            }
        }

        /*public Keys ShortcutOpenRdpConnection
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
        }*/

        public Keys GetShortcut(KprMenu.MenuItem menuItem)
        {
            var configKey = string.Format("KPR_shortcut{0}", menuItem.ToString());
            var defaultKey = Keys.None;

            switch (menuItem)
            {
                case KprMenu.MenuItem.OpenRdpConnection:
                    defaultKey = KprMenu.DefaultOpenRdpConnectionShortcut;
                    break;
                case KprMenu.MenuItem.OpenRdpConnectionAdmin:
                    defaultKey = KprMenu.DefaultOpenRdpConnectionAdminShortcut;
                    break;
            }

            return (Keys)_config.GetLong(configKey, (long)defaultKey);
        }

        public void SetShortcut(KprMenu.MenuItem menuItem, Keys value)
        {
            var configKey = string.Format("KPR_shortcut{0}", menuItem.ToString());
            var defaultKey = Keys.None;

            switch (menuItem)
            {
                case KprMenu.MenuItem.OpenRdpConnection:
                    defaultKey = KprMenu.DefaultOpenRdpConnectionShortcut;
                    break;
                case KprMenu.MenuItem.OpenRdpConnectionAdmin:
                    defaultKey = KprMenu.DefaultOpenRdpConnectionAdminShortcut;
                    break;
            }

            if (value == defaultKey)
            {
                if (!string.IsNullOrEmpty(_config.GetString(configKey)))
                    _config.SetString(configKey, null);
            }
            else
                _config.SetLong(configKey, (long)value);
        }
    }
}