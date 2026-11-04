@echo off
setlocal
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
  echo The .NET Framework compiler is not installed. Install the .NET Framework 4.8 developer tools, then try again.
  exit /b 1
)
"%CSC%" /nologo /target:winexe /platform:anycpu /optimize+ /out:"%~dp0Musiko.exe" /reference:System.dll /reference:System.Windows.Forms.dll /resource:"%~dp0index.html",Musiko.Player.html "%~dp0MusikoLauncher.cs"
if errorlevel 1 exit /b %errorlevel%
echo Created: %~dp0Musiko.exe
