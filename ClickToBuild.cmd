FOR %%b in (
       "%VS140COMNTOOLS%..\..\VC\vcvarsall.bat"
       "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat"
       "%ProgramFiles%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" 

       "%VS120COMNTOOLS%..\..\VC\vcvarsall.bat"
       "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat"
       "%ProgramFiles%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" 

       "%VS110COMNTOOLS%..\..\VC\vcvarsall.bat"
       "%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat"
       "%ProgramFiles%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" 
    ) do (
    if exist %%b ( 
       call %%b x86
       goto build
    )
)
  
FOR %%b in (
       "%VS140COMNTOOLS%\vsvars32.bat"
       "%VS120COMNTOOLS%\vsvars32.bat"
       "%VS110COMNTOOLS%\vsvars32.bat"
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