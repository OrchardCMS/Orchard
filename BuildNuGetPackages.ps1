$outputDirectory = "build/NuGet"
Set-Alias nuget src/.nuget/nuget

nuget update -self

If (Test-Path "$outputDirectory") {
	rmdir "$outputDirectory" -Force -Recurse
}
mkdir "$outputDirectory"

# Orchard.Libraries
nuget pack lib/Orchard.Libraries.nuspec -OutputDirectory "$outputDirectory"

# Orchard.Framework
nuget pack src/Orchard/Orchard.Framework.csproj -IncludeReferencedProjects -Build -Prop Configuration=Release -OutputDirectory "$outputDirectory"