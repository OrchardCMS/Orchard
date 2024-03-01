@echo off

REM Necessary for the InstallDir variable to work inside the MsBuild-finding loop below.
SETLOCAL ENABLEDELAYEDEXPANSION

for /f "usebackq tokens=1* delims=: " %%i in (`lib\vswhere\vswhere -latest -version "[16.0,18.0)" -requires Microsoft.Component.MSBuild`) do (
  if /i "%%i"=="installationPath" (
	set InstallDir=%%j
	echo !InstallDir!
	if exist "!InstallDir!\MSBuild\Current\Bin\MSBuild.exe" (
		echo "Using MSBuild from !InstallDir!"
		set msbuild="!InstallDir!\MSBuild\Current\Bin\MSBuild.exe"
		goto build
	)
  )
)

echo "Unable to detect suitable environment. Build may not succeed."

:build

SET target=%1
SET project=%2
SET solution=%3

IF "%target%" == "" SET target=Build
IF "%project%" == "" SET project=Orchard.proj
IF "%solution%" == "" SET solution=src\Orchard.sln

lib\nuget\nuget.exe restore %solution%

%msbuild% /t:%target% %project% /p:Solution=%solution% /m

:end

pause