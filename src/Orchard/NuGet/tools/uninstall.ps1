param($installPath, $toolsPath, $package, $project)

$pathProbe = Join-Path $installPath "..\Orchard.Libraries*"
$libPath = Join-Path (Get-Item $pathProbe).FullName "lib\net45"

Function RemoveReference($assemblySubPath) {
    $project.Object.References | Where-Object { $_.Path -eq (Join-Path $libPath $assemblySubPath) } | ForEach-Object { $_.Remove() }
}

# When adding or removing base assemblies the changes should be reflected in install.ps1 too.
RemoveReference("autofac\Autofac.dll");
RemoveReference("autofac\Autofac.Configuration.dll");
RemoveReference("castle-windsor\net45\Castle.Core.dll");
RemoveReference("nhibernate\FluentNHibernate.dll");
RemoveReference("nhibernate\NHibernate.dll");
RemoveReference("log4net\log4net.dll");
RemoveReference("newtonsoft.json\Newtonsoft.Json.dll");
RemoveReference("aspnetwebapi\System.Net.Http.Formatting.dll");
RemoveReference("aspnetwebapi\System.Web.Http.WebHost.dll");
RemoveReference("aspnetmvc\System.Web.Mvc.dll");