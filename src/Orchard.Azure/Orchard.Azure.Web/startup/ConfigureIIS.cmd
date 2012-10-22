REM   Check if this task is running on the compute emulator, if not then sets the Idle Timeout to zero

IF "%ComputeEmulatorRunning%" == "false" (
	%windir%\system32\inetsrv\appcmd set config -section:applicationPools -applicationPoolDefaults.processModel.idleTimeout:00:00:00
)