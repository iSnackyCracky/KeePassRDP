<#
 #  Copyright (C) 2018 - 2024 iSnackyCracky, NETertainer
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

Add-Type -Assembly System.IO.Compression
Add-Type -Assembly System.IO.Compression.FileSystem

[System.Environment]::CurrentDirectory = (Get-Location).Path;

$ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("KeePassRDP\bin\Release\KeePassRDP.dll");
$zipFile = [System.IO.File]::Open("KeePassRDP_v$($ver.ProductVersion).zip", [System.IO.FileMode]::Create);
$archive = New-Object System.IO.Compression.ZipArchive($zipFile, [System.IO.Compression.ZipArchiveMode]::Create, $true);

@("KeePassRDP\bin\Release\KeePassRDP.plgx", "COPYING") | %{
    $archiveEntry = $archive.CreateEntry([System.IO.Path]::GetFileName($_));
    $archiveEntryStream = $archiveEntry.Open();
    $archiveFile = [System.IO.File]::Open($_, [System.IO.FileMode]::Open);
    $archiveFile.CopyTo($archiveEntryStream);
    $archiveFile.Dispose();
    $archiveEntryStream.Dispose();
}

$archive.Dispose();
$zipFile.Dispose();

Start-Sleep -Seconds 1
& iexpress /n /q /m KeePassRDP\KeePassRDPexe.SED