#  Copyright (C) 2018-2020 iSnackyCracky
#
#  This file is part of KeePassRDP.
#
#  KeePassRDP is free software: you can redistribute it and/or modify
#  it under the terms of the GNU General Public License as published by
#  the Free Software Foundation, either version 3 of the License, or
#  (at your option) any later version.
#
#  KeePassRDP is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#  GNU General Public License for more details.
#
#  You should have received a copy of the GNU General Public License
#  along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

$VersionFilePath = ".\KeePassRDP.ver"
[System.Version]$KeePassRDPver = (Get-Content "KeePassRDP\Properties\AssemblyInfo.cs" | Select-String -Pattern "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+")[0].Matches.Value

if (Test-Path -Path '.\KeePassRDP.plgx' -PathType Leaf) {
    # create ZIP-file for release-upload
    $zipName = "KeePassRDP_v$( $KeePassRDPver.Major ).$( $KeePassRDPver.Minor ).$( $KeePassRDPver.Build ).zip"
    Start-Process -FilePath '.\7z.exe' -ArgumentList 'a', ".\$zipName", '.\KeePassRDP.plgx' -Wait
}

# Create KeePassRDP Version File for update checking
":" |Out-File -FilePath $VersionFilePath -Encoding utf8
"KeePassRDP:$( $KeePassRDPver.ToString() )" |Out-File -FilePath $VersionFilePath -Encoding utf8 -Append
":" |Out-File -FilePath $VersionFilePath -Encoding utf8 -Append