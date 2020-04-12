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