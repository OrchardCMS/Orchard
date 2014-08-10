param($installPath, $toolsPath, $package, $project)

$outputDirectory = "$installPath../../../OrchardLibraries"

If (!(Test-Path "$outputDirectory")){
    mkdir "$outputDirectory"
    copy "$installPath/Libraries/*" "$outputDirectory" -Recurse
}