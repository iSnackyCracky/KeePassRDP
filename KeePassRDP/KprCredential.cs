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
 *  along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

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
