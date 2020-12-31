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

using System;
using System.Collections.Generic;
using System.Timers;

namespace KeePassRDP
{
    internal class CredentialManager
    {
        private const int _interval = 1000;

        private List<KprCredential> _credentials = new List<KprCredential>();
        private Timer _timer = new Timer(_interval);
        private enum _ActionType
        {
            Add,
            Remove
        }

        public double TimerInterval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }
        public int CredentialCount { get { return _credentials.Count; } }

        public CredentialManager() { _timer.Elapsed += OnTimer_Elapsed; }
        public CredentialManager(KprCredential credential)
        {
            _timer.Elapsed += OnTimer_Elapsed;
            Add(credential);
        }
        public CredentialManager(List<KprCredential> credentials)
        {
            _timer.Elapsed += OnTimer_Elapsed;
            Add(credentials);
        }

        public void Add(KprCredential credential)
        {
            credential.Save();
            _credentials.Add(credential);
            ManageTimer(_ActionType.Add);
        }
        public void Add(List<KprCredential> credentials)
        {
            foreach (KprCredential cred in credentials) { cred.Save(); }
            _credentials.AddRange(credentials);
            ManageTimer(_ActionType.Add);
        }

        public void Remove(Guid credentialGuid)
        {
            foreach (KprCredential cred in _credentials.FindAll(x => x.GUID.Equals(credentialGuid))) { cred.Invalidate(); }
            _credentials.RemoveAll(x => x.GUID.Equals(credentialGuid));
            ManageTimer(_ActionType.Remove);
        }
        public void Remove(KprCredential credential)
        {
            credential.Invalidate();
            _credentials.Remove(credential);
            ManageTimer(_ActionType.Remove);
        }
        public void Remove(List<KprCredential> credentials)
        {
            foreach (KprCredential credential in credentials)
            {
                credential.Invalidate();
                _credentials.Remove(credential);
            }
            ManageTimer(_ActionType.Remove);
        }

        public void RemoveAll() { foreach (KprCredential cred in _credentials.FindAll(x => x.IsValid)) { Remove(cred); } }

        private void ManageTimer(_ActionType action) { _timer.Enabled = action == _ActionType.Add || CredentialCount > 0; }

        private void OnTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (KprCredential cred in _credentials.FindAll(x => x.IsValid))
            {
                cred.DecreaseTTL(_interval);
                if (!cred.IsValid) { Remove(cred); }
            }
        }
    }
}
