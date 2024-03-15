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

        public Guid GUID { get; private set; }
        public DateTimeOffset ValidUntil { get; private set; }
        public bool IsValid { get { return ValidUntil + _increase >= DateTimeOffset.UtcNow; } }

        private TimeSpan _increase;

        internal KprCredential(ProtectedString username, SecureString password, string targetName, NativeCredentials.CRED_TYPE type, int ttl) : base()
        {
            if (string.IsNullOrEmpty(targetName))
                throw new ArgumentNullException("targetName");

            _increase = TimeSpan.Zero;

            GUID = Guid.NewGuid();
            ValidUntil = ttl > 0 ? DateTimeOffset.UtcNow + TimeSpan.FromSeconds(ttl) : DateTimeOffset.MaxValue;

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
                { "ValidUntil", ValidUntil }
            };
            Comment = CredentialComment;
        }

        internal KprCredential(NativeCredentials.Credential credential) : base()
        {
            _increase = TimeSpan.Zero;

            GUID = Guid.NewGuid();
            if (credential.Attributes.ContainsKey("ValidUntil"))
                ValidUntil = (DateTimeOffset)credential.Attributes["ValidUntil"];

            Flags = credential.Flags;
            Type = credential.Type;
            TargetName = credential.TargetName;
            UserName = credential.UserName;
            CredentialBlob = credential.CredentialBlob;
            Persist = credential.Persist;
            Attributes = credential.Attributes;
            Comment = credential.Comment;
        }

        public void IncreaseTTL(TimeSpan ttl)
        {
            if(ttl.TotalMilliseconds > 0)
                _increase += ttl;
        }

        public void IncreaseTTL(int seconds)
        {
            if (seconds > 0)
                IncreaseTTL(TimeSpan.FromSeconds(seconds));
        }

        public void DecreaseTTL(TimeSpan ttl)
        {
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
                        (DateTimeOffset)cred.Attributes["ValidUntil"] < ValidUntil))
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

            if (cred != null)
                cred.ZeroMemory();
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
                    (DateTimeOffset)cred.Attributes["ValidUntil"] == ValidUntil)
                    NativeCredentials.CredDelete(TargetName, Type);
            }
            catch (Win32Exception)
            {
            }

            if (cred != null)
                cred.ZeroMemory();

            ValidUntil = DateTimeOffset.MinValue;
            ResetTTL();
            ZeroMemory();

            if (CredentialBlob != null)
                CredentialBlob.Dispose();
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