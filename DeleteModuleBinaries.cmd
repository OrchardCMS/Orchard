@echo off
REM Delete all compiled module binaries to force dynamic compilation (.csproj) to kick in
set SRC=src\Orchard.Web\Modules
for /f %%i in ('dir %SRC% /b /ad') do del /q %SRC%\%%i\bin\%%i.dll
