@echo off
echo Enabling Virtual Terminal support...

REM Run the command to enable Virtual Terminal support
reg add HKCU\Console /v VirtualTerminalLevel /t REG_DWORD /d 1

echo Virtual Terminal support has been enabled.
pause