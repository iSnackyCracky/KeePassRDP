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

using System;

namespace KeePassRDP.Commands
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class CommandArgumentAttribute : Attribute
    {
        public int Position { get; set; }
        public string Parameter { get; set; }
        public char Delimiter { get; set; }
        public char Prefix { get; set; }

        public CommandArgumentAttribute() : this('/') { }

        public CommandArgumentAttribute(char prefix)
        {
            Position = int.MaxValue;
            Parameter = string.Empty;
            Delimiter = char.MinValue;
            Prefix = prefix;
        }
    }
}
