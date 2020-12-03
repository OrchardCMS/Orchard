# https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/set-strictmode?view=powershell-7
Set-StrictMode -Version Latest

# https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_preference_variables?view=powershell-7#erroractionpreference
$ErrorActionPreference = "Continue" # Just explicit set it

Import-Module -Name ./DeploymentUtility -Force

"Verify if Node.js installed"
if (-not (Get-Command -Name node -ErrorAction Ignore)) {
    throw  (
		"Missing Node.js executable, please install Node.js." +
		"If already installed, make sure it can be reached from the current environment."
	)
}

$ARTIFACTS = "$PSScriptRoot/../artifacts"
# Set deployment source folder
if (-not $Env:DEPLOYMENT_SOURCE) {
    'Set $DEPLOYMENT_SOURCE variable from the current directory'
    $Env:DEPLOYMENT_SOURCE = $PSScriptRoot
}

if (-not $Env:DEPLOYMENT_TARGET) {
    'Set $DEPLOYMENT_TARGET variable'
    $Env:DEPLOYMENT_TARGET = "$ARTIFACTS/wwwroot"
}

if (-not $Env:NEXT_MANIFEST_PATH) {
    'Set $NEXT_MANIFEST_PATH variable'
    $Env:NEXT_MANIFEST_PATH = "$ARTIFACTS/manifest"

	if (-not $Env:PREVIOUS_MANIFEST_PATH) {
		'Set $PREVIOUS_MANIFEST_PATH variable'
		$Env:PREVIOUS_MANIFEST_PATH = "$ARTIFACTS/manifest"
	}
}

# Log environment variables
$environmentNameToWriteValue = @(
    "DEPLOYMENT_SOURCE"
    "DEPLOYMENT_TARGET"
    "NEXT_MANIFEST_PATH"
    "PREVIOUS_MANIFEST_PATH"
    "WEBSITE_NODE_DEFAULT_VERSION"
    "WEBSITE_NPM_DEFAULT_VERSION"
    "SCM_REPOSITORY_PATH"
    "SOLUTION_PATH"
    "PROJECT_PATH"
    "MSBUILD_PATH"
    "Path" 
)
Write-EnviromentValue -EnvironmentName $environmentNameToWriteValue

################ Build Node.js project with yarn if there is yarn.lock file ################
$nodeProjectsDir = Get-ChildItem -Path . -Recurse -Filter "yarn.lock" |
	Select-Object -ExpandProperty DirectoryName -Unique |
    Where-Object { $_ -NotMatch "node_modules" }

"Node projects directory:"
$nodeProjectsDir

Install-Yarn

$nodeProjectsDir | Foreach-Object {
	$projectDir = $_
	Push-Location -Path $projectDir

	"Current Node project directory is $(Get-Location)"
	"Installing npm packages with yarn"
	Invoke-ExternalCommand -ScriptBlock { yarn install }

	"Building Node.js project with yarn" 
	Invoke-ExternalCommand -ScriptBlock { yarn build }
	Pop-Location
}
###########################################################################################

# Build .NET project
"Restore NuGet packages"
# REF https://docs.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-restore#options
$msBuildDir = Split-Path -Path $Env:MSBUILD_PATH -Parent
Invoke-ExternalCommand -ScriptBlock { ./lib/nuget/nuget.exe restore "$Env:SOLUTION_PATH" -MSBuildPath "$msBuildDir" }

"Build .NET project to the pre-compiled directory"
$preCompiledDir = "$Env:DEPLOYMENT_SOURCE/build/Precompiled"

"Build .NET project to the temp directory"
"Building the project with MSBuild to '$preCompiledDir'" 
Invoke-ExternalCommand -ScriptBlock { 
	cmd /c "$Env:MSBUILD_PATH" `
		"$Env:PROJECT_PATH" `
		/t:Precompiled `
		/p:PreCompiledDir=$preCompiledDir `
		/verbosity:minimal `
		/maxcpucount `
		/nologo `
		$Env:SCM_BUILD_ARGS
		# Set SCM_BUILD_ARGS as App Service Configuration to any string you want to append to the MSBuild command line.
}

Install-KuduSync

"Syncing a build output to a deployment folder" 
Invoke-ExternalCommand -ScriptBlock {
	cmd /c kudusync `
		-f "$preCompiledDir" `
		-t "$Env:DEPLOYMENT_TARGET" `
		-n "$Env:NEXT_MANIFEST_PATH" `
		-p "$Env:PREVIOUS_MANIFEST_PATH" `
		-i ".git;.hg;.deployment;deploy.cmd;deploy.ps1;node_modules;"
}

if ($Env:POST_DEPLOYMENT_ACTION) {
    "Post deployment stub"
    Invoke-ExternalCommand -ScriptBlock { $Env:POST_DEPLOYMENT_ACTION }
}

"Deployment successfully"
