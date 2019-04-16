# Create KeePassRDP Version File for update checking

$VersionFilePath = ".\KeePassRDP.ver"
$KeePassRDPver = (Get-Content "KeePassRDP\Properties\AssemblyInfo.cs" |Select-String -Pattern "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+")[0].Matches.Value

":" |Out-File -FilePath $VersionFilePath -Encoding utf8
"KeePassRDP:$KeePassRDPver" |Out-File -FilePath $VersionFilePath -Encoding utf8 -Append
":" |Out-File -FilePath $VersionFilePath -Encoding utf8 -Append