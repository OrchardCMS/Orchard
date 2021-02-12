[Reflection.Assembly]::LoadWithPartialName("System.Xml") | Out-Null
[Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | Out-Null
[System.Xml.Linq.XNamespace]$ns1 = "urn:schemas-microsoft-com:asm.v1"

$currentPath = (Get-Item -Path ".\").FullName
$orchardWebConfigFullPath = $currentPath+"\src\Orchard.Web\Web.Config"
[XML] $orchardWebConfig = Get-Content ($orchardWebConfigFullPath)
$configFiles = Get-ChildItem -Path ($currentPath +"\src\")  -Filter web.config -Recurse -ErrorAction SilentlyContinue -Force
foreach ($configFile in $configFiles) {
    $configFullPath = $configFile.FullName
    Write-Host "Processing $configFullPath ..."

    if ($configFullPath.ToLower().EndsWith("\orchard.web\web.config")) { #skip orchard.web config files
        continue
    }
    [XML] $projectWebConfig = Get-Content ($configFullPath)
    $elements = $projectWebConfig.configuration.runtime.assemblyBinding.dependentAssembly
    foreach ($element in $elements){
        Write-Host "Checking" $element.assemblyIdentity.name
        $hasBinding = $orchardWebConfig.configuration.runtime.assemblyBinding.dependentAssembly.assemblyIdentity.Where({ $_.name -eq $element.assemblyIdentity.name -and $_.publicKeyToken -eq $element.assemblyIdentity.publicKeyToken -and $_.culture -eq $element.assemblyIdentity.culture }, 'First').Count -gt 0
        if (-not $hasBinding){
            # add the node in $webConfig
            Write-Host "Adding" $element.assemblyIdentity.name
            $newNode = $orchardWebConfig.ImportNode($element, $true);
            $orchardWebConfig.configuration.runtime.assemblyBinding.AppendChild($newNode)
            Write-Host "Added " $element.assemblyIdentity.name
        } else {
            Write-Host "Skipped" $element.assemblyIdentity.name 
        }
    }
        Write-Host "Processed $configFullPath ..."
}

$orchardWebConfig.Save($orchardWebConfigFullPath)
