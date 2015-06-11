CScript //H:CScript

set wcatfiles=%programfiles%\wcat
if not "%programfiles(x86)%"=="" set wcatfiles=%programfiles(x86)%\wcat
if not exist "%wcatfiles%" set wcatfiles="%~dp0\..\..\lib\wcat"
