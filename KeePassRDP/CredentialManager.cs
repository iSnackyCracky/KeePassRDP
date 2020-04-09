using System;
using System.Timers;
using System.Collections.Generic;

namespace KeePassRDP
{
    internal class CredentialManager
    {
        private List<Credential> _credentials;
        private Timer _timer = new Timer(1000);

        public double TimerInterval {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }
        public int CredentialCount {
            get { return _credentials.Count; }
        }

        public CredentialManager() { }
        public CredentialManager(Credential credential) { _credentials.Add(credential); }
        public CredentialManager(List<Credential> credentials) { _credentials.AddRange(credentials); }

        public void Add(Credential credential) { _credentials.Add(credential); }
        public void Add(List<Credential> credentials) { _credentials.AddRange(credentials); }

        public void Remove(Guid credentialGuid) { _credentials.RemoveAll(x => x.GUID.Equals(credentialGuid)); }
        public void Remove(Credential credential) { _credentials.Remove(credential); }
        public void Remove(List<Credential> credentials)
        {
            foreach (var credential in credentials)
            {
                _credentials.Remove(credential);
            }
        }
    }
}
