#https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/set-strictmode?view=powershell-7
Set-StrictMode -Version Latest

# https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_preference_variables?view=powershell-7#erroractionpreference
$ErrorActionPreference = "Stop"

Import-Module -Name .\DeploymentUtility

"Verify if Node.js installed"
if (-not (Get-Command -Name node -ErrorAction Ignore)) {
    throw "Missing node.js executable, please install node.js." +
    "If already installed, make sure it can be reached from the current Environment."
}

# Setup
$ARTIFACTS = "$PSScriptRoot\..\artifacts"

if (-not $Env:DEPLOYMENT_SOURCE) {
    $Env:DEPLOYMENT_SOURCE = $PSScriptRoot
}

if (-not $Env:DEPLOYMENT_TARGET) {
    $Env:DEPLOYMENT_TARGET = "$ARTIFACTS\wwwroot"
}

if (-not $Env:NEXT_MANIFEST_PATH) {
    $Env:NEXT_MANIFEST_PATH = "$ARTIFACTS\manifest"

    if (-not $Env:PREVIOUS_MANIFEST_PATH) {
        $Env:PREVIOUS_MANIFEST_PATH = "$ARTIFACTS\manifest"
    }
}

if (-not $Env:KUDU_SYNC_CMD) {
    "Installing Kudu Sync"
    Invoke-ExternalCommand -ScriptBlock { & npm install kudusync -g --silent }

    # Locally just running "kuduSync" would also work
    $Env:KUDU_SYNC_CMD = "$Env:AppData\npm\kuduSync.cmd"
}

# Log Environment variables
$EnvironmentNameToWriteValue = @(
    "DEPLOYMENT_SOURCE"
    "DEPLOYMENT_TARGET"
    "NEXT_MANIFEST_PATH"
    "PREVIOUS_MANIFEST_PATH"
    "KUDU_SYNC_CMD"
    "WEBSITE_NODE_DEFAULT_VERSION"
    "SCM_REPOSITORY_PATH"
    "Path" 
    "SOLUTION_PATH"
    "PROJECT_PATH"
    "MSBUILD_PATH"
)
Write-EnviromentValue -EnvironmentName $EnvironmentNameToWriteValue

"Current node version: $(& node --version)"
"Current npm version: $(& npm --version)"
"Current MSBUILD version: $(& $Env:MSBUILD_PATH -version)"

###########################################################
# Deployment
###########################################################

"Handling .NET Web Application deployment."
"Restore NuGet packages"
"Current nuget version: $(nuget help | Select -First 1)"

Invoke-ExternalCommand -ScriptBlock { 
	& nuget restore "$Env:SOLUTION_PATH" -MSBuildPath "$(Split-Path -Path $Env:MSBUILD_PATH)"  
}

"Build .NET project to the temp directory"
$preCompiledDir = "$Env:DEPLOYMENT_SOURCE\build\Precompiled"

"Building with MSBUILD to '$preCompiledDir'" 
Invoke-ExternalCommand -ScriptBlock { 
    # https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference?view=vs-2019
    & "$Env:MSBUILD_PATH" `
        "$Env:PROJECT_PATH" `
        /t:Precompiled `
        /verbosity:minimal `
        /maxcpucount `
        /nologo `
        $Env:SCM_BUILD_ARGS
    # Set SCM_BUILD_ARGS App Services Apps Settings to string you want to append to the msbuild command line.
}

"Kudu syncing" 
Invoke-ExternalCommand -ScriptBlock { 
    & "$Env:KUDU_SYNC_CMD" `
        -v 50 `
        -f "$preCompiledDir" `
        -t "$Env:DEPLOYMENT_TARGET" `
        -n "$Env:NEXT_MANIFEST_PATH" `
        -p "$Env:PREVIOUS_MANIFEST_PATH" `
        -i ".git;.hg;.deployment;deploy.cmd;deploy.ps1;node_modules;"
}

if ($Env:POST_DEPLOYMENT_ACTION) {
    "Post deployment stub"
    Invoke-ExternalCommand -ScriptBlock { & $Env:POST_DEPLOYMENT_ACTION }
}

"Deployment successfully"

