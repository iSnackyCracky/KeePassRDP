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

using KeePass;
using KeePass.App.Configuration;
using KeePassRDP.Utils;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace KeePassRDP
{
    internal interface IKprResourceManager
    {
        string this[string key] { get; }
        void Translate(Control control);
        void TranslateMany(params Control[] controls);
        void Translate(ToolStripItem item);
        void TranslateMany(params ToolStripItem[] items);
        void ClearCache();
    }

    internal class KprResourceManager : IKprResourceManager, IDisposable
    {
        private static readonly Lazy<IKprResourceManager> _instance = new Lazy<IKprResourceManager>(() =>
        {
            try
            {
                Resources.Resources.Culture = CultureInfo.CreateSpecificCulture(Program.Translation.Properties.Iso6391Code);
            }
            catch
            {
                Resources.Resources.Culture = CultureInfo.CreateSpecificCulture("en");
            }

            return new KprResourceManager();
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        private class EmptyKprResourceManager : IKprResourceManager
        {
            public string this[string key]
            {
                get
                {
                    return key;
                }
            }

            public void ClearCache()
            {
            }

            public void Translate(Control control)
            {
            }

            public void Translate(ToolStripItem item)
            {
            }

            public void TranslateMany(params Control[] controls)
            {
            }

            public void TranslateMany(params ToolStripItem[] items)
            {
            }
        }

        private static readonly EmptyKprResourceManager _emptyInstance = new EmptyKprResourceManager();

        public static IKprResourceManager Instance { get { return !KeePassRDPExt.Initialized.IsSet ? _emptyInstance : _instance.Value; } }

        private readonly ConcurrentDictionary<string, string> _cache;
        private readonly Lazy<ResourceManager> _fileBasedResourceManager;

        public string this[string key]
        {
            get
            {
                if (!KeePassRDPExt.Initialized.IsSet)
                    return key;

                return _cache.GetOrAdd(key, k =>
                {
                    string text = null;
                    try
                    {
                        text = _fileBasedResourceManager.Value != null ? _fileBasedResourceManager.Value.GetString(k, Resources.Resources.Culture) : null;
                    }
                    catch
                    {
                    }
                    return text ??
                        Resources.Resources.ResourceManager.GetString(k, Resources.Resources.Culture) ??
                        null;
                }) ?? key;
            }
        }

        private KprResourceManager()
        {
            _cache = new ConcurrentDictionary<string, string>(4, 0, StringComparer.OrdinalIgnoreCase);
            _fileBasedResourceManager = new Lazy<ResourceManager>(() =>
                Resources.Resources.Culture != null &&
                File.Exists(Path.Combine(AppConfigSerializer.AppDataDirectory, string.Format("{0}.{1}.resources", Util.KeePassRDP, Resources.Resources.Culture.Name))) ?
                    ResourceManager.CreateFileBasedResourceManager(
                    Util.KeePassRDP,
                    AppConfigSerializer.AppDataDirectory,
                    null) :
                    null,
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public void Translate(Control control)
        {
            control.SuspendLayout();
            control.Text = this[control.Text];
            control.ResumeLayout(false);
        }

        public void TranslateMany(params Control[] controls)
        {
            foreach (var control in controls)
                Translate(control);
        }

        public void Translate(ToolStripItem item)
        {
            item.Text = this[item.Text];
        }

        public void TranslateMany(params ToolStripItem[] items)
        {
            foreach (var item in items)
                Translate(item);
        }

        public void ClearCache()
        {
            _cache.Clear();
        }

        public void Dispose()
        {
            ClearCache();
            Resources.Resources.ResourceManager.ReleaseAllResources();
            if (_fileBasedResourceManager.IsValueCreated && _fileBasedResourceManager.Value != null)
                _fileBasedResourceManager.Value.ReleaseAllResources();
        }
    }
}