if "%WindowsSdkDir%" neq "" goto build
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" goto initialize2k8on64Dev11
if exist "%ProgramFiles%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" goto initialize2k8Dev11
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" goto initialize2k8on64Dev12
if exist "%ProgramFiles%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" goto initialize2k8Dev12
echo "Unable to detect suitable environment. Build may not succeed."
goto build


:initialize2k8Dev12
call "%ProgramFiles%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" x86
goto build

:initialize2k8on64Dev12
call "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" x86
goto build

:initialize2k8Dev11
call "%ProgramFiles%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
goto build

:initialize2k8on64Dev11
call "%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
goto build

:build
msbuild /t:Build AzurePackage.proj
pause
goto end


:end
