cls
function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value;
    if($Invocation.PSScriptRoot)
    {
        return $Invocation.PSScriptRoot;
    }
    Elseif($Invocation.MyCommand.Path)
    {
        return Split-Path $Invocation.MyCommand.Path
    }
    else
    {
        return $Invocation.InvocationName.Substring(0,$Invocation.InvocationName.LastIndexOf("\"));
    }
}

function Insert-Content 
{
    param ([String]$path)
    process 
    {
        $( ,$_; Get-Content $path -ea SilentlyContinue) | Out-File $path
    }
 }


$themeDirectory = [System.IO.Directory]::GetParent((Get-ScriptDirectory)).FullName
$files = Get-ChildItem -Path $themeDirectory -Recurse -Filter "*.cshtml"

foreach ($file in $files)
{      
    Write-Output "Processing $($file.FullName)"
    $fileContent = $null
    $fileContent = Get-Content $file.FullName -TotalCount 1
    $contentToAdd = "<!-- $($file.Name) -->"
    if ($fileContent -ne $contentToAdd)
    {
        Write-Output "`t*`t Modifying File Name: $($file.FullName)"
        $contentToAdd | Insert-Content $file.FullName       
    }       
}
