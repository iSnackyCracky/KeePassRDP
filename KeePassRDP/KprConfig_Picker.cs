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

using KeePassRDP.Utils;

namespace KeePassRDP
{
    public partial class KprConfig
    {
        private bool _cachedCredPickerRememberSize = false;
        private bool _cachedCredPickerRememberSizeValue = true;
        public bool CredPickerRememberSize
        {
            get
            {
                return BoolGetter(CredPickerRememberSizeKey, ref _cachedCredPickerRememberSize, ref _cachedCredPickerRememberSizeValue, true);
            }
            set
            {
                BoolSetter(CredPickerRememberSizeKey, value, ref _cachedCredPickerRememberSize, ref _cachedCredPickerRememberSizeValue, true);
            }
        }

        private bool _cachedCredPickerRememberSortOrder = false;
        private bool _cachedCredPickerRememberSortOrderValue = false;
        public bool CredPickerRememberSortOrder
        {
            get
            {
                return BoolGetter(CredPickerRememberSortOrderKey, ref _cachedCredPickerRememberSortOrder, ref _cachedCredPickerRememberSortOrderValue);
            }
            set
            {
                BoolSetter(CredPickerRememberSortOrderKey, value, ref _cachedCredPickerRememberSortOrder, ref _cachedCredPickerRememberSortOrderValue);
                if (value == false)
                    CredPickerSortOrder = null;
            }
        }

        private bool _cachedCredPickerSortOrder = false;
        private KprListSorter _cachedCredPickerSortOrderValue = null;
        private KprListSorter _defaultCredPickerSortOrderValue = null;
        public KprListSorter CredPickerSortOrder
        {
            get
            {
                if (_cachedCredPickerSortOrder)
                    return _cachedCredPickerSortOrderValue != null ? _cachedCredPickerSortOrderValue.Clone() : _cachedCredPickerSortOrderValue;

                if (!CredPickerRememberSortOrder)
                    return CredPickerSortOrder = null;

                var serialized = _config.GetString(CredPickerSortOrderKey, string.Empty);
                if (string.IsNullOrEmpty(serialized))
                {
                    _cachedCredPickerSortOrder = true;
                    return _cachedCredPickerSortOrderValue = null;
                }

                _cachedCredPickerSortOrder = true;
                _defaultCredPickerSortOrderValue = KprListSorter.Deserialize(serialized);
                return (_cachedCredPickerSortOrderValue = new KprListSorter
                {
                    Column = _defaultCredPickerSortOrderValue.Column,
                    SortOrder = _defaultCredPickerSortOrderValue.SortOrder
                }).Clone();
            }
            set
            {
                if (_defaultCredPickerSortOrderValue == value)
                    return;

                if (!CredPickerRememberSortOrder)
                {
                    if (_config.GetString(CredPickerSortOrderKey) != null)
                        _config.SetString(CredPickerSortOrderKey, null);
                    _cachedCredPickerSortOrder = true;
                    _defaultCredPickerSortOrderValue = _cachedCredPickerSortOrderValue = null;
                    return;
                }

                _cachedCredPickerSortOrder = true;
                _cachedCredPickerSortOrderValue = value;

                if (value == null || (/*value.Column == "Title" &&*/ value.SortOrder == System.Windows.Forms.SortOrder.None))
                {
                    if (_config.GetString(CredPickerSortOrderKey) != null)
                        _config.SetString(CredPickerSortOrderKey, null);
                    _defaultCredPickerSortOrderValue = _cachedCredPickerSortOrderValue = null;
                    return;
                }

                if (_defaultCredPickerSortOrderValue == null)
                    _defaultCredPickerSortOrderValue = new KprListSorter();

                if (_cachedCredPickerSortOrderValue != null)
                {
                    _defaultCredPickerSortOrderValue.Column = _cachedCredPickerSortOrderValue.Column;
                    _defaultCredPickerSortOrderValue.SortOrder = _cachedCredPickerSortOrderValue.SortOrder;
                }

                var serialized = value.Serialize();
                if (!string.IsNullOrEmpty(serialized))
                    _config.SetString(CredPickerSortOrderKey, serialized);
                else if (_config.GetString(CredPickerSortOrderKey) != null)
                    _config.SetString(CredPickerSortOrderKey, null);
            }
        }

        private bool _cachedCredPickerWidth = false;
        private ulong _cachedCredPickerWidthValue = 1150UL;
        public int CredPickerWidth
        {
            get
            {
                return (int)UlongGetter(CredPickerWidthKey, ref _cachedCredPickerWidth, ref _cachedCredPickerWidthValue, 1150UL);
            }
            set
            {
                UlongSetter(CredPickerWidthKey, (ulong)value, ref _cachedCredPickerWidth, ref _cachedCredPickerWidthValue, 1150UL);
            }
        }

