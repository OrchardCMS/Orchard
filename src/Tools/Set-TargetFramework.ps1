<#
.SYNOPSIS
    For settting the .NET framework version to all projects in the solution

.PARAMETER TargetVerion
    .NET framework version that you want to target

.EXAMPLE
    .\Set-TargetFramework.ps1 -TargetVersion V4.6.1
#>

param(
    [Parameter(Mandatory = $true)] [string] $TargetVersion
)

$PSDefaultParameterValues['*:Encoding'] = 'utf8'

$projectFiles = Get-ChildItem -Path "../" -Filter "*.csproj" -Recurse
# Look behind then look ahead, replace only .NET version
# -Raw content to not reformat existing file
# Set output file to utf8
$regex = "(?<=<TargetFrameworkVersion>)[\w\.]+(?=<\/TargetFrameworkVersion>)"
$projectFiles | ForEach-Object {
    $content = (Get-Content $_.FullName -Raw)   
    ($content -replace $regex, $TargetVersion).Trim() | Out-File $_.FullName
}
