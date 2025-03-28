<#
 #  Copyright (C) 2018 - 2025 iSnackyCracky, NETertainer
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
 #  along with KeePassRDP.  If not, see <http://www.gnu.org/licenses/>.
 #
 #>

[System.Environment]::CurrentDirectory = (Get-Location).Path;

$Version = ([System.Diagnostics.FileVersionInfo]::GetVersionInfo("KeePassRDP\bin\Release\KeePassRDP.dll")).ProductVersion
$FileHash = $(Get-FileHash -Algorithm SHA256 "KeePassRDPSetup\bin\Release\KeePassRDP_v$Version.msi").Hash
$FilePath = "KeePassRDPSetup\winget\KeePassRDP.KeePassRDP.installer.yaml"
$Content = Get-Content -Encoding UTF8 $FilePath
$Content = $Content -replace "###FILE_HASH###", $FileHash
Set-Content -Encoding Utf8 -Path $FilePath -Value $Content

#.\wingetcreate.exe submit -p "KeePassRDP v$Version" -t "$env:WINGET_GH_TOKEN" "KeePassRDPSetup\winget"