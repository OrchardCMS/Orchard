for /f "usebackq tokens=*" %%i in (`lib\vswhere\vswhere -latest -version "[15.0,16.0)" -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set InstallDir=%%i
)


FOR %%b in (
       "%InstallDir%\Common7\Tools\VsMSBuildCmd.bat"
       "%VS140COMNTOOLS%\Common7\Tools\vsvars32.bat"
    ) do (
    if exist %%b ( 
       call %%b
       goto build
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

msbuild /t:%target% %project% /p:Solution=%solution% /m

pause