        private bool _cachedCredPickerHeight = false;
        private ulong _cachedCredPickerHeightValue = 500UL;
        public int CredPickerHeight
        {
            get
            {
                return (int)UlongGetter(CredPickerHeightKey, ref _cachedCredPickerHeight, ref _cachedCredPickerHeightValue, 500UL);
            }
            set
            {
                UlongSetter(CredPickerHeightKey, (ulong)value, ref _cachedCredPickerHeight, ref _cachedCredPickerHeightValue, 500UL);
            }
        }

        private bool _cachedCredPickerCustomGroup = false;
        private string _cachedCredPickerCustomGroupValue = Util.DefaultTriggerGroup;
        public string CredPickerCustomGroup
        {
            get
            {
                return StringGetter(CredPickerCustomGroupKey, ref _cachedCredPickerCustomGroup, ref _cachedCredPickerCustomGroupValue, Util.DefaultTriggerGroup);
            }
            set
            {
                StringSetter(CredPickerCustomGroupKey, value, ref _cachedCredPickerCustomGroup, ref _cachedCredPickerCustomGroupValue, string.IsNullOrWhiteSpace(value) ? value : Util.DefaultTriggerGroup);
            }
        }

        private bool _cachedKeePassShowResolvedReferences = false;
        private bool _cachedKeePassShowResolvedReferencesValue = true;
        public bool KeePassShowResolvedReferences
        {
            get
            {
                return BoolGetter(KeePassShowResolvedReferencesKey, ref _cachedKeePassShowResolvedReferences, ref _cachedKeePassShowResolvedReferencesValue, true);
            }
            set
            {
                BoolSetter(KeePassShowResolvedReferencesKey, value, ref _cachedKeePassShowResolvedReferences, ref _cachedKeePassShowResolvedReferencesValue, true);
            }
        }

        private bool _cachedCredPickerShowInGroups = false;
        private bool _cachedCredPickerShowInGroupsValue = true;
        public bool CredPickerShowInGroups
        {
            get
            {
                return BoolGetter(CredPickerShowInGroupsKey, ref _cachedCredPickerShowInGroups, ref _cachedCredPickerShowInGroupsValue, true);
            }
            set
            {
                BoolSetter(CredPickerShowInGroupsKey, value, ref _cachedCredPickerShowInGroups, ref _cachedCredPickerShowInGroupsValue, true);
                // Reset sort order when columns change.
                if (!value &&
                    CredPickerRememberSortOrder &&
                    CredPickerSortOrder != null &&
                    CredPickerSortOrder.Column == "Path")
                    CredPickerSortOrder = null;
            }
        }

        private bool _cachedCredPickerIncludeSelected = false;
        private bool _cachedCredPickerIncludeSelectedValue = false;
        public bool CredPickerIncludeSelected
        {
            get
            {
                return BoolGetter(CredPickerIncludeSelectedKey, ref _cachedCredPickerIncludeSelected, ref _cachedCredPickerIncludeSelectedValue);
            }
            set
            {
                BoolSetter(CredPickerIncludeSelectedKey, value, ref _cachedCredPickerIncludeSelected, ref _cachedCredPickerIncludeSelectedValue);
            }
        }

        private bool _cachedCredPickerLargeRows = false;
        private bool _cachedCredPickerLargeRowsValue = false;
        public bool CredPickerLargeRows
        {
            get
            {
                return BoolGetter(CredPickerLargeRowsKey, ref _cachedCredPickerLargeRows, ref _cachedCredPickerLargeRowsValue);
            }
            set
            {
                BoolSetter(CredPickerLargeRowsKey, value, ref _cachedCredPickerLargeRows, ref _cachedCredPickerLargeRowsValue);
            }
        }

        private bool _cachedCredPickerRegExPre = false;
        private string _cachedCredPickerRegExPreValue = Util.DefaultCredPickRegExPre;
        public string CredPickerRegExPre
        {
            get
            {
                return StringGetter(CredPickerRegExPreKey, ref _cachedCredPickerRegExPre, ref _cachedCredPickerRegExPreValue, Util.DefaultCredPickRegExPre);
            }
            set
            {
                StringSetter(CredPickerRegExPreKey, value, ref _cachedCredPickerRegExPre, ref _cachedCredPickerRegExPreValue, Util.DefaultCredPickRegExPre);
            }
        }

        private bool _cachedCredPickerRegExPost = false;
        private string _cachedCredPickerRegExPostValue = Util.DefaultCredPickRegExPre;
        public string CredPickerRegExPost
        {
            get
            {
                return StringGetter(CredPickerRegExPostKey, ref _cachedCredPickerRegExPost, ref _cachedCredPickerRegExPostValue, Util.DefaultCredPickRegExPost);
            }
            set
            {
                StringSetter(CredPickerRegExPostKey, value, ref _cachedCredPickerRegExPost, ref _cachedCredPickerRegExPostValue, Util.DefaultCredPickRegExPost);
            }
        }
    }
}