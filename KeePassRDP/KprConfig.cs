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

using KeePass.App.Configuration;
using KeePass.Util.Spr;
using KeePassRDP.Extensions;
using KeePassRDP.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace KeePassRDP
{
    public partial class KprConfig
    {
        #region Constants
        private static class Key
        {
            public const string KeePassSprCompileFlags = "KPR_keepassSprCompileFlags";
            public const string KeePassShowResolvedReferences = "KPR_keepassShowResolvedReferences";
            public const string KeePassConfirmOnClose = "KPR_keepassConfirmOnClose";
            public const string KeePassConnectToAll = "KPR_keepassConnectToAll";
            public const string KeePassAlwaysConfirm = "KPR_keePassAlwaysConfirm";
            public const string KeePassDefaultEntryAction = "KPR_keePassDefaultEntryActionKey";
            public const string KeePassContextMenuOnScreen = "KPR_keePassContextMenuOnScreen";
            public const string KeePassHotkeysRegisterLast = "KPR_keePassHotkeysRegisterLast";
            public const string KeePassContextMenuItems = "KPR_keePassContextMenuItems";
            public const string KeePassToolbarItems = "KPR_keePassToolbarItems";

            public const string CredVaultUseWindows = "KPR_credVaultUseWindows";
            public const string CredVaultOverwriteExisting = "KPR_credVaultOverwriteExisting";
            public const string CredVaultRemoveOnExit = "KPR_credVaultRemoveOnExit";
            public const string CredVaultTtl = "KPR_credVaultTtl";
            public const string CredVaultAdaptiveTtl = "KPR_credVaultAdaptiveTtl";

            public const string MstscUseFullscreen = "KPR_mstscUseFullscreen";
            public const string MstscUsePublic = "KPR_mstscUsePublic";
            public const string MstscUseAdmin = "KPR_mstscUseAdmin";
            public const string MstscUseRestrictedAdmin = "KPR_mstscUseRestrictedAdmin";
            public const string MstscUseRemoteGuard = "KPR_mstscUseRemoteGuard";
            public const string MstscUseSpan = "KPR_mstscUseSpan";
            public const string MstscUseMultimon = "KPR_mstscUseMultimon";
            public const string MstscWidth = "KPR_mstscWidth";
            public const string MstscHeight = "KPR_mstscHeight";
            public const string MstscReplaceTitle = "KPR_mstscReplaceTitle";
            public const string MstscCleanupRegistry = "KPR_mstscCleanupRegistry";
            public const string MstscConfirmCertificate = "KPR_mstscConfirmCertificate";
            public const string MstscHandleCredDialog = "KPR_mstscHandleCredDialog";
            public const string MstscSignRdpFiles = "KPR_mstscSignRdpFiles";
            public const string MstscExecutable = "KPR_mstscExecutable";
            public const string MstscSecureDesktop = "KPR_mstscSecureDesktop";

            public const string CredPickerSecureDesktop = "KPR_credPickerSecureDesktop";
            public const string CredPickerRememberSize = "KPR_credPickerRememberSize";
            public const string CredPickerRememberSortOrder = "KPR_credPickerRememberSortOrder";
            public const string CredPickerSortOrder = "KPR_credPickerSortOrder";
            public const string CredPickerShowInGroups = "KPR_credPickerShowInGroups";
            public const string CredPickerIncludeSelected = "KPR_credPickerIncludeSelected";
            public const string CredPickerLargeRows = "KPR_credPickerLargeRows";
            public const string CredPickerWidth = "KPR_credPickerWidth";
            public const string CredPickerHeight = "KPR_credPickerHeight";
            public const string CredPickerCustomGroup = "KPR_credPickerCustomGroup";
            public const string CredPickerTriggerRecursive = "KPR_credPickerTriggerRecursive";
            public const string CredPickerRegExPre = "KPR_credPickerRegExPrefix";
            public const string CredPickerRegExPost = "KPR_credPickerRegExPostfix";

            /*private const string ShortcutOpenRdpConnectionKey = "KPR_shortcutOpenRdpConnection";
            private const string ShortcutOpenRdpConnectionAdminKey = "KPR_shortcutOpenRdpConnectionAdmin";
            private const string ShortcutOpenRdpConnectionNoCredKey = "KPR_shortcutOpenRdpConnectionNoCred";
            private const string ShortcutOpenRdpConnectionNoCredAdminKey = "KPR_shortcutOpenRdpConnectionNoCredAdmin";
            private const string ShortcutIgnoreCredentialsKey = "KPR_shortcutIgnoreCredentials";*/
        }

        internal static class Default
        {
            public const SprCompileFlags KeePassSprCompileFlags = SprCompileFlags.All;
            public const bool KeePassShowResolvedReferences = true;
            public const bool KeePassConfirmOnClose = true;
            public const bool KeePassConnectToAll = true;
            public const bool KeePassAlwaysConfirm = false;
            public const bool KeePassDefaultEntryAction = false;
            public const bool KeePassContextMenuOnScreen = true;
            public const bool KeePassHotkeysRegisterLast = false;
            public const KprMenu.MenuItem KeePassContextMenuItems = KprMenu.DefaultContextMenuItems;
            public const KprMenu.MenuItem KeePassToolbarItems = KprMenu.DefaultToolbarItems;

            public const bool CredVaultUseWindows = true;
            public const bool CredVaultOverwriteExisting = false;
            public const bool CredVaultRemoveOnExit = true;
            public const int CredVaultTtl = 10;
            public const bool CredVaultAdaptiveTtl = true;

            public const bool MstscUseFullscreen = false;
            public const bool MstscUsePublic = false;
            public const bool MstscUseAdmin = false;
            public const bool MstscUseRestrictedAdmin = false;
            public const bool MstscUseRemoteGuard = false;
            public const bool MstscUseSpan = false;
            public const bool MstscUseMultimon = false;
            public const int MstscWidth = 0;
            public const int MstscHeight = 0;
            public const bool MstscReplaceTitle = true;
            public const bool MstscCleanupRegistry = false;
            public const bool MstscConfirmCertificate = false;
            public const bool MstscHandleCredDialog = false;
            public const string MstscSignRdpFiles = null;
            public const string MstscExecutable = null;
            public const bool MstscSecureDesktop = false;

            public const bool CredPickerSecureDesktop = false;
            public const bool CredPickerRememberSize = true;
            public const bool CredPickerRememberSortOrder = false;
            public const KprListSorter CredPickerSortOrder = null;
            public const bool CredPickerShowInGroups = true;
            public const bool CredPickerIncludeSelected = false;
            public const bool CredPickerLargeRows = false;
            public const int CredPickerWidth = 1150;
            public const int CredPickerHeight = 500;
            public const string CredPickerCustomGroup = Util.DefaultTriggerGroup;
            public const bool CredPickerTriggerRecursive = true;
            public const string CredPickerRegExPre = Util.DefaultCredPickRegExPre;
            public const string CredPickerRegExPost = Util.DefaultCredPickRegExPost;
        }
        #endregion

        private readonly AceCustomConfig _config;

        public KprConfig(AceCustomConfig config)
        {
            _config = config;
        }

        internal void CopyFrom(KprConfig config)
        {
            foreach (var prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                prop.SetValue(this, prop.GetValue(config, null), null);
        }

        internal void CopyTo(KprConfig config)
        {
            foreach (var prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                prop.SetValue(config, prop.GetValue(this, null), null);
        }

        internal KprConfig Clone(bool deep = false)
        {
            var config = new KprConfig(deep ? _config : new AceCustomConfig());

            CopyTo(config);

            return config;
            /*{
                KeePassSprCompileFlags = KeePassSprCompileFlags,
                KeePassShowResolvedReferences = KeePassShowResolvedReferences,
                KeePassConfirmOnClose = KeePassConfirmOnClose,
                KeePassConnectToAll = KeePassConnectToAll,
                KeePassAlwaysConfirm = KeePassAlwaysConfirm,
                KeePassDefaultEntryAction = KeePassDefaultEntryAction,
                KeePassContextMenuOnScreen = KeePassContextMenuOnScreen,
                KeePassHotkeysRegisterLast = KeePassHotkeysRegisterLast,
                KeePassContextMenuItems = KeePassContextMenuItems,
                KeePassToolbarItems = KeePassToolbarItems,

                CredVaultUseWindows = CredVaultUseWindows,
                CredVaultOverwriteExisting = CredVaultOverwriteExisting,
                CredVaultRemoveOnExit = CredVaultRemoveOnExit,
                CredVaultTtl = CredVaultTtl,
                CredVaultAdaptiveTtl = CredVaultAdaptiveTtl,

                MstscUseFullscreen = MstscUseFullscreen,
                MstscUsePublic = MstscUsePublic,
                MstscUseAdmin = MstscUseAdmin,
                MstscUseRestrictedAdmin = MstscUseRestrictedAdmin,
                MstscUseRemoteGuard = MstscUseRemoteGuard,
                MstscUseSpan = MstscUseSpan,
                MstscUseMultimon = MstscUseMultimon,
                MstscWidth = MstscWidth,
                MstscHeight = MstscHeight,
                MstscReplaceTitle = MstscReplaceTitle,
                MstscCleanupRegistry = MstscCleanupRegistry,
                MstscConfirmCertificate = MstscConfirmCertificate,
                MstscHandleCredDialog = MstscHandleCredDialog,
                MstscSignRdpFiles = MstscSignRdpFiles,
                MstscExecutable = MstscExecutable,
                MstscSecureDesktop = MstscSecureDesktop,

                CredPickerSecureDesktop = CredPickerSecureDesktop,
                CredPickerRememberSize = CredPickerRememberSize,
                CredPickerRememberSortOrder = CredPickerRememberSortOrder,
                CredPickerSortOrder = CredPickerSortOrder,
                CredPickerShowInGroups = CredPickerShowInGroups,
                CredPickerIncludeSelected = CredPickerIncludeSelected,
                CredPickerLargeRows = CredPickerLargeRows,
                CredPickerWidth = CredPickerWidth,
                CredPickerHeight = CredPickerHeight,
                CredPickerCustomGroup = CredPickerCustomGroup,
                CredPickerTriggerRecursive = CredPickerTriggerRecursive,
                CredPickerRegExPre = CredPickerRegExPre,
                CredPickerRegExPost = CredPickerRegExPost
            };*/
        }

        internal void Reset()
        {
            FieldInfo value;
            var defaults = typeof(Default).GetFields(BindingFlags.Static | BindingFlags.Public).ToDictionary(x => x.Name, x => x);
            foreach (var prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                if (defaults.TryGetValue(prop.Name, out value))
                    prop.SetValue(this, value.GetValue(null), null);

            /*KeePassSprCompileFlags = Default.KeePassSprCompileFlags;
            KeePassShowResolvedReferences = Default.KeePassShowResolvedReferences;
            KeePassConfirmOnClose = Default.KeePassConfirmOnClose;
            KeePassConnectToAll = Default.KeePassConnectToAll;
            KeePassAlwaysConfirm = Default.KeePassAlwaysConfirm;
            KeePassDefaultEntryAction = Default.KeePassDefaultEntryAction;
            KeePassContextMenuOnScreen = Default.KeePassContextMenuOnScreen;
            KeePassHotkeysRegisterLast = Default.KeePassHotkeysRegisterLast;
            KeePassContextMenuItems = Default.KeePassContextMenuItems;
            KeePassToolbarItems = Default.KeePassToolbarItems;

            CredVaultUseWindows = Default.CredVaultUseWindows;
            CredVaultOverwriteExisting = Default.CredVaultOverwriteExisting;
            CredVaultRemoveOnExit = Default.CredVaultRemoveOnExit;
            CredVaultTtl = Default.CredVaultTtl;
            CredVaultAdaptiveTtl = Default.CredVaultAdaptiveTtl;

            MstscUseFullscreen = Default.MstscUseFullscreen;
            MstscUsePublic = Default.MstscUsePublic;
            MstscUseAdmin = Default.MstscUseAdmin;
            MstscUseRestrictedAdmin = Default.MstscUseRestrictedAdmin;
            MstscUseRemoteGuard = Default.MstscUseRemoteGuard;
            MstscUseSpan = Default.MstscUseSpan;
            MstscUseMultimon = Default.MstscUseMultimon;
            MstscWidth = Default.MstscWidth;
            MstscHeight = Default.MstscHeight;
            MstscReplaceTitle = Default.MstscReplaceTitle;
            MstscCleanupRegistry = Default.MstscCleanupRegistry;
            MstscConfirmCertificate = Default.MstscConfirmCertificate;
            MstscHandleCredDialog = Default.MstscHandleCredDialog;
            MstscSignRdpFiles = Default.MstscSignRdpFiles;
            MstscExecutable = Default.MstscExecutable;
            MstscSecureDesktop = Default.MstscSecureDesktop;

            CredPickerSecureDesktop = Default.CredPickerSecureDesktop;
            CredPickerRememberSize = Default.CredPickerRememberSize;
            CredPickerRememberSortOrder = Default.CredPickerRememberSortOrder;
            CredPickerSortOrder = Default.CredPickerSortOrder;
            CredPickerShowInGroups = Default.CredPickerShowInGroups;
            CredPickerIncludeSelected = Default.CredPickerIncludeSelected;
            CredPickerLargeRows = Default.CredPickerLargeRows;
            CredPickerWidth = Default.CredPickerWidth;
            CredPickerHeight = Default.CredPickerHeight;
            CredPickerCustomGroup = Default.CredPickerCustomGroup;
            CredPickerTriggerRecursive = Default.CredPickerTriggerRecursive;
            CredPickerRegExPre = Default.CredPickerRegExPre;
            CredPickerRegExPost = Default.CredPickerRegExPost;*/
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
            {
                if (!cached)
                    cached = true;
                return;
            }
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
            {
                if (!cached)
                    cached = true;
                return;
            }
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
            {
                if (!cached)
                    cached = true;
                return;
            }
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
            if (string.IsNullOrEmpty(defaultValue))
                defaultValue = string.Empty;
            cachedValue = _config.GetString(strID, defaultValue);
            if (string.IsNullOrEmpty(cachedValue))
                cachedValue = string.Empty;
            _config.RemoveIfDefault(strID, cachedValue, defaultValue);
            return cachedValue;
        }

        private void StringSetter(string strID, string value, ref bool cached, ref string cachedValue, string defaultValue = null)
        {
            if (string.IsNullOrEmpty(value))
                value = string.Empty;
            if (string.IsNullOrEmpty(cachedValue))
                cachedValue = string.Empty;
            if (cachedValue == value)
            {
                if (!cached)
                    cached = true;
                return;
            }
            if (!cached)
                cached = true;
            if (string.IsNullOrEmpty(defaultValue))
                defaultValue = string.Empty;
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
            {
                if (!cached)
                    cached = true;
                return;
            }
            if (!cached)
                cached = true;
            cachedValue = value;
            if (!_config.RemoveIfDefault(strID, value, defaultValue))
                _config.SetString(strID, value.ToString());
        }
        #endregion
    }
}