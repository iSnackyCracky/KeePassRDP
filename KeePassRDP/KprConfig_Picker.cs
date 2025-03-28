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

using KeePassRDP.Utils;

namespace KeePassRDP
{
    public partial class KprConfig
    {
        private bool _cachedCredPickerSecureDesktop = false;
        private bool _cachedCredPickerSecureDesktopValue = Default.CredPickerSecureDesktop;
        public bool CredPickerSecureDesktop
        {
            get
            {
                return BoolGetter(Key.CredPickerSecureDesktop, ref _cachedCredPickerSecureDesktop, ref _cachedCredPickerSecureDesktopValue, Default.CredPickerSecureDesktop);
            }
            set
            {
                BoolSetter(Key.CredPickerSecureDesktop, value, ref _cachedCredPickerSecureDesktop, ref _cachedCredPickerSecureDesktopValue, Default.CredPickerSecureDesktop);
            }
        }

        private bool _cachedCredPickerRememberSize = false;
        private bool _cachedCredPickerRememberSizeValue = Default.CredPickerRememberSize;
        public bool CredPickerRememberSize
        {
            get
            {
                return BoolGetter(Key.CredPickerRememberSize, ref _cachedCredPickerRememberSize, ref _cachedCredPickerRememberSizeValue, Default.CredPickerRememberSize);
            }
            set
            {
                BoolSetter(Key.CredPickerRememberSize, value, ref _cachedCredPickerRememberSize, ref _cachedCredPickerRememberSizeValue, Default.CredPickerRememberSize);
            }
        }

        private bool _cachedCredPickerRememberSortOrder = false;
        private bool _cachedCredPickerRememberSortOrderValue = Default.CredPickerRememberSortOrder;
        public bool CredPickerRememberSortOrder
        {
            get
            {
                return BoolGetter(Key.CredPickerRememberSortOrder, ref _cachedCredPickerRememberSortOrder, ref _cachedCredPickerRememberSortOrderValue, Default.CredPickerRememberSortOrder);
            }
            set
            {
                BoolSetter(Key.CredPickerRememberSortOrder, value, ref _cachedCredPickerRememberSortOrder, ref _cachedCredPickerRememberSortOrderValue, Default.CredPickerRememberSortOrder);
                if (value == false)
                    CredPickerSortOrder = null;
            }
        }

        private bool _cachedCredPickerSortOrder = false;
        private KprListSorter _cachedCredPickerSortOrderValue = null;
        private KprListSorter _defaultCredPickerSortOrderValue = Default.CredPickerSortOrder;
        public KprListSorter CredPickerSortOrder
        {
            get
            {
                if (_cachedCredPickerSortOrder)
                    return _cachedCredPickerSortOrderValue != null ? _cachedCredPickerSortOrderValue.Clone() : _cachedCredPickerSortOrderValue;

                if (!CredPickerRememberSortOrder)
                    return CredPickerSortOrder = null;

                var serialized = _config.GetString(Key.CredPickerSortOrder, string.Empty);
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
                    if (_config.GetString(Key.CredPickerSortOrder) != null)
                        _config.SetString(Key.CredPickerSortOrder, null);
                    _cachedCredPickerSortOrder = true;
                    _defaultCredPickerSortOrderValue = _cachedCredPickerSortOrderValue = null;
                    return;
                }

                _cachedCredPickerSortOrder = true;
                _cachedCredPickerSortOrderValue = value;

                if (value == null || (/*value.Column == "Title" &&*/ value.SortOrder == System.Windows.Forms.SortOrder.None))
                {
                    if (_config.GetString(Key.CredPickerSortOrder) != null)
                        _config.SetString(Key.CredPickerSortOrder, null);
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
                    _config.SetString(Key.CredPickerSortOrder, serialized);
                else if (_config.GetString(Key.CredPickerSortOrder) != null)
                    _config.SetString(Key.CredPickerSortOrder, null);
            }
        }

        private bool _cachedCredPickerWidth = false;
        private ulong _cachedCredPickerWidthValue = Default.CredPickerWidth;
        public int CredPickerWidth
        {
            get
            {
                return (int)UlongGetter(Key.CredPickerWidth, ref _cachedCredPickerWidth, ref _cachedCredPickerWidthValue, Default.CredPickerWidth);
            }
            set
            {
                UlongSetter(Key.CredPickerWidth, (ulong)value, ref _cachedCredPickerWidth, ref _cachedCredPickerWidthValue, Default.CredPickerWidth);
            }
        }

        private bool _cachedCredPickerHeight = false;
        private ulong _cachedCredPickerHeightValue = Default.CredPickerHeight;
        public int CredPickerHeight
        {
            get
            {
                return (int)UlongGetter(Key.CredPickerHeight, ref _cachedCredPickerHeight, ref _cachedCredPickerHeightValue, Default.CredPickerHeight);
            }
            set
            {
                UlongSetter(Key.CredPickerHeight, (ulong)value, ref _cachedCredPickerHeight, ref _cachedCredPickerHeightValue, Default.CredPickerHeight);
            }
        }

