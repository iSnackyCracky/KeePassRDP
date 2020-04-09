using KeePassLib.Security;
using System;

namespace KeePassRDP
{
    public class Credential
    {
        public Guid GUID { get; } = new Guid();
        public ProtectedString Username { get; }
        public ProtectedString Password { get; }
        public ProtectedString URI { get; }
        public int TTL { get; private set; } = 10000;
        public bool IsValid { get; private set; } = true;

        public Credential(string username, string password, string uri)
        {
            Username = new ProtectedString(true, username);
            Password = new ProtectedString(true, password);
            URI = new ProtectedString(true, uri);
        }
        public Credential(string username, string password, string uri, int ttl)
        {
            Username = new ProtectedString(true, username);
            Password = new ProtectedString(true, password);
            URI = new ProtectedString(true, uri);
            TTL = ttl;
        }
        public Credential(ProtectedString username, ProtectedString password, ProtectedString uri)
        {
            Username = username;
            Password = password;
            URI = uri;
        }
        public Credential(ProtectedString username, ProtectedString password, ProtectedString uri, int ttl)
        {
            Username = username;
            Password = password;
            URI = uri;
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
            else
            {
                TTL -= amount;
            }

            return TTL;
        }

        public bool Invalidate()
        {
            IsValid = false;
            return false;
        }
    }
}
