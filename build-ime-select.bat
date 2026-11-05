@echo off
setlocal

set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if not exist "%CSC%" (
    echo C# compiler not found: "%CSC%" 1>&2
    exit /b 1
)

if not exist "%~dp0out\" (
    mkdir "%~dp0out"
    if errorlevel 1 exit /b 1
)

"%CSC%" /nologo /target:exe /optimize+ /out:"%~dp0out\ime-select.exe" "%~dp0imeSelect.cs"
if errorlevel 1 exit /b 1

echo Built "%~dp0out\ime-select.exe"
