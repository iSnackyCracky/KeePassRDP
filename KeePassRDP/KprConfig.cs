/*
 *  Copyright (C) 2018-2020 iSnackyCracky
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

namespace KeePassRDP
{
    public class KprConfig
    {
        private readonly AceCustomConfig _config;
        const string KeePassShowResolvedReferencesKey = "KPR_keepassShowResolvedReferences";
        const string CredVaultUseWindowsKey = "KPR_credVaultUseWindows";
        const string CredVaultTtlKey = "KPR_credVaultTtl";
        const string MstscUseFullscreenKey = "KPR_mstscUseFullscreen";
        const string MstscUseAdminKey = "KPR_mstscUseAdmin";
        const string MstscUseSpanKey = "KPR_mstscUseSpan";
        const string MstscUseMultimonKey = "KPR_mstscUseMultimon";
        const string MstscWidthKey = "KPR_mstscWidth";
        const string MstscHeightKey = "KPR_mstscHeight";

        const string CredPickerRememberSizeKey = "KPR_credPickerRememberSize";
        const string CredPickerWidthKey = "KPR_credPickerWidth";
        const string CredPickerHeightKey = "KPR_credPickerHeight";
        const string CredPickerRegExPreKey = "KPR_credPickerRegExPrefix";
        const string CredPickerRegExPostKey = "KPR_credPickerRegExPostfix";

        const string ShortcutOpenRdpConnectionKey = "KPR_shortcutOpenRdpConnection";
        const string ShortcutOpenRdpConnectionAdminKey = "KPR_shortcutOpenRdpConnectionAdmin";
        const string ShortcutOpenRdpConnectionNoCredKey = "KPR_shortcutOpenRdpConnectionNoCred";
        const string ShortcutOpenRdpConnectionNoCredAdminKey = "KPR_shortcutOpenRdpConnectionNoCredAdmin";
        const string ShortcutIgnoreCredentialsKey = "KPR_shortcutIgnoreCredentials";

        public KprConfig(AceCustomConfig config) { _config = config; }

        public bool KeePassShowResolvedReferences
        {
            get { return _config.GetBool(KeePassShowResolvedReferencesKey, true); }
            set { _config.SetBool(KeePassShowResolvedReferencesKey, value); }
        }

        public bool CredVaultUseWindows
        {
            get { return _config.GetBool(CredVaultUseWindowsKey, true); }
            set { _config.SetBool(CredVaultUseWindowsKey, value); }
        }

        public ulong CredVaultTtl
        {
            get { return _config.GetULong(CredVaultTtlKey, 10); }
            set { _config.SetULong(CredVaultTtlKey, value); }
        }

        public bool MstscUseFullscreen
        {
            get { return _config.GetBool(MstscUseFullscreenKey, false); }
            set { _config.SetBool(MstscUseFullscreenKey, value); }
        }

        public bool MstscUseAdmin
        {
            get { return _config.GetBool(MstscUseAdminKey, false); }
            set { _config.SetBool(MstscUseAdminKey, value); }
        }

        public bool MstscUseSpan
        {
            get { return _config.GetBool(MstscUseSpanKey, false); }
            set { _config.SetBool(MstscUseSpanKey, value); }
        }

        public bool MstscUseMultimon
        {
            get { return _config.GetBool(MstscUseMultimonKey, false); }
            set { _config.SetBool(MstscUseMultimonKey, value); }
        }

        public ulong MstscWidth
        {
            get { return _config.GetULong(MstscWidthKey, 0); }
            set { _config.SetULong(MstscWidthKey, value); }
        }

        public ulong MstscHeight
        {
            get { return _config.GetULong(MstscHeightKey, 0); }
            set { _config.SetULong(MstscHeightKey, value); }
        }

        public bool CredPickerRememberSize
        {
            get { return _config.GetBool(CredPickerRememberSizeKey, true); }
            set { _config.SetBool(CredPickerRememberSizeKey, value); }
        }

        public ulong CredPickerWidth
        {
            get { return _config.GetULong(CredPickerWidthKey, 937); }
            set { _config.SetULong(CredPickerWidthKey, value); }
        }

        public ulong CredPickerHeight
        {
            get { return _config.GetULong(CredPickerHeightKey, 467); }
            set { _config.SetULong(CredPickerHeightKey, value); }
        }

        public string CredPickerRegExPre
        {
            get { return _config.GetString(CredPickerRegExPreKey, Util.DefaultCredPickRegExPre); }
            set { _config.SetString(CredPickerRegExPreKey, value); }
        }

        public string CredPickerRegExPost
        {
            get { return _config.GetString(CredPickerRegExPostKey, Util.DefaultCredPickRegExPost); }
            set { _config.SetString(CredPickerRegExPostKey, value); }
        }

        public ulong ShortcutOpenRdpConnection
        {
            get { return _config.GetULong(ShortcutOpenRdpConnectionKey, KprMenu.DefaultOpenRdpConnectionShortcut); }
            set { _config.SetULong(ShortcutOpenRdpConnectionKey, value); }
        }
        public ulong ShortcutOpenRdpConnectionAdmin
        {
            get { return _config.GetULong(ShortcutOpenRdpConnectionAdminKey, KprMenu.DefaultOpenRdpConnectionAdminShortcut); }
            set { _config.SetULong(ShortcutOpenRdpConnectionAdminKey, value); }
        }
        public ulong ShortcutOpenRdpConnectionNoCred
        {
            get { return _config.GetULong(ShortcutOpenRdpConnectionNoCredKey, 0); }
            set { _config.SetULong(ShortcutOpenRdpConnectionNoCredKey, value); }
        }
        public ulong ShortcutOpenRdpConnectionNoCredAdmin
        {
            get { return _config.GetULong(ShortcutOpenRdpConnectionNoCredAdminKey, 0); }
            set { _config.SetULong(ShortcutOpenRdpConnectionNoCredAdminKey, value); }
        }
        public ulong ShortcutIgnoreCredentials
        {
            get { return _config.GetULong(ShortcutIgnoreCredentialsKey, 0); }
            set { _config.SetULong(ShortcutIgnoreCredentialsKey, value); }
        }
    }
}
