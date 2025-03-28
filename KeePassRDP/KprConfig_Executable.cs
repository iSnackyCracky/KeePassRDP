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

namespace KeePassRDP
{
    public partial class KprConfig
    {
        private bool _cachedMstscUseFullscreen = false;
        private bool _cachedMstscUseFullscreenValue = Default.MstscUseFullscreen;
        public bool MstscUseFullscreen
        {
            get
            {
                return BoolGetter(Key.MstscUseFullscreen, ref _cachedMstscUseFullscreen, ref _cachedMstscUseFullscreenValue, Default.MstscUseFullscreen);
            }
            set
            {
                BoolSetter(Key.MstscUseFullscreen, value, ref _cachedMstscUseFullscreen, ref _cachedMstscUseFullscreenValue, Default.MstscUseFullscreen);
            }
        }

        private bool _cachedMstscUsePublic = false;
        private bool _cachedMstscUsePublicValue = Default.MstscUsePublic;
        public bool MstscUsePublic
        {
            get
            {
                return BoolGetter(Key.MstscUsePublic, ref _cachedMstscUsePublic, ref _cachedMstscUsePublicValue, Default.MstscUsePublic);
            }
            set
            {
                BoolSetter(Key.MstscUsePublic, value, ref _cachedMstscUsePublic, ref _cachedMstscUsePublicValue, Default.MstscUsePublic);
            }
        }

        private bool _cachedMstscUseAdmin = false;
        private bool _cachedMstscUseAdminValue = Default.MstscUseAdmin;
        public bool MstscUseAdmin
        {
            get
            {
                return BoolGetter(Key.MstscUseAdmin, ref _cachedMstscUseAdmin, ref _cachedMstscUseAdminValue, Default.MstscUseAdmin);
            }
            set
            {
                BoolSetter(Key.MstscUseAdmin, value, ref _cachedMstscUseAdmin, ref _cachedMstscUseAdminValue, Default.MstscUseAdmin);
            }
        }

        private bool _cachedMstscUseRestrictedAdmin = false;
        private bool _cachedMstscUseRestrictedAdminValue = Default.MstscUseRestrictedAdmin;
        public bool MstscUseRestrictedAdmin
        {
            get
            {
                return BoolGetter(Key.MstscUseRestrictedAdmin, ref _cachedMstscUseRestrictedAdmin, ref _cachedMstscUseRestrictedAdminValue, Default.MstscUseRestrictedAdmin);
            }
            set
            {
                BoolSetter(Key.MstscUseRestrictedAdmin, value, ref _cachedMstscUseRestrictedAdmin, ref _cachedMstscUseRestrictedAdminValue, Default.MstscUseRestrictedAdmin);
            }
        }

        private bool _cachedMstscUseRemoteGuard = false;
        private bool _cachedMstscUseRemoteGuardValue = Default.MstscUseRemoteGuard;
        public bool MstscUseRemoteGuard
        {
            get
            {
                return BoolGetter(Key.MstscUseRemoteGuard, ref _cachedMstscUseRemoteGuard, ref _cachedMstscUseRemoteGuardValue, Default.MstscUseRemoteGuard);
            }
            set
            {
                BoolSetter(Key.MstscUseRemoteGuard, value, ref _cachedMstscUseRemoteGuard, ref _cachedMstscUseRemoteGuardValue, Default.MstscUseRemoteGuard);
            }
        }

        private bool _cachedMstscUseSpan = false;
        private bool _cachedMstscUseSpanValue = Default.MstscUseSpan;
        public bool MstscUseSpan
        {
            get
            {
                return BoolGetter(Key.MstscUseSpan, ref _cachedMstscUseSpan, ref _cachedMstscUseSpanValue, Default.MstscUseSpan);
            }
            set
            {
                BoolSetter(Key.MstscUseSpan, value, ref _cachedMstscUseSpan, ref _cachedMstscUseSpanValue, Default.MstscUseSpan);
            }
        }

        private bool _cachedMstscUseMultimon = false;
        private bool _cachedMstscUseMultimonValue = Default.MstscUseMultimon;
        public bool MstscUseMultimon
        {
            get
            {
                return BoolGetter(Key.MstscUseMultimon, ref _cachedMstscUseMultimon, ref _cachedMstscUseMultimonValue, Default.MstscUseMultimon);
            }
            set
            {
                BoolSetter(Key.MstscUseMultimon, value, ref _cachedMstscUseMultimon, ref _cachedMstscUseMultimonValue, Default.MstscUseMultimon);
            }
        }

        private bool _cachedMstscWidth = false;
        private ulong _cachedMstscWidthValue = Default.MstscWidth;
        public int MstscWidth
        {
            get
            {
                return (int)UlongGetter(Key.MstscWidth, ref _cachedMstscWidth, ref _cachedMstscWidthValue, Default.MstscWidth);
            }
            set
            {
                UlongSetter(Key.MstscWidth, (ulong)value, ref _cachedMstscWidth, ref _cachedMstscWidthValue, Default.MstscWidth);
            }
        }

