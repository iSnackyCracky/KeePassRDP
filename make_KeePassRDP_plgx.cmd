@echo off

REM create the KeePassRDP.plgx file


rmdir /S /Q KeePassRDP\bin\
rmdir /S /Q KeePassRDP\obj\
xcopy /E /I KeePassRDP C:\tmp\KeePassRDP

KeePass.exe --plgx-create C:\tmp\KeePassRDP
del /F /Q ..\KeePassRDP.plgx
copy C:\tmp\KeePassRDP.plgx ..\KeePassRDP.plgx
rmdir /S /Q C:\tmp\KeePassRDP
del /F /Q C:\tmp\KeePassRDP.plgx
powershell -ExecutionPolicy Bypass -Command ".\make_KeePassRDP_ver.ps1"
pause