        private bool _cachedCredPickerCustomGroup = false;
        private string _cachedCredPickerCustomGroupValue = Default.CredPickerCustomGroup;
        public string CredPickerCustomGroup
        {
            get
            {
                return StringGetter(Key.CredPickerCustomGroup, ref _cachedCredPickerCustomGroup, ref _cachedCredPickerCustomGroupValue, Default.CredPickerCustomGroup);
            }
            set
            {
                StringSetter(Key.CredPickerCustomGroup, !string.IsNullOrWhiteSpace(value) ? value : Default.CredPickerCustomGroup, ref _cachedCredPickerCustomGroup, ref _cachedCredPickerCustomGroupValue, Default.CredPickerCustomGroup);
            }
        }

        private bool _cachedCredPickerTriggerRecursive = false;
        private bool _cachedCredPickerTriggerRecursiveValue = Default.CredPickerTriggerRecursive;
        public bool CredPickerTriggerRecursive
        {
            get
            {
                return BoolGetter(Key.CredPickerTriggerRecursive, ref _cachedCredPickerTriggerRecursive, ref _cachedCredPickerTriggerRecursiveValue, Default.CredPickerTriggerRecursive);
            }
            set
            {
                BoolSetter(Key.CredPickerTriggerRecursive, value, ref _cachedCredPickerTriggerRecursive, ref _cachedCredPickerTriggerRecursiveValue, Default.CredPickerTriggerRecursive);
            }
        }

        private bool _cachedKeePassShowResolvedReferences = false;
        private bool _cachedKeePassShowResolvedReferencesValue = Default.KeePassShowResolvedReferences;
        public bool KeePassShowResolvedReferences
        {
            get
            {
                return BoolGetter(Key.KeePassShowResolvedReferences, ref _cachedKeePassShowResolvedReferences, ref _cachedKeePassShowResolvedReferencesValue, Default.KeePassShowResolvedReferences);
            }
            set
            {
                BoolSetter(Key.KeePassShowResolvedReferences, value, ref _cachedKeePassShowResolvedReferences, ref _cachedKeePassShowResolvedReferencesValue, Default.KeePassShowResolvedReferences);
            }
        }

        private bool _cachedCredPickerShowInGroups = false;
        private bool _cachedCredPickerShowInGroupsValue = Default.CredPickerShowInGroups;
        public bool CredPickerShowInGroups
        {
            get
            {
                return BoolGetter(Key.CredPickerShowInGroups, ref _cachedCredPickerShowInGroups, ref _cachedCredPickerShowInGroupsValue, Default.CredPickerShowInGroups);
            }
            set
            {
                BoolSetter(Key.CredPickerShowInGroups, value, ref _cachedCredPickerShowInGroups, ref _cachedCredPickerShowInGroupsValue, Default.CredPickerShowInGroups);
                // Reset sort order when columns change.
                if (!value &&
                    CredPickerRememberSortOrder &&
                    CredPickerSortOrder != null &&
                    CredPickerSortOrder.Column == "Path")
                    CredPickerSortOrder = null;
            }
        }

        private bool _cachedCredPickerIncludeSelected = false;
        private bool _cachedCredPickerIncludeSelectedValue = Default.CredPickerIncludeSelected;
        public bool CredPickerIncludeSelected
        {
            get
            {
                return BoolGetter(Key.CredPickerIncludeSelected, ref _cachedCredPickerIncludeSelected, ref _cachedCredPickerIncludeSelectedValue, Default.CredPickerIncludeSelected);
            }
            set
            {
                BoolSetter(Key.CredPickerIncludeSelected, value, ref _cachedCredPickerIncludeSelected, ref _cachedCredPickerIncludeSelectedValue, Default.CredPickerIncludeSelected);
            }
        }

        private bool _cachedCredPickerLargeRows = false;
        private bool _cachedCredPickerLargeRowsValue = Default.CredPickerLargeRows;
        public bool CredPickerLargeRows
        {
            get
            {
                return BoolGetter(Key.CredPickerLargeRows, ref _cachedCredPickerLargeRows, ref _cachedCredPickerLargeRowsValue, Default.CredPickerLargeRows);
            }
            set
            {
                BoolSetter(Key.CredPickerLargeRows, value, ref _cachedCredPickerLargeRows, ref _cachedCredPickerLargeRowsValue, Default.CredPickerLargeRows);
            }
        }

        private bool _cachedCredPickerRegExPre = false;
        private string _cachedCredPickerRegExPreValue = Default.CredPickerRegExPre;
        public string CredPickerRegExPre
        {
            get
            {
                return StringGetter(Key.CredPickerRegExPre, ref _cachedCredPickerRegExPre, ref _cachedCredPickerRegExPreValue, Default.CredPickerRegExPre);
            }
            set
            {
                StringSetter(Key.CredPickerRegExPre, value, ref _cachedCredPickerRegExPre, ref _cachedCredPickerRegExPreValue, Default.CredPickerRegExPre);
            }
        }

        private bool _cachedCredPickerRegExPost = false;
        private string _cachedCredPickerRegExPostValue = Default.CredPickerRegExPost;
        public string CredPickerRegExPost
        {
            get
            {
                return StringGetter(Key.CredPickerRegExPost, ref _cachedCredPickerRegExPost, ref _cachedCredPickerRegExPostValue, Default.CredPickerRegExPost);
            }
            set
            {
                StringSetter(Key.CredPickerRegExPost, value, ref _cachedCredPickerRegExPost, ref _cachedCredPickerRegExPostValue, Default.CredPickerRegExPost);
            }
        }
    }
}