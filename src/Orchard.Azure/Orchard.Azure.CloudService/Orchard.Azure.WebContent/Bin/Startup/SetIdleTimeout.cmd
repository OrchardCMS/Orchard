REM Unless running in the compute emulator, set the default idle timeout of IIS application pools to zero (no automatic recycling).
IF "%ComputeEmulatorRunning%" == "false" (
	%windir%\system32\inetsrv\appcmd set config -section:applicationPools -applicationPoolDefaults.processModel.idleTimeout:00:00:00
)