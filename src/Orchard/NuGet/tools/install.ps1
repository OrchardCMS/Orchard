# Needed to add basic lib references without having to include them in the Orchard.Framework package too.

param($installPath, $toolsPath, $package, $project)

$pathProbe = Join-Path $installPath "..\Orchard.Libraries*"
$libPath = Join-Path (Get-Item $pathProbe).FullName "lib\net45"

Function AddReference($assemblySubPath) {
    $project.Object.References.Add((Join-Path $libPath $assemblySubPath));
}

# When adding or removing base assemblies the changes should be reflected in uninstall.ps1 too.
AddReference("autofac\Autofac.dll");
AddReference("autofac\Autofac.Configuration.dll");
AddReference("castle-windsor\net45\Castle.Core.dll");
AddReference("nhibernate\FluentNHibernate.dll");
AddReference("nhibernate\NHibernate.dll");
AddReference("log4net\log4net.dll");
AddReference("newtonsoft.json\Newtonsoft.Json.dll");
AddReference("aspnetwebapi\System.Net.Http.Formatting.dll");
AddReference("aspnetwebapi\System.Web.Http.WebHost.dll");
AddReference("aspnetmvc\System.Web.Mvc.dll");