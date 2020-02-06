Set-StrictMode -Version Latest
$ErrorActionPreference = "Continue"

function Invoke-ExternalCommand {
    param (
        [Parameter(Mandatory = $true)] [scriptblock] $ScriptBlock
    )

    # Displays an error message and continue executing if there is a standard error.
    # This is because there are some external command tools write warning message to standard error. 
    & $ScriptBlock 2>&1 

    # If last exit code is not 0, throw an exception to stop a script
    if ($LastExitCode) {
        throw "Failed exitCode=$LastExitCode, command=$($ScriptBlock.ToString())"
    }
}

function Write-EnviromentValue {
    param (
        [Parameter(Mandatory = $true)] [String[]] $EnvironmentName
    )

    "----------------- Begin of environment variables ---------------------------------"
    Get-Item -Path Env:* | Where-Object { 
        $EnvironmentName -contains $_.Name 
    } | Format-Table Name, Value -Wrap

    "----------------- End of environment variables ---------------------------------"
}

Export-ModuleMember -Function Invoke-ExternalCommand
Export-ModuleMember -Function Write-EnviromentValue
