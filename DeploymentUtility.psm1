Set-StrictMode -Version Latest

# Continue a build process even though there is a warning wrote to std err. 
# We will check exit code in Invoke-ExternalCommand to design whether it fail or not
$ErrorActionPreference = "Continue"

function Add-NpmToPathVariable {
    $path = "$env:Appdata\npm"
    $escapedPath = [Regex]::Escape($path)

    # Remove existing npm path safe to add npm path again 
    $paths = $env:Path -split ';' | Where-Object { 
        $_ -notmatch "^$escapedPath\\?$"
    }

    # Update a path variable to this session 
    $env:Path = ($paths + $path) -join ";" # array + element item
}

function Invoke-ExternalCommand {
    param (
        [Parameter(Mandatory = $true)] [scriptblock] $ScriptBlock
    )

    # Displays an error message and continue executing if there is a standard error.
    # This is because there are some external command tools write warning message to standard error. 
    # Use Write-Output also fix "Window title cannot be longer than 1023 characters" issue 
    # https://github.com/projectkudu/kudu/issues/2635
    & $ScriptBlock 2>&1 | Write-Output

    # If last exit code is not 0, throw an exception to stop a script
    if ($LastExitCode) {
        throw "Failed with exit code = $LastExitCode and command = $($ScriptBlock.ToString())"
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

function Install-Yarn {
    "Verify if yarn installed" 
    if (Get-Command -Name yarn -ErrorAction Ignore) {
        "Updating yarn as a global tool to the latest version"
        # https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/cmd#parameters
        # issue https://github.com/projectkudu/kudu/issues/2635
        Invoke-ExternalCommand -ScriptBlock { npm update yarn -g }
    }
    else {
        "Installing yarn as a global tool"
        Invoke-ExternalCommand -ScriptBlock { npm install yarn -g }
        Add-NpmToPathVariable
    }
}

function Install-KuduSync {
    "Verify if kudusync installed" 
    if (Get-Command -Name kudusync -ErrorAction Ignore) {
        "Updating kudusync as a global tool to the latest version"
        # https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/cmd#parameters
        # issue https://github.com/projectkudu/kudu/issues/2635
        Invoke-ExternalCommand -ScriptBlock { npm update kudusync -g }
    }
    else {
        "Installing kudusync as a global tool"
        Invoke-ExternalCommand -ScriptBlock { npm install kudusync -g }
        Add-NpmToPathVariable
    }
}

Export-ModuleMember -Function Invoke-ExternalCommand
Export-ModuleMember -Function Write-EnviromentValue
Export-ModuleMember -Function Install-Yarn
Export-ModuleMember -Function Install-KuduSync
