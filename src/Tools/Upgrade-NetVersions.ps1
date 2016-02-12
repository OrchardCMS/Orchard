cls
$projects = Get-ChildItem -Recurse -Filter "*.csproj"

foreach ($project in $projects)
{
    $fileName = $project.FullName
    $fileName
    $projectFileAsXml = [Xml](Get-Content $fileName)
    $nsmgr = New-Object System.Xml.XmlNamespaceManager -ArgumentList $projectFileAsXml.NameTable
    $nsmgr.AddNamespace("a","http://schemas.microsoft.com/developer/msbuild/2003")    
    $updateNodes = $projectFileAsXml.SelectNodes("//a:Project/a:PropertyGroup//a:TargetFrameworkVersion/text()", $nsmgr)
    $updateNodes[0].Value = "v4.6.1"
    $updateNodes[0].Value
    $projectFileAsXml.Save($fileName)
}