using System;

namespace KeePassRDP
{
    public class KprCredential : CredentialManagement.Credential
    {
        public Guid GUID { get; } = new Guid();
        public int TTL { get; private set; }
        public bool IsValid { get; private set; } = true;

        public KprCredential(string username, string password, string target, CredentialManagement.CredentialType type, int ttl)
        {
            Username = username;
            Password = password;
            Target = target;
            Type = type;
            TTL = ttl;
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
