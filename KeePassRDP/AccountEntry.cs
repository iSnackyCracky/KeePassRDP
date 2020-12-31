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

namespace KeePassRDP
{
    class AccountEntry
    {
        public AccountEntry(string path, string title, string username, string notes, int uidhash)
        {
            Path = path;
            Title = title;
            Username = username;
            Notes = notes;
            UidHash = uidhash;
        }
        public string Path { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Notes { get; set; }
        public int UidHash { get; set; }
    }
}
