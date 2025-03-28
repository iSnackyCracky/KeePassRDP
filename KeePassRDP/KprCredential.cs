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

using KeePass.Resources;
using KeePass.UI;
using KeePassLib.Security;
using KeePassRDP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;

namespace KeePassRDP
{
    /// <summary>
    /// Extends <see cref="NativeCredentials.Credential"/> for consumption by <see cref="KprCredentialManager{T}"/>
    /// </summary>
    internal sealed class KprCredential : NativeCredentials.Credential, IKprCredential
    {
        public const string CredentialComment = Util.KeePassRDP;

        public Guid GUID { get { return _guid; } }
        //public DateTimeOffset ValidUntil { get { return _validUntil; } }
        public bool IsValid { get { return _validUntil + _increase >= DateTimeOffset.UtcNow; } }

        private readonly Guid _guid;
        private readonly TimeSpan _ttl;
        private TimeSpan _increase;
        private DateTimeOffset _validUntil;

        internal KprCredential(ProtectedString username, SecureString password, string targetName, NativeCredentials.CRED_TYPE type, int ttl = 0) : base()
        {
            if (string.IsNullOrEmpty(targetName))
                throw new ArgumentNullException("targetName");

            _guid = Guid.NewGuid();
            _ttl = ttl > 0 ? TimeSpan.FromSeconds(ttl) : TimeSpan.Zero;
            _increase = TimeSpan.Zero;
            _validUntil = _ttl.TotalMilliseconds > 0 ? DateTimeOffset.UtcNow + _ttl : DateTimeOffset.MaxValue;

            var usernameString = string.Empty;
            if (!username.IsEmpty)
            {
                var usernameChars = username.ReadChars();
                usernameString = new string(usernameChars);
                MemoryUtil.SecureZeroMemory(usernameChars);
            }

            Type = type;
            TargetName = targetName;
            UserName = usernameString;
            //CredentialBlob = password.IsEmpty ? string.Empty : new string(password.ReadChars());
            CredentialBlob = password.Copy();
            Persist = NativeCredentials.CRED_PERSIST.SESSION;
            Attributes = new Dictionary<string, object>
            {
                { "ValidUntil", _validUntil }
            };
            Comment = CredentialComment;
        }

        internal KprCredential(NativeCredentials.Credential credential) : base()
        {
            _guid = Guid.NewGuid();
            _ttl = TimeSpan.Zero;
            _increase = TimeSpan.Zero;
            if (credential.Attributes.ContainsKey("ValidUntil"))
                _validUntil = (DateTimeOffset)credential.Attributes["ValidUntil"];

            Flags = credential.Flags;
            Type = credential.Type;
            TargetName = credential.TargetName;
            UserName = credential.UserName;
            CredentialBlob = credential.CredentialBlob != null && credential.CredentialBlob.Length > 0 ? credential.CredentialBlob.Copy() : null;
            Persist = credential.Persist;
            Attributes = credential.Attributes;
            Comment = credential.Comment;
        }

        public void IncreaseTTL(TimeSpan ttl)
        {
            if (ttl == TimeSpan.MaxValue)
            {
                _increase = TimeSpan.Zero;
                _validUntil = DateTimeOffset.MaxValue;

                return;
            }

            if (ttl.TotalMilliseconds > 0)
                _increase += ttl;
        }

        public void IncreaseTTL(int seconds)
        {
            if (seconds > 0)
                IncreaseTTL(TimeSpan.FromSeconds(seconds));
        }

        public void DecreaseTTL(TimeSpan ttl)
        {
            if (ttl == TimeSpan.MaxValue)
            {
                _increase = TimeSpan.Zero;
                _validUntil = DateTimeOffset.MinValue;

                return;
            }

            if (ttl.TotalMilliseconds > 0)
                _increase -= ttl;
        }

        public void DecreaseTTL(int seconds)
        {
            if (seconds > 0)
                DecreaseTTL(TimeSpan.FromSeconds(seconds));
        }

        public void ResetTTL()
        {
            _increase = TimeSpan.Zero;
        }

        public void ResetValidUntil()
        {
            _validUntil = _ttl.TotalMilliseconds > 0 ? DateTimeOffset.UtcNow + _ttl : DateTimeOffset.MaxValue;
            ResetTTL();
        }

        /// <summary>
        /// Writes <see cref="KprCredential"/> to Windows vault.
        /// </summary>
        /// <param name="force">Force overwriting of existing credentials.</param>
        public void Write(bool force = false)
        {
            NativeCredentials.Credential cred = null;

            try
            {
                if (force ||
                    !NativeCredentials.CredRead(TargetName, Type, out cred) ||
                    cred == null ||
                    (cred.Persist == Persist &&
                        cred.Comment == Comment &&
                        (DateTimeOffset)cred.Attributes["ValidUntil"] < _validUntil))
                    NativeCredentials.CredWrite(this);
            }
            catch (Win32Exception ex)
            {
                VistaTaskDialog.ShowMessageBoxEx(
                    string.Format(KprResourceManager.Instance["Failed to store credentials in vault: {0}"], ex.Message),
                    null,
                    Util.KeePassRDP + " - " + KPRes.Warning,
                    VtdIcon.Warning,
                    null, null, 0, null, 0);
            }
            finally
            {
                if (cred != null)
                    cred.ZeroMemory();
            }
        }

        public void Dispose()
        {
            NativeCredentials.Credential cred = null;

            try
            {
                if (NativeCredentials.CredRead(TargetName, Type, out cred) &&
                    cred != null &&
                    cred.Persist == Persist &&
                    cred.Comment == Comment &&
                    (DateTimeOffset)cred.Attributes["ValidUntil"] == _validUntil)
                    NativeCredentials.CredDelete(TargetName, Type);
            }
            catch (Win32Exception) { }

            if (cred != null)
                cred.ZeroMemory();

            ZeroMemory();

            if (CredentialBlob != null)
                CredentialBlob.Dispose();

            _validUntil = DateTimeOffset.MinValue;
            ResetTTL();
        }
    }

    public interface IKprCredential : IDisposable
    {
        Guid GUID { get; }
        bool IsValid { get; }
        void Write(bool force = false);
        void ZeroMemory();
    }
}