
if "%WindowsSdkDir%" neq "" goto build
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" goto initialize2k8on64
if exist "%ProgramFiles%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" goto initialize2k8
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" goto initialize2k8on64Dev11
if exist "%ProgramFiles%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" goto initialize2k8Dev11
echo "Unable to detect suitable environment. Build may not succeed."
goto build


:initialize2k8
call "%ProgramFiles%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
goto build

:initialize2k8on64
call "%ProgramFiles(x86)%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
goto build

:initialize2k8Dev11
call "%ProgramFiles%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
goto build

:initialize2k8on64Dev11
call "%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
goto build

:build
call build
pause
goto end


:end
