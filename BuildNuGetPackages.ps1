$dotNetFolderName = "net45"
$outputDirectory = "build/NuGet"
Set-Alias nuget src/.nuget/nuget

nuget update -self

If (Test-Path $outputDirectory){
	rmdir $outputDirectory -Force -Recurse
}
mkdir $outputDirectory

# Orchard.Libraries
mkdir Orchard.Libraries/lib/$dotNetFolderName
copy lib/* Orchard.Libraries/lib/$dotNetFolderName -Recurse
move Orchard.Libraries/lib/$dotNetFolderName/Orchard.Libraries.nuspec Orchard.Libraries/Orchard.Libraries.nuspec
rm Orchard.Libraries.*.nupkg
nuget pack Orchard.Libraries/Orchard.Libraries.nuspec -OutputDirectory $outputDirectory
rmdir Orchard.Libraries -Force -Recurse

# Orchard.Framework
nuget pack src/Orchard/Orchard.Framework.csproj -IncludeReferencedProjects -Build -Prop Configuration=Release -OutputDirectory $outputDirectory