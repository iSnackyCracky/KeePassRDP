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

using KeePass.App.Configuration;
using System.IO;

namespace KeePassRDP.Commands
{
    internal class FreeRdpCommand : Command<FreeRdpCommand>, IMstscCommand
    {
        [CommandArgument(Position = 1)]
        public string Filename { get; set; }

        [CommandArgument(Parameter = "v", Delimiter = ':')]
        public string HostPort { get; set; }

        [CommandArgument(Parameter = "gateway:g", Delimiter = ':')]
        public string Gateway { get; set; }

        [CommandArgument(Parameter = "admin", Prefix = '+')]
        public bool? Admin { get; set; }

        [CommandArgument(Parameter = "f", Prefix = '+')]
        public bool? Fullscreen { get; set; }

        [CommandArgument(Parameter = "w", Delimiter = ':')]
        public int? Width { get; set; }

        [CommandArgument(Parameter = "h", Delimiter = ':')]
        public int? Height { get; set; }

        [CommandArgument(Parameter = "span", Prefix = '+')]
        public bool? Span { get; set; }

        [CommandArgument(Parameter = "multimon")]
        public bool? Multimon { get; set; }

        [CommandArgument(Parameter = "restricted-admin", Prefix = '+')]
        public bool? RestrictedAdmin { get; set; }

        public bool? Public { get; set; }
        public bool? RemoteGuard { get; set; }
        public string Shadow { get; set; }
        public bool? Control { get; set; }
        public bool? NoConsentPrompt { get; set; }

        public FreeRdpCommand() : this(null)
        {
        }

        public FreeRdpCommand(string executable) : base(string.IsNullOrWhiteSpace(executable) ? Path.Combine(AppConfigSerializer.AppDataDirectory, "wfreerdp.exe") : executable)
        {
        }
    }
}
