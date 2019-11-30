<#
.SYNOPSIS
    For settting the .NET framework version to all projects in the solution
.PARAMETER TargetFrameworkVersion
    .NET framework version that you want to target
.EXAMPLE
    .\Set-TargetFramework.ps1 -TargetFrameworkVersion v4.6.1
#>

param(
    [Parameter(Mandatory = $true)] 
    [ValidatePattern("^v\d\.\d(\.\d)?$")]
    [string] $TargetFrameworkVersion
)

[Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | Out-Null

$projectFiles = Get-ChildItem -Path "../" -Filter "*.csproj" -Recurse
$projectFiles | ForEach-Object {
    $projectFilePath = $_.FullName

    # Preserving Formatting with LINQ to XML 
    # https://blogs.msdn.microsoft.com/charlie/2008/09/30/linq-farm-preserving-formatting-with-linq-to-xml/
    $loadOptions = [System.Xml.Linq.LoadOptions]::PreserveWhitespace 
    $xDoc = [System.Xml.Linq.XDocument]::Load($projectFilePath, $loadOptions)
    $ns = [System.Xml.Linq.XNamespace]::Get("http://schemas.microsoft.com/developer/msbuild/2003")

    $targetFramework = $xDoc.Descendants($ns + "TargetFrameworkVersion")
    $targetFramework.SetValue($TargetFrameworkVersion)
    $xDoc.Save($projectFilePath, [System.Xml.Linq.SaveOptions]::DisableFormatting)
}

