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

using KeePass.Resources;
using KeePass.UI;
using KeePassRDP.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace KeePassRDP
{
    /// <summary>
    /// Manager for adding and removing <see cref="KprCredential"/> from Windows vault.
    /// </summary>
    /// <typeparam name="T">
    /// Type of managed credentials. Needs to implement <see cref="IKprCredential"/>.
    /// </typeparam>
    internal class KprCredentialManager<T> : IList<T> where T : IKprCredential
    {
        private const int _interval = 1000;

        private enum Action : byte
        {
            Add,
            Remove
        }

        /// <summary>
        /// Interval in milliseconds to check for and remove invalid credentials.
        /// </summary>
        public double Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        /// <summary>
        /// Switch to enable/disable checking for invalid credentials.
        /// </summary>
        public bool Enabled
        {
            get { return _timer.Enabled; }
            set { _timer.Enabled = value; }
        }

        /// <inheritdoc/>
        public int Count
        {
            get { return _credentials.Count; }
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            get
            {
                return _credentials[index];
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        private readonly List<T> _credentials;
        private readonly Timer _timer;
        private readonly KprConfig _config;

        public KprCredentialManager(KprConfig config) : this(null, config)
        {
        }

        public KprCredentialManager(IEnumerable<T> credentials, KprConfig config)
        {
            _config = config;
            _credentials = credentials == null ? new List<T>() : new List<T>(credentials);
            _timer = new Timer(_interval);
            _timer.Elapsed += OnTimer_Elapsed;

            if (!NativeCredentials.CredCanPersist(_config.CredVaultUseWindows ? NativeCredentials.CRED_TYPE.DOMAIN_PASSWORD : NativeCredentials.CRED_TYPE.GENERIC))
                VistaTaskDialog.ShowMessageBoxEx(
                    KprResourceManager.Instance["Unable to persist credentials in Windows vault."],
                    null,
                    Util.KeePassRDP + " - " + KPRes.FatalError,
                    VtdIcon.Error,
                    null, null, 0, null, 0);

            NativeCredentials.Credential[] ncredentials;
            if (NativeCredentials.CredEnumerate(null, out ncredentials) && ncredentials.Length > 0)
            {
                _credentials.AddRange(ncredentials.Where(cred => cred.Comment == KprCredential.CredentialComment).Select(cred => new KprCredential(cred)).Cast<T>());
                foreach (var ncred in ncredentials.Where(cred => cred.Comment != KprCredential.CredentialComment))
                    ncred.ZeroMemory();
            }

            if (_credentials.Count > 0)
                ManageTimer(Action.Add);
        }

        /// <inheritdoc/>
        public void Add(T credential)
        {
            credential.Write(_config.CredVaultOverwriteExisting);
            credential.ZeroMemory();
            _credentials.Add(credential);
            ManageTimer(Action.Add);
        }

        /// <inheritdoc/>
        public void Insert(int index, T credential)
        {
            credential.Write(_config.CredVaultOverwriteExisting);
            credential.ZeroMemory();
            _credentials.Insert(index, credential);
            ManageTimer(Action.Add);
        }

        /// <inheritdoc cref="List{T}.AddRange(IEnumerable{T})"/>
        public void AddRange(IEnumerable<T> credentials)
        {
            if (!credentials.Any())
                return;
            foreach (var credential in credentials)
            {
                credential.Write(_config.CredVaultOverwriteExisting);
                credential.ZeroMemory();
                _credentials.Add(credential);
            }
            ManageTimer(Action.Add);
        }

        /// <inheritdoc cref="Remove(T)"/>
        public bool Remove(Guid credentialGuid)
        {
            var result = false;
            foreach (var credential in _credentials.FindAll(x => x.GUID == credentialGuid))
                using (credential)
                    if (_credentials.Remove(credential))
                        result = true;
            if (result)
                ManageTimer(Action.Remove);
            return result;
        }

        /// <inheritdoc/>
        public bool Remove(T credential)
        {
            using (credential)
                if (_credentials.Remove(credential))
                {
                    ManageTimer(Action.Remove);
                    return true;
                }
            return false;
        }

        /// <inheritdoc cref="List{T}.RemoveAll(Predicate{T})"/>
        public int RemoveAll(Predicate<T> match)
        {
            var count = 0;
            foreach (var credential in _credentials.FindAll(match))
            {
                using (credential)
                    if (_credentials.Remove(credential))
                        count++;
            }
            if (count > 0)
                ManageTimer(Action.Remove);
            return count;
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            using (_credentials[index])
                _credentials.RemoveAt(index);
            ManageTimer(Action.Remove);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            foreach (var credential in _credentials)
                credential.Dispose();
            _credentials.Clear();
            ManageTimer(Action.Remove);
        }

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            return _credentials.IndexOf(item);
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return _credentials.Contains(item);
        }

        /// <inheritdoc cref="List{T}.FindAll(Predicate{T})"/>
        public List<T> FindAll(Predicate<T> match)
        {
            return _credentials.FindAll(match);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _credentials.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return _credentials.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void OnTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RemoveAll(x => !x.IsValid);
        }

        private void ManageTimer(Action action)
        {
            Enabled = action == Action.Add || Count > 0;
        }
    }
}