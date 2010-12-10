SET CDIR = %CD%
call "%ProgramFiles%\Windows Azure SDK\v1.2\bin\setenv.cmd"
csrun /devstore 
csrun /run:"%CDIR %\build\Compile\Orchard.Azure.CloudService.csx";"%CDIR %\src\Orchard.Azure\Orchard.Azure.CloudService\ServiceConfiguration.cscfg" /launchbrowser 
pause