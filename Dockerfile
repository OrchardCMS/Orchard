FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2019 AS build
WORKDIR /app


# Set the shell to PowerShell for convenience.
SHELL ["powershell", "-Command"]

# Copy source files
COPY . .

# Restore NuGet packages
RUN nuget restore src/Orchard.sln

# Compile the project
RUN msbuild Orchard.proj /m /t:Compile /p:MvcBuildViews=true /p:WarningLevel=0

# Test the project
# Note: The tests might require an actual SQL Server and other dependencies.
# It's recommended to run the tests in a CI environment rather than in the Docker build process.
# However, if you still want to run the tests:
#RUN msbuild Orchard.proj /m /t:Test
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8 AS runtime
WORKDIR /inetpub/wwwroot

# Copy build in to inetpub
COPY --from=build /app/src/build/Stage ./