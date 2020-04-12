using System;

namespace KeePassRDP
{
    public class KprCredential : CredentialManagement.Credential
    {
        public Guid GUID { get; private set; }
        public int TTL { get; private set; }
        public bool IsValid { get; private set; }

        public KprCredential(string username, string password, string target, CredentialManagement.CredentialType type, int ttl)
        {
            GUID = new Guid();
            IsValid = true;
            Username = username;
            Password = password;
            Target = target;
            Type = type;
            TTL = ttl * 1000;
        }

        public int IncreaseTTL(int amount)
        {
            TTL += amount;
            return TTL;
        }
        public int DecreaseTTL(int amount)
        {
            if (amount >= TTL)
            {
                TTL = 0;
                Invalidate();
            }
            else { TTL -= amount; }

            return TTL;
        }

        public void Invalidate()
        {
            if (IsValid)
            {
                Delete();
                IsValid = false;
            }
        }
    }
}
