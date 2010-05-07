if "%~1"=="" build Build
msbuild /t:%~1 Orchard.proj
msbuild /t:%~1 AzurePackage.proj
