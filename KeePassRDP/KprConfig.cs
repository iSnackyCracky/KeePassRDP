/*
 *  Copyright (C) 2018 - 2023 iSnackyCracky, NETertainer
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

using KeePass.App.Configuration;
using KeePassRDP.Extensions;
using System;

namespace KeePassRDP
{
    public partial class KprConfig
    {
        #region Constants
        private const string KeePassShowResolvedReferencesKey = "KPR_keepassShowResolvedReferences";
        private const string KeePassConfirmOnCloseKey = "KPR_keepassConfirmOnClose";
        private const string KeePassConnectToAllKey = "KPR_keepassConnectToAll";
        private const string KeePassAlwaysConfirmKey = "KPR_keePassAlwaysConfirm";
        private const string KeePassHotkeysRegisterLastKey = "KPR_keePassHotkeysRegisterLast";
        private const string KeePassContextMenuItemsKey = "KPR_keePassContextMenuItems";
        private const string KeePassToolbarItemsKey = "KPR_keePassToolbarItems";
        private const string CredVaultUseWindowsKey = "KPR_credVaultUseWindows";
        private const string CredVaultOverwriteExistingKey = "KPR_credVaultOverwriteExisting";
        private const string CredVaultRemoveOnExitKey = "KPR_credVaultRemoveOnExit";
        private const string CredVaultTtlKey = "KPR_credVaultTtl";
        private const string CredVaultAdaptiveTtlKey = "KPR_credVaultAdaptiveTtl";
        private const string MstscUseFullscreenKey = "KPR_mstscUseFullscreen";
        private const string MstscUsePublicKey = "KPR_mstscUsePublic";
        private const string MstscUseAdminKey = "KPR_mstscUseAdmin";
        private const string MstscUseRestrictedAdminKey = "KPR_mstscUseRestrictedAdmin";
        private const string MstscUseRemoteGuardKey = "KPR_mstscUseRemoteGuard";
        private const string MstscUseSpanKey = "KPR_mstscUseSpan";
        private const string MstscUseMultimonKey = "KPR_mstscUseMultimon";
        private const string MstscWidthKey = "KPR_mstscWidth";
        private const string MstscHeightKey = "KPR_mstscHeight";
        private const string MstscReplaceTitleKey = "KPR_mstscReplaceTitle";
        private const string MstscConfirmCertificateKey = "KPR_mstscConfirmCertificate";

        private const string CredPickerRememberSizeKey = "KPR_credPickerRememberSize";
        private const string CredPickerRememberSortOrderKey = "KPR_credPickerRememberSortOrder";
        private const string CredPickerSortOrderKey = "KPR_credPickerSortOrder";
        private const string CredPickerShowInGroupsKey = "KPR_credPickerShowInGroups";
        private const string CredPickerIncludeSelectedKey = "KPR_credPickerIncludeSelected";
        private const string CredPickerLargeRowsKey = "KPR_credPickerLargeRows";
        private const string CredPickerWidthKey = "KPR_credPickerWidth";
        private const string CredPickerHeightKey = "KPR_credPickerHeight";
        private const string CredPickerCustomGroupKey = "KPR_credPickerCustomGroup";
        private const string CredPickerRegExPreKey = "KPR_credPickerRegExPrefix";
        private const string CredPickerRegExPostKey = "KPR_credPickerRegExPostfix";

        private const string ShortcutOpenRdpConnectionKey = "KPR_shortcutOpenRdpConnection";
        private const string ShortcutOpenRdpConnectionAdminKey = "KPR_shortcutOpenRdpConnectionAdmin";
        private const string ShortcutOpenRdpConnectionNoCredKey = "KPR_shortcutOpenRdpConnectionNoCred";
        private const string ShortcutOpenRdpConnectionNoCredAdminKey = "KPR_shortcutOpenRdpConnectionNoCredAdmin";
        private const string ShortcutIgnoreCredentialsKey = "KPR_shortcutIgnoreCredentials";
        #endregion

        private readonly AceCustomConfig _config;

        public KprConfig(AceCustomConfig config)
        {
            _config = config;
        }

        #region Getter/setter helper functions
        private bool BoolGetter(string strID, ref bool cached, ref bool cachedValue, bool defaultValue = false)
        {
            if (cached)
                return cachedValue;
            cached = true;
            cachedValue = _config.GetBool(strID, defaultValue);
            _config.RemoveIfDefault(strID, cachedValue, defaultValue);
            return cachedValue;
        }

        private void BoolSetter(string strID, bool value, ref bool cached, ref bool cachedValue, bool defaultValue = false)
        {
            if (cachedValue == value)
                return;
            if (!cached)
                cached = true;
            cachedValue = value;
            if (!_config.RemoveIfDefault(strID, value, defaultValue))
                _config.SetBool(strID, value);
        }

        private long LongGetter(string strID, ref bool cached, ref long cachedValue, long defaultValue = 0L)
        {
            if (cached)
                return cachedValue;
            cached = true;
            cachedValue = _config.GetLong(strID, defaultValue);
            _config.RemoveIfDefault(strID, cachedValue, defaultValue);
            return cachedValue;
        }

        private void LongSetter(string strID, long value, ref bool cached, ref long cachedValue, long defaultValue = 0L)
        {
            if (cachedValue == value)
                return;
            if (!cached)
                cached = true;
            cachedValue = value;
            if (!_config.RemoveIfDefault(strID, value, defaultValue))
                _config.SetLong(strID, value);
        }

        private ulong UlongGetter(string strID, ref bool cached, ref ulong cachedValue, ulong defaultValue = 0UL)
        {
            if (cached)
                return cachedValue;
            cached = true;
            cachedValue = _config.GetULong(strID, defaultValue);
            _config.RemoveIfDefault(strID, cachedValue, defaultValue);
            return cachedValue;
        }

        private void UlongSetter(string strID, ulong value, ref bool cached, ref ulong cachedValue, ulong defaultValue = 0UL)
        {
            if (cachedValue == value)
                return;
            if (!cached)
                cached = true;
            cachedValue = value;
            if (!_config.RemoveIfDefault(strID, value, defaultValue))
                _config.SetULong(strID, value);
        }

        private string StringGetter(string strID, ref bool cached, ref string cachedValue, string defaultValue = null)
        {
            if (cached)
                return cachedValue;
            cached = true;
            cachedValue = _config.GetString(strID, defaultValue);
            _config.RemoveIfDefault(strID, cachedValue, defaultValue);
            return cachedValue;
        }

        private void StringSetter(string strID, string value, ref bool cached, ref string cachedValue, string defaultValue = null)
        {
            if (cachedValue == value)
                return;
            if (!cached)
                cached = true;
            cachedValue = value;
            if (!_config.RemoveIfDefault(strID, value, defaultValue))
                _config.SetString(strID, value);
        }

        private T EnumGetter<T>(string strID, ref bool cached, ref T cachedValue, T defaultValue) where T : struct
        {
            if (cached)
                return cachedValue;
            cached = true;
            T items;
            if (!Enum.TryParse(_config.GetString(strID) ?? string.Empty, out items))
                items = defaultValue;
            cachedValue = items;
            _config.RemoveIfDefault(strID, cachedValue, defaultValue);
            return cachedValue;
        }

        private void EnumSetter<T>(string strID, T value, ref bool cached, ref T cachedValue, T defaultValue) where T : struct
        {
            if (cachedValue.Equals(value))
                return;
            if (!cached)
                cached = true;
            cachedValue = value;
            if (!_config.RemoveIfDefault(strID, value, defaultValue))
                _config.SetString(strID, value.ToString());
        }
        #endregion
    }
}