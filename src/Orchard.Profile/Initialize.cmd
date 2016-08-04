@echo off
call "%~dp0\_environment"
"%wcatfiles%\wcat.wsf" -terminate -update -clients localhost

