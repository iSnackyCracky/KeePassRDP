/*
 *  Copyright (C) 2018 - 2025 iSnackyCracky, NETertainer
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
using System.IO;
using System.Linq;

namespace KeePassRDP.Commands
{
    internal class CustomCommand : Command
    {
        public override string Executable { get { return Path.GetFullPath(Environment.ExpandEnvironmentVariables(_executable)); } }

        public ICommand Command { get { return _command; } }

        private readonly ICommand _command;

        public CustomCommand(string inputString) : this(inputString.Split(new[] { ':' }, 3).Skip(1))
        {
        }

        private CustomCommand(IEnumerable<string> commandString) : base(commandString.FirstOrDefault())
        {
            _command = CreateInstance(string.Join(":", commandString)) ?? this;
        }
    }
}
