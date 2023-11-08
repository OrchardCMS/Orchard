FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2022 AS build
WORKDIR /app


# Set the shell to PowerShell for convenience.
#SHELL ["powershell", "-Command"]

# Restore NuGet packages
WORKDIR /src
RUN  Get-Location
RUN Get-ChildItem
COPY . .
RUN  Get-Location
RUN Get-ChildItem
RUN nuget restore src/Orchard.sln
RUN  Get-Location
RUN Get-ChildItem

# Add MSBuild to the PATH. The PATH is already set in the sdk image.
# Therefore, no action is required here.

# Compile the project
RUN msbuild Orchard.proj /m /t:Compile /p:MvcBuildViews=true /p:TreatWarningsAsErrors=true -WarnAsError
RUN  Get-Location
RUN Get-ChildItem

# Test the project
# Note: The tests might require an actual SQL Server and other dependencies.
# It's recommended to run the tests in a CI environment rather than in the Docker build process.
# However, if you still want to run the tests:
RUN msbuild Orchard.proj /m /t:Test