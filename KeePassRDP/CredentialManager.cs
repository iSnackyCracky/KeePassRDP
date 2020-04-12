using System;
using System.Timers;
using System.Collections.Generic;

namespace KeePassRDP
{
    internal class CredentialManager
    {
        private List<Credential> _credentials;
        private Timer _timer = new Timer(1000);

        private List<KprCredential> _credentials = new List<KprCredential>();

        public double TimerInterval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }
        public int CredentialCount {
            get { return _credentials.Count; }
        }

        public CredentialManager() { }
        public CredentialManager(KprCredential credential)
        {
            Add(credential);
        }
        public CredentialManager(List<KprCredential> credentials)
        {
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

        public void RemoveAll()
        {
            foreach (KprCredential cred in _credentials) { Remove(cred); }
        }

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
