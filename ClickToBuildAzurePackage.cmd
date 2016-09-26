SET target=%1

IF "%target%"=="" SET target=Build

ClickToBuild %target% AzurePackage.proj

