param($installPath, $toolsPath, $package, $project)

copy "$installPath/tools/CopyOrchardLibraries.ps1" "$installPath../../../CopyOrchardLibraries.ps1"

# When modifying the command don't forget to also change it in uninstall.ps1.
$postBuildEvent = @'
powershell.exe -ExecutionPolicy ByPass -file "$(SolutionDir)\CopyOrchardLibraries.ps1" -SolutionDirectory '$(SolutionDir)' -TargetDirectory '$(TargetDir)'
'@

# Below logic taken from MVC3.HTML5Boilerplate_YUICompressor, see https://github.com/leftofnull/MVC3.HTML5Boilerplate_YUICompressor
$added = $false
foreach ($prop in $project.Properties) {
	if ($prop.Name -eq "PostBuildEvent") {
		if ($prop.Value -eq "") {
			write-host 'Appending post-build event'
			$prop.Value = $postBuildEvent
		}
        else {
			write-host 'Creating post-build event'
			$prop.Value = "$prop.Value`r`n`$postBuildEvent"
		}
		$added = $true
	}
}

if ($added -eq $false) {
	write-host 'Creating post-build event'
	$prop.Value = $postBuildEvent
	$added = $true
}

write-host 'Orchard Libraries copy added to post-build event.'