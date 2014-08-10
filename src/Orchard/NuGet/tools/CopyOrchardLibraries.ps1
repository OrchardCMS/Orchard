# Run from the post-build event to copy Orchard libraries to the output folder. This is necessary because Orchard.Framework depends on these libraries but otherwise those wouldn't be copied there without explicit references.

param
(
    [string] $SolutionDirectory,
    [string] $TargetDirectory
)

$SolutionDirectory = $SolutionDirectory.Trim('"', '''')
$TargetDirectory = $TargetDirectory.Trim('"', '''')

$sourcePath = $SolutionDirectory + "OrchardLibraries"

Write-Host "`r`n"
Write-Host "Starting copying Orchard Libraries to the output folder"
Write-Host "Source directory:" $sourcePath
Write-Host "Target directory:" $TargetDirectory
Write-Host "`r`n"


$dlls = Get-ChildItem "$sourcePath" -Filter "*.dll" -Recurse -Force
foreach ($dll in $dlls) {
    $targetName = $TargetDirectory + $dll.Name

    if (Test-Path $targetName) {
        # If a file with the same name exists then the dll is only copied if it has a higher version.
        try {
            # Note that loading assemblies will also cause the file being loaded in the script, thus e.g. you can't delete it while the script is running.
            if ([System.Reflection.Assembly]::LoadFrom($dll.FullName).GetName().Version -gt [System.Reflection.Assembly]::LoadFrom($targetName).GetName().Version) {
                Copy-Item $dll.FullName "$targetName"
            }
        }
        catch [BadImageFormatException] { # Not a managed assembly.
            if ([System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll.FullName).ProductVersion -gt [System.Diagnostics.FileVersionInfo]::GetVersionInfo($targetName).ProductVersion) {
                Copy-Item $dll.FullName "$targetName"
            }
        }
    }
    else {
        Copy-Item $dll.FullName "$targetName"
    }
}

Write-Host "`r`n"
Write-Host "Finished copying Orchard Libraries to the output folder"