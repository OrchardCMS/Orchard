@echo off
call "%~dp0\_environment"
"%wcatfiles%\wcat.wsf" -terminate -run -clients localhost -t ".\Scripts\%1.txt" -f ".\settings.txt" -s localhost -singleip -x