        private bool _cachedMstscHeight = false;
        private ulong _cachedMstscHeightValue = Default.MstscHeight;
        public int MstscHeight
        {
            get
            {
                return (int)UlongGetter(Key.MstscHeight, ref _cachedMstscHeight, ref _cachedMstscHeightValue, Default.MstscHeight);
            }
            set
            {
                UlongSetter(Key.MstscHeight, (ulong)value, ref _cachedMstscHeight, ref _cachedMstscHeightValue, Default.MstscHeight);
            }
        }

        private bool _cachedMstscReplaceTitle = false;
        private bool _cachedMstscReplaceTitleValue = Default.MstscReplaceTitle;
        public bool MstscReplaceTitle
        {
            get
            {
                return BoolGetter(Key.MstscReplaceTitle, ref _cachedMstscReplaceTitle, ref _cachedMstscReplaceTitleValue, Default.MstscReplaceTitle);
            }
            set
            {
                BoolSetter(Key.MstscReplaceTitle, value, ref _cachedMstscReplaceTitle, ref _cachedMstscReplaceTitleValue, Default.MstscReplaceTitle);
            }
        }

        private bool _cachedMstscCleanupRegistry = false;
        private bool _cachedMstscCleanupRegistryValue = Default.MstscCleanupRegistry;
        public bool MstscCleanupRegistry
        {
            get
            {
                return BoolGetter(Key.MstscCleanupRegistry, ref _cachedMstscCleanupRegistry, ref _cachedMstscCleanupRegistryValue, Default.MstscCleanupRegistry);
            }
            set
            {
                BoolSetter(Key.MstscCleanupRegistry, value, ref _cachedMstscCleanupRegistry, ref _cachedMstscCleanupRegistryValue, Default.MstscCleanupRegistry);
            }
        }

        private bool _cachedMstscConfirmCertificate = false;
        private bool _cachedMstscConfirmCertificateValue = Default.MstscConfirmCertificate;
        public bool MstscConfirmCertificate
        {
            get
            {
                return BoolGetter(Key.MstscConfirmCertificate, ref _cachedMstscConfirmCertificate, ref _cachedMstscConfirmCertificateValue, Default.MstscConfirmCertificate);
            }
            set
            {
                BoolSetter(Key.MstscConfirmCertificate, value, ref _cachedMstscConfirmCertificate, ref _cachedMstscConfirmCertificateValue, Default.MstscConfirmCertificate);
            }
        }

        private bool _cachedMstscHandleCredDialog = false;
        private bool _cachedMstscHandleCredDialogValue = Default.MstscHandleCredDialog;
        public bool MstscHandleCredDialog
        {
            get
            {
                return BoolGetter(Key.MstscHandleCredDialog, ref _cachedMstscHandleCredDialog, ref _cachedMstscHandleCredDialogValue, Default.MstscHandleCredDialog);
            }
            set
            {
                BoolSetter(Key.MstscHandleCredDialog, value, ref _cachedMstscHandleCredDialog, ref _cachedMstscHandleCredDialogValue, Default.MstscHandleCredDialog);
            }
        }

        private bool _cachedMstscSignRdpFiles = false;
        private string _cachedMstscSignRdpFilesValue = Default.MstscSignRdpFiles;
        public string MstscSignRdpFiles
        {
            get
            {
                return StringGetter(Key.MstscSignRdpFiles, ref _cachedMstscSignRdpFiles, ref _cachedMstscSignRdpFilesValue, Default.MstscSignRdpFiles);
            }
            set
            {
                StringSetter(Key.MstscSignRdpFiles, value, ref _cachedMstscSignRdpFiles, ref _cachedMstscSignRdpFilesValue, Default.MstscSignRdpFiles);
            }
        }

        private bool _cachedMstscExecutable = false;
        private string _cachedMstscExecutableValue = Default.MstscExecutable;
        public string MstscExecutable
        {
            get
            {
                return StringGetter(Key.MstscExecutable, ref _cachedMstscExecutable, ref _cachedMstscExecutableValue, Default.MstscExecutable);
            }
            set
            {
                StringSetter(Key.MstscExecutable, value, ref _cachedMstscExecutable, ref _cachedMstscExecutableValue, Default.MstscExecutable);
            }
        }

        private bool _cachedMstscSecureDesktop = false;
        private bool _cachedMstscSecureDesktopValue = Default.MstscSecureDesktop;
        public bool MstscSecureDesktop
        {
            get
            {
                return BoolGetter(Key.MstscSecureDesktop, ref _cachedMstscSecureDesktop, ref _cachedMstscSecureDesktopValue, Default.MstscSecureDesktop);
            }
            set
            {
                BoolSetter(Key.MstscSecureDesktop, value, ref _cachedMstscSecureDesktop, ref _cachedMstscSecureDesktopValue, Default.MstscSecureDesktop);
            }
        }
    }
}