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

namespace KeePassRDP
{
    public partial class KprConfig
    {
        private bool _cachedMstscUseFullscreen = false;
        private bool _cachedMstscUseFullscreenValue = false;
        public bool MstscUseFullscreen
        {
            get
            {
                return BoolGetter(MstscUseFullscreenKey, ref _cachedMstscUseFullscreen, ref _cachedMstscUseFullscreenValue);
            }
            set
            {
                BoolSetter(MstscUseFullscreenKey, value, ref _cachedMstscUseFullscreen, ref _cachedMstscUseFullscreenValue);
            }
        }

        private bool _cachedMstscUsePublic = false;
        private bool _cachedMstscUsePublicValue = false;
        public bool MstscUsePublic
        {
            get
            {
                return BoolGetter(MstscUsePublicKey, ref _cachedMstscUsePublic, ref _cachedMstscUsePublicValue);
            }
            set
            {
                BoolSetter(MstscUsePublicKey, value, ref _cachedMstscUsePublic, ref _cachedMstscUsePublicValue);
            }
        }

        private bool _cachedMstscUseAdmin = false;
        private bool _cachedMstscUseAdminValue = false;
        public bool MstscUseAdmin
        {
            get
            {
                return BoolGetter(MstscUseAdminKey, ref _cachedMstscUseAdmin, ref _cachedMstscUseAdminValue);
            }
            set
            {
                BoolSetter(MstscUseAdminKey, value, ref _cachedMstscUseAdmin, ref _cachedMstscUseAdminValue);
            }
        }

        private bool _cachedMstscUseRestrictedAdmin = false;
        private bool _cachedMstscUseRestrictedAdminValue = false;
        public bool MstscUseRestrictedAdmin
        {
            get
            {
                return BoolGetter(MstscUseRestrictedAdminKey, ref _cachedMstscUseRestrictedAdmin, ref _cachedMstscUseRestrictedAdminValue);
            }
            set
            {
                BoolSetter(MstscUseRestrictedAdminKey, value, ref _cachedMstscUseRestrictedAdmin, ref _cachedMstscUseRestrictedAdminValue);
            }
        }

        private bool _cachedMstscUseRemoteGuard = false;
        private bool _cachedMstscUseRemoteGuardValue = false;
        public bool MstscUseRemoteGuard
        {
            get
            {
                return BoolGetter(MstscUseRemoteGuardKey, ref _cachedMstscUseRemoteGuard, ref _cachedMstscUseRemoteGuardValue);
            }
            set
            {
                BoolSetter(MstscUseRemoteGuardKey, value, ref _cachedMstscUseRemoteGuard, ref _cachedMstscUseRemoteGuardValue);
            }
        }

        private bool _cachedMstscUseSpan = false;
        private bool _cachedMstscUseSpanValue = false;
        public bool MstscUseSpan
        {
            get
            {
                return BoolGetter(MstscUseSpanKey, ref _cachedMstscUseSpan, ref _cachedMstscUseSpanValue);
            }
            set
            {
                BoolSetter(MstscUseSpanKey, value, ref _cachedMstscUseSpan, ref _cachedMstscUseSpanValue);
            }
        }

        private bool _cachedMstscUseMultimon = false;
        private bool _cachedMstscUseMultimonValue = false;
        public bool MstscUseMultimon
        {
            get
            {
                return BoolGetter(MstscUseMultimonKey, ref _cachedMstscUseMultimon, ref _cachedMstscUseMultimonValue);
            }
            set
            {
                BoolSetter(MstscUseMultimonKey, value, ref _cachedMstscUseMultimon, ref _cachedMstscUseMultimonValue);
            }
        }

        private bool _cachedMstscWidth = false;
        private ulong _cachedMstscWidthValue = 0UL;
        public int MstscWidth
        {
            get
            {
                return (int)UlongGetter(MstscWidthKey, ref _cachedMstscWidth, ref _cachedMstscWidthValue);
            }
            set
            {
                UlongSetter(MstscWidthKey, (ulong)value, ref _cachedMstscWidth, ref _cachedMstscWidthValue);
            }
        }

        private bool _cachedMstscHeight = false;
        private ulong _cachedMstscHeightValue = 0UL;
        public int MstscHeight
        {
            get
            {
                return (int)UlongGetter(MstscHeightKey, ref _cachedMstscHeight, ref _cachedMstscHeightValue);
            }
            set
            {
                UlongSetter(MstscHeightKey, (ulong)value, ref _cachedMstscHeight, ref _cachedMstscHeightValue);
            }
        }

        private bool _cachedMstscReplaceTitle = false;
        private bool _cachedMstscReplaceTitleValue = true;
        public bool MstscReplaceTitle
        {
            get
            {
                return BoolGetter(MstscReplaceTitleKey, ref _cachedMstscReplaceTitle, ref _cachedMstscReplaceTitleValue, true);
            }
            set
            {
                BoolSetter(MstscReplaceTitleKey, value, ref _cachedMstscReplaceTitle, ref _cachedMstscReplaceTitleValue, true);
            }
        }

        private bool _cachedMstscConfirmCertificate = false;
        private bool _cachedMstscConfirmCertificateValue = false;
        public bool MstscConfirmCertificate
        {
            get
            {
                return BoolGetter(MstscConfirmCertificateKey, ref _cachedMstscConfirmCertificate, ref _cachedMstscConfirmCertificateValue);
            }
            set
            {
                BoolSetter(MstscConfirmCertificateKey, value, ref _cachedMstscConfirmCertificate, ref _cachedMstscConfirmCertificateValue);
            }
        }
    }
}