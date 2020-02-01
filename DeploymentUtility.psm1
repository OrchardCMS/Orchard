Set-StrictMode -Version Latest
$ErrorActionPreference = "Continue"

function Invoke-ExternalCommand {
    param (
        [Parameter(Mandatory = $true)] [scriptblock] $ScriptBlock
    )

    # Displays an error message and continue executing if there is an standard error.
    & $ScriptBlock 2>&1 
    if ($LastExitCode) {
        "Failed exitCode=$LastExitCode, command=$($ScriptBlock.ToString())"
    }
}

function Exit-ScriptIfError {
    if ($LastExitCode) {
        "Command failed with exitCode=$LastExitCode"
        Exit 1 
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
Export-ModuleMember -Function Exit-ScriptIfError
Export-ModuleMember -Function Write-EnviromentValue
