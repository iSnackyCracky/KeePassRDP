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

namespace KeePassRDP.Commands
{
    internal class MstscCommand : Command<MstscCommand>
    {
        public override string ExecutablePath { get { return KeePassRDPExt.MstscPath; } }

        [CommandArgument(Position = 1)]
        public string Filename { get; set; }

        [CommandArgument(Parameter = "v", Delimiter = ':')]
        public string HostPort { get; set; }

        [CommandArgument(Parameter = "g", Delimiter = ':')]
        public string Gateway { get; set; }

        [CommandArgument(Parameter = "admin")]
        public bool? Admin { get; set; }

        [CommandArgument(Parameter = "f")]
        public bool? Fullscreen { get; set; }

        [CommandArgument(Parameter = "w", Delimiter = ':')]
        public int? Width { get; set; }

        [CommandArgument(Parameter = "h", Delimiter = ':')]
        public int? Height { get; set; }

        [CommandArgument(Parameter = "public")]
        public bool? Public { get; set; }

        [CommandArgument(Parameter = "span")]
        public bool? Span { get; set; }

        [CommandArgument(Parameter = "multimon")]
        public bool? Multimon { get; set; }

        [CommandArgument(Parameter = "restrictedAdmin")]
        public bool? RestrictedAdmin { get; set; }

        [CommandArgument(Parameter = "remoteGuard")]
        public bool? RemoteGuard { get; set; }

        [CommandArgument(Parameter = "shadow", Delimiter = ':')]
        public string Shadow { get; set; }

        [CommandArgument(Parameter = "control")]
        public bool? Control { get; set; }

        [CommandArgument(Parameter = "noConsentPrompt")]
        public bool? NoConsentPrompt { get; set; }
    }
}
