
$VersionFilePath = ".\KeePassRDP.ver"
[System.Version]$KeePassRDPver = (Get-Content "KeePassRDP\Properties\AssemblyInfo.cs" | Select-String -Pattern "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+")[0].Matches.Value



# cleanup project directory
if (Test-Path -Path '.\KeePassRDP\bin' -PathType Container) {
    Write-Output -InputObject 'Deleting \KeePassRDP\bin ...'
    Remove-Item -Path '.\KeePassRDP\bin' -Recurse -Force -Confirm:$false
}
if (Test-Path -Path '.\KeePassRDP\obj' -PathType Container) {
    Write-Output -InputObject 'Deleting \KeePassRDP\obj ...'
    Remove-Item -Path '.\KeePassRDP\obj' -Recurse -Force -Confirm:$false
}
Write-Output -InputObject 'Deleting old KeePassRDP.plgx ...'
Get-ChildItem -Path '..\' -Filter "KeePassRDP.plgx" | Remove-Item -Force -Confirm:$false
Write-Output -InputObject 'Deleting old KeePassRDP*.zip ...'
Get-ChildItem -Path '..\' -Filter "KeePassRDP*.zip" | Remove-Item -Force -Confirm:$false

Copy-Item -Path '.\KeePassRDP' -Destination 'C:\tmp\KeePassRDP' -Recurse -Force

# make KeePassRDP.plgx file
Write-Output -InputObject 'Creating KeePassRDP.plgx ...'
Start-Process -FilePath '.\KeePass.exe' -ArgumentList '--plgx-create', 'C:\tmp\KeePassRDP' -Wait -NoNewWindow

if (Test-Path -Path 'C:\tmp\KeePassRDP.plgx' -PathType Leaf) {
    Write-Output -InputObject 'Moving KeePassRDP.plgx ...'
    Move-Item -Path 'C:\tmp\KeePassRDP.plgx' -Destination '..\KeePassRDP.plgx'

    # create ZIP-file for release-upload
    $zipName = "KeePassRDP_v$( $KeePassRDPver.Major ).$( $KeePassRDPver.Minor ).$( $KeePassRDPver.Build ).zip"
    Write-Output -InputObject "Creating $zipName ..."
    Start-Process -FilePath '.\7z.exe' -ArgumentList 'a', "..\$zipName", '..\KeePassRDP.plgx' -Wait
} else {
    Write-Error -Message 'Could not find KeePassRDP.plgx - please check KeePass --plgx-create call.'
}

if (Test-Path -Path 'C:\tmp\KeePassRDP' -PathType Container) {
    Write-Output -InputObject 'Deleting temporary directory ...'
    Remove-Item -Path 'C:\tmp\KeePassRDP' -Recurse -Force -Confirm:$false
}




# Create KeePassRDP Version File for update checking
Write-Output -InputObject 'Creating KeePassRDP.ver'
":" |Out-File -FilePath $VersionFilePath -Encoding utf8
"KeePassRDP:$( $KeePassRDPver.ToString() )" |Out-File -FilePath $VersionFilePath -Encoding utf8 -Append
":" |Out-File -FilePath $VersionFilePath -Encoding utf8 -Append

Write-Output -InputObject 'DONE!'
pause