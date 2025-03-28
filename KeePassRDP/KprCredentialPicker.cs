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

using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Utility;
using KeePassRDP.Extensions;
using KeePassRDP.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassRDP
{
    internal class KprCredentialPicker : IDisposable
    {
        private static readonly Lazy<Icon> _icon = new Lazy<Icon>(
            () => IconUtil.ExtractIcon(KeePassRDPExt.MstscPath, 0, UIUtil.GetSmallIconSize().Height),
            LazyThreadSafetyMode.ExecutionAndPublication);

        internal Lazy<KprCredentialPickerForm> CredentialPickerForm { get { return _credentialPickerForm; } }

        private readonly IPluginHost _host;
        private readonly KprConfig _config;
        private readonly Lazy<KprCredentialPickerForm> _credentialPickerForm;
        private readonly HashSet<PwUuid> _groupUUIDs;
        private readonly HashSet<PwUuid> _excludedGroupUUIDs;
        private readonly StringBuilder _regexBuilder;
        private readonly WeakReference _iconRef;

        private PwUuid _lastResult;
        private readonly List<PwEntry> _pes;
        private KprEntrySettings _peSettings;
        private bool _defaultRegexIncluded;

        public KprCredentialPicker(IPluginHost host, KprConfig config)
        {
            _host = host;
            _config = config;
            _credentialPickerForm = new Lazy<KprCredentialPickerForm>(() => new KprCredentialPickerForm(_config, _host)
            {
                Icon = _icon.Value
            }, LazyThreadSafetyMode.ExecutionAndPublication);
            _groupUUIDs = new HashSet<PwUuid>();
            _excludedGroupUUIDs = new HashSet<PwUuid>();
            _regexBuilder = new StringBuilder();
            _iconRef = new WeakReference(null);
            _lastResult = null;
            _pes = new List<PwEntry>();
            _peSettings = null;
            _defaultRegexIncluded = false;
        }

        public void Dispose()
        {
            if (_credentialPickerForm.IsValueCreated)
                _credentialPickerForm.Value.Dispose();
        }

        /// <summary>
        /// Clear <see cref="KprCredentialPicker"/>.
        /// </summary>
        public void Clear()
        {
            _excludedGroupUUIDs.Clear();
            _groupUUIDs.Clear();
            _regexBuilder.Clear();
            _pes.Clear();
            _defaultRegexIncluded = false;
        }

        /// <summary>
        /// Reset <see cref="KprCredentialPicker"/> for next usage.
        /// </summary>
        public void Reset()
        {
            Clear();
            _peSettings = null;
            _lastResult = null;
            if (_credentialPickerForm.IsValueCreated)
                _credentialPickerForm.Value.Reset();
        }

        /// <summary>
        /// Add <see cref="PwEntry"/> to select credentials for, respecting <see cref="KprEntrySettings"/>.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="entrySettings"></param>
        /// <param name="triggerGroup"></param>
        public void AddEntry(PwEntry entry, KprEntrySettings entrySettings, string triggerGroup = Util.DefaultTriggerGroup, bool recursive = true)
        {
            // When entry parent group is trigger group add trigger parent group for recursive search.
            if (entrySettings.CpRecurseGroups && Util.InRdpSubgroup(entry, triggerGroup, recursive))
            {
                var rdpGroup = entry.ParentGroup;
                if (recursive)
                    while (rdpGroup != null && !string.Equals(rdpGroup.Name, triggerGroup, StringComparison.Ordinal))
                        rdpGroup = rdpGroup.ParentGroup;
                if (rdpGroup != null && rdpGroup.ParentGroup != null)
                    _groupUUIDs.Add(rdpGroup.ParentGroup.Uuid);
            }

            // Add unique list of included group UUIDs.
            _groupUUIDs.UnionWith(entrySettings.CpGroupUUIDs.Select(uuidString => new PwUuid(MemUtil.HexStringToByteArray(uuidString))));

            // Add unique list of excluded group UUIDs.
            _excludedGroupUUIDs.UnionWith(entrySettings.CpExcludedGroupUUIDs.Select(uuidString => new PwUuid(MemUtil.HexStringToByteArray(uuidString))));

            // Add recursive groups if enabled.
            if (entrySettings.CpRecurseGroups && _groupUUIDs.Count > 0)
                _groupUUIDs.UnionWith(_groupUUIDs
                    .Select(uuid => _host.Database.RootGroup.FindGroup(uuid, true))
                    .Where(group => group != null)
                    .SelectMany(childs => childs.GetFlatGroupList())
                    .Select(child => child.Uuid)
                    .ToList());

            _pes.Add(entry);
            _peSettings = entrySettings;

            AddRegEx();
        }

        /// <summary>
        /// Show <see cref="KprCredentialPickerForm"/> and return selected <see cref="PwEntry"/>.
        /// </summary>
        /// <param name="includeSelected">Switch for including selected <see cref="PwEntry"/> with credentials.</param>
        /// <returns><see cref="PwEntry"/>, <see langword="null"/></returns>
        public PwEntry GetCredentialEntry(bool includeSelected = false, bool resolveReferences = false)
        {
            PwEntry pe = null;
            _lastResult = null;

            // Remove excluded group UUIDs.
            _groupUUIDs.ExceptWith(_excludedGroupUUIDs);

            if (_groupUUIDs.Count > 0)
            {
                IEnumerable<PwEntry> accountEntries = null;
                var compiledRegex = _regexBuilder.Length > 0 ? new Regex(_regexBuilder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Singleline) : null;

                if (compiledRegex != null)
                {
                    var rootGroup = _host.Database.RootGroup;
                    var activeDatabase = resolveReferences ? _host.MainWindow.ActiveDatabase : null;
                    accountEntries = _groupUUIDs
                        .Select(uuid => rootGroup.FindGroup(uuid, true))
                        .Where(group => group != null)
                        .SelectMany(group => group.GetRdpAccountEntries(compiledRegex, activeDatabase))
                        .Where(entry => !entry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty);
                }

                if (includeSelected)
                {
                    var entries = _pes.Where(entry => !Util.IsEntryIgnored(entry) && !entry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty);
                    accountEntries = accountEntries == null ?
                        entries :
                        accountEntries.Concat(entries);
                }

                if (accountEntries != null && accountEntries.Any())
                {
                    // Fill KprCredentialPickerForm with the entries found.
                    var frmCredPick = _credentialPickerForm.Value;
                    if (!frmCredPick.Created)
                        frmCredPick.CreateControl();
                    frmCredPick.RdpAccountEntries = accountEntries;
                    frmCredPick.ConnPwEntry = _pes.FirstOrDefault();

                    frmCredPick.StartPosition = FormStartPosition.CenterParent;
                    frmCredPick.Location = Point.Empty;
                    frmCredPick.Icon = _iconRef.Target as Icon ?? _icon.Value;

                    // Show KprCredentialPickerForm, get result and reset the dialog.
                    if (_config.CredPickerSecureDesktop)
                    {
                        using (var mrs = new ManualResetEventSlim(false))
                        {
                            Task.Factory.StartNew(() =>
                            {
                                //SecureDesktop.Instance.Prepare();

                                var primaryScreen = Screen.FromControl(_host.MainWindow);
                                frmCredPick.StartPosition = FormStartPosition.CenterScreen;
                                frmCredPick.Location = primaryScreen.Bounds.Location;

                                SecureDesktop.Instance.Run(_ =>
                                {
                                    try
                                    {
                                        _lastResult = frmCredPick.ShowDialog() == DialogResult.OK ? frmCredPick.ReturnUuid : null;
                                    }
                                    catch
                                    {
                                        _lastResult = null;
                                        throw;
                                    }
                                    finally
                                    {
                                        mrs.Set();
                                    }

                                    return SecureDesktop.CompletedTask;
                                });
                            }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                            try
                            {
                                if (!mrs.Wait(TimeSpan.FromSeconds(90)))
                                    throw new TimeoutException();

                                SecureDesktop.Instance.Wait(30); // Maybe wait forever?
                            }
                            catch (TimeoutException)
                            {
                                frmCredPick.Close();
                                _lastResult = null;
                            }
                            catch
                            {
                                _lastResult = null;
                                throw;
                            }
                            finally
                            {
                                frmCredPick.Reset();
                                if (_iconRef.Target is Icon)
                                    using (var icon = _iconRef.Target as Icon)
                                        IconUtil.DestroyIcon(icon.Handle);
                                _iconRef.Target = null;
                            }
                        }
                    }
                    else
                    {
                        if (!_config.MstscSecureDesktop)
                            if (SecureDesktop.IsValueCreated && SecureDesktop.Instance.IsAlive && !SecureDesktop.Instance.IsCancellationRequested)
                                SecureDesktop.Instance.Cancel();

                        try
                        {
                            _lastResult = frmCredPick.ShowDialog(_host.MainWindow) == DialogResult.OK ? frmCredPick.ReturnUuid : null;
                        }
                        catch
                        {
                            _lastResult = null;
                            throw;
                        }
                        finally
                        {
                            frmCredPick.Reset();
                            if (_iconRef.Target is Icon)
                                using (var icon = _iconRef.Target as Icon)
                                    IconUtil.DestroyIcon(icon.Handle);
                            _iconRef.Target = null;
                        }
                    }

                    var lastResult = _lastResult;
                    _lastResult = null;

                    if (lastResult == null)
                        throw new OperationCanceledException();

                    pe = accountEntries.First(account => account.Uuid.Equals(lastResult));
                }
                else if (_pes.Count == 1)
                    pe = _pes.FirstOrDefault();
            }

            return pe;
        }

        public void SetIcon(Image image)
        {
            var bitmap = image as Bitmap;

            if (bitmap is Bitmap)
                _iconRef.Target = Icon.FromHandle(bitmap.GetHicon());
            else
                _iconRef.Target = null;
        }

        private void AddRegEx()
        {
            if (!_defaultRegexIncluded && _peSettings.CpIncludeDefaultRegex)
            {
                _defaultRegexIncluded = true;
                _regexBuilder
                    .Append(_regexBuilder.Length > 0 ? "|.*(" : ".*(")
                    .Append(_config.CredPickerRegExPre)
                    .Append(").*(")
                    .Append(_config.CredPickerRegExPost)
                    .Append(").*");
            }

            foreach (var regex in _peSettings.CpRegExPatterns)
            {
                if (_regexBuilder.Length > 0)
                    _regexBuilder.Append("|");
                _regexBuilder.Append(regex);
            }
        }
    }
}