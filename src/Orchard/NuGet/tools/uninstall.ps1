param($installPath, $toolsPath, $package, $project)

copy "$installPath/tools/CopyOrchardLibraries.ps1" "$installPath../../../CopyOrchardLibraries.ps1"

# When modifying the command don't forget to also change it in install.ps1.
$postBuildEvent = @'
powershell.exe -ExecutionPolicy ByPass -file "$(SolutionDir)\CopyOrchardLibraries.ps1" -SolutionDirectory '$(SolutionDir)' -TargetDirectory '$(TargetDir)'
'@

# Below logic taken from MVC3.HTML5Boilerplate_YUICompressor, see https://github.com/leftofnull/MVC3.HTML5Boilerplate_YUICompressor
foreach ($prop in $project.Properties) {
	if ($prop.Name -eq "PostBuildEvent") {
		if ($prop.Value -eq "") {
			write-host 'Removed post-build event'
		}
        elseif ($prop.Value.Trim() -eq $postBuildEvent) {
			$prop.Value = ""
			write-host 'Removed Orchard Libraries copying post-build event.'
		}
        else {
			$banana = $prop.Value.Split("`n");
			$dessert = ""
			foreach ($scoop in $banana) {
				if ($scoop.Trim() -ne $postBuildEvent) {
					$dessert = "$dessert$scoop`n"
				}
			}
			$prop.Value = $dessert.Trim()
			write-host 'Removed Orchard Libraries copying post-build event.'
		}
	}
}