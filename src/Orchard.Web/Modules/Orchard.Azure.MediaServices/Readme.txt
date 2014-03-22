INSTALLING THE MODULE
*********************

The module has been continuously built and tested against the latest version of Orchard in the 1.x branch of the official Orchard Codeplex repository.

The module can be installed into an Orchard site in a few different ways:

Use the full Orchard source code committed in the module's TFS repository
------------------------------------------------------------------------------

1. Clone the repository to the local developer machine.
2. Use Visual Studio to either run the site locally or publish Orchard through any of the standard publishing mechanisms.

For convenience, a recipe named "Orchard.Azure.MediaServices" has been added to Orchard.Setup in this repository that makes it quicker to get a site up and running where the module is already enabled and configured for use.

Add it to another copy of the Orchard source code for a "develop + publish" workflow
------------------------------------------------------------------------------------

1. Copy the whole module project folder into the Orchard.Web/Modules folder of the Orchard source code.
2. Add the Orchard.Azure.MediaServices and Orchard.Azure.MediaServices.Tests projects to the Orchard.sln solution.
3. Add a project reference from Orchard.Azure.Web to Orchard.Azure.MediaServices to ensure the module is included when publishing to a Windows Azure Cloud Service.
4. Make the necessary modifications to the Web.config files in Orchard.Web and Orchard.Azure.Web (see below for details).

"XCopy" deploy it into an already published Orchard site
--------------------------------------------------------

When dynamic compilation is enabled:
1. If the deployed Orchard site is configured to use dynamic compilation, upload the module's folder without the bin and obj folders to the /Modules folder.

When dynamic compilation is disabled:
1. If the deployed Orchard site is not configured to use dynamic compilation, upload the module's folder, including the bin folder, to the /Modules folder. To prevent unnecessary duplicate DLLs from being uploaded as part of the module's bin folder, skip this step and instead follow the steps described in the next section.

When dynamic compilation is disabled and you don't want to upload duplicate DLLs:
1. To prevent unnecessary duplicate DLLs from being uploaded as part of the module's bin folder, build the solution using the ClickToBuild.cmd file that can be found in the root directory of the project.
2. When ClickToBuild.cmd is done, a number of new folders will have been created. Go to "\build\Stage\Modules", and upload the Orchard.Azure.MediaServices folder to the Modules folder of the installation on the hosting server.


CONFIGURING ORCHARD FOR THE MODULE
**********************************

1. Add assembly binding redirects to Orchard
--------------------------------------------

The module includes Windows Azure Media Services SDK for .NET version 3.0.0.0 which is distributed via Nuget. This client SDK library in turn depends on specific versions of a number of other libraries, also distributed via Nuget. Orchard ships with newer versions of some of these assemblies, and in the Orchard process there can only be one version of any given assembly at a time.

Therefore, for the module to work, a few assembly binding redirects need to be added to the main Orchard Web.config file. These redirects cause the Windows Azure Media Services SDK for .NET client library to bind to the newer versions of these dependencies, which are present in the Bin folder of the web application. Without these redirects, the assembly binding will fail and the module will not work.

Specifically, the following redirects must be added to the the Web.config file in the Orchard.Web and Orchard.Azure.web projects:

    <dependentAssembly>
        <assemblyIdentity name="System.Spatial" publicKeyToken="31bf3856ad364e35" culture="neutral"/>
        <bindingRedirect oldVersion="0.0.0.0-5.6.0.0" newVersion="5.6.0.0"/>
    </dependentAssembly>
    <dependentAssembly>
        <assemblyIdentity name="Microsoft.Data.Services.Client" publicKeyToken="31bf3856ad364e35" culture="neutral"/>
        <bindingRedirect oldVersion="0.0.0.0-5.6.0.0" newVersion="5.6.0.0"/>
    </dependentAssembly>
    <dependentAssembly>
        <assemblyIdentity name="Microsoft.Data.Edm" publicKeyToken="31bf3856ad364e35" culture="neutral"/>
        <bindingRedirect oldVersion="0.0.0.0-5.6.0.0" newVersion="5.6.0.0"/>
    </dependentAssembly>
    <dependentAssembly>
        <assemblyIdentity name="Microsoft.Data.OData" publicKeyToken="31bf3856ad364e35" culture="neutral"/>
        <bindingRedirect oldVersion="0.0.0.0-5.6.0.0" newVersion="5.6.0.0"/>
    </dependentAssembly>
    <dependentAssembly>
        <assemblyIdentity name="Microsoft.WindowsAzure.Storage" publicKeyToken="31bf3856ad364e35" culture="neutral"/>
        <bindingRedirect oldVersion="0.0.0.0-3.0.0.0" newVersion="3.0.0.0"/>
    </dependentAssembly>
    
2. Increase the maximum request content length for Orchard
----------------------------------------------------------

When uploading large media files using the module, the files are first uploaded to Orchard where they are kept in local file system storage temporarily, before being ingested into WAMS by a server-side background process. By default, Orchard is configured in its Web.config to allow file uploads of up to 64 MB.

If the module will be used to ingest media files larger than 64 MB these values need to be increased, or the first upload step (from the user's/editor's local machine to Orchard temporary storage) will fail.

Specifically, the following attributes need to be reconfigured with a value appropriate for the size of media files with which the module is intended to be used:
    - system.web/httpRuntime/@maxRequestLength
    - system.webServer/security/requestFiltering/requestLimits/@maxAllowedContentLength
    
Both of these attributes must be configured with the same value, or whichever value is smaller will effectively impose a limit on the allowed media file upload size.

3. Configuring the logging level for the module
-----------------------------------------------

The module logs messages of different severity levels and verbosity through the standard Orchard logging pipeline.

To ensure the most verbose logging output from the module, while limiting the verbosity level of other logging sources, a <logger> element can be added to the file Config/Log4net.config in the Orchard.Web and/or Orchard.Azure.Web projects:

    <logger name="Orchard.Azure.MediaServices">
        <!-- Log verbosely from the Orchard.Azure.MediaServices module. -->
        <priority value="DEBUG" />
        <!-- Send DEBUG level logging data to debugger console and debug file, but not to error file. -->
        <appender-ref ref="debugger"/>
        <appender-ref ref="debug-file" />
    </logger>

CONFIGURING THE MODULE
**********************
   
1. Configuring the WAMS account credentials
-------------------------------------------

After the module is installed and the feature "Windows Azure Media Services" is enabled, the WAMS account credentials must be configured in the "Windows Azure Media" settings section in the Orchard administration dashboard.