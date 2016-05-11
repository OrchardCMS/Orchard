SET target=%1

IF "%target%"=="" SET target=Build

ClickToBuild %target% Orchard.proj src\Orchard.Azure\Orchard.Azure.sln