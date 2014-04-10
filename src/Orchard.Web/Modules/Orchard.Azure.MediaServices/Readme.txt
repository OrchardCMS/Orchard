
CONFIGURING ORCHARD FOR THE MODULE
**********************************
    
1. Increase the maximum request content length for Orchard
----------------------------------------------------------

The module provides two ways of uploading media files to Microsoft Azure Media Services (WAMS):
* Direct CORS-based upload to Microsoft Azure Blob Storage. This is used by default if the browser supports CORS.
* Proxied upload where files are first uploaded to Orchard where they are kept in local file system storage temporarily, before being ingested into WAMS by a server-side background process. This is used as a fallback for browsers that don't support CORS.

When using proxied upload, IIS places limits on the maximum allowed upload file sizes. By default, Orchard is configured in its Web.config to allow file uploads of up to 64 MB. If the module will be used to ingest media files larger than 64 MB these values need to be increased, or the first upload step (from the user's/editor's local machine to Orchard temporary storage) will fail.

Specifically, the following attributes need to be reconfigured with a value appropriate for the size of media files with which the module is intended to be used:
    - system.web/httpRuntime/@maxRequestLength
    - system.webServer/security/requestFiltering/requestLimits/@maxAllowedContentLength
    
Both of these attributes must be configured with the same value, or whichever value is smaller will effectively impose a limit on the allowed media file upload size.

2. Configuring the logging level for the module
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

After the module is installed and the feature "Microsoft Azure Media Services" is enabled, the WAMS account credentials must be configured in the "Windows Azure Media" settings section in the Orchard administration dashboard. This information can be obtained from the Windows Azure management portal.

2. Configuring the Microsoft Azure Storage account credentials
------------------------------------------------------------

To use the direct CORS-based upload a CORS rule must be added to the Microsoft Azure Storage account used by the configured WAMS instance. The module can do this automatically, but it needs the Windows Azure Storage account credentials. Specify the account key for the underlying Windows Azure Storage account used by your Windows Azure Media Services instance if you want to automatically enable CORS support with the appropriate origin URLs for the Orchard site. CORS rules will be added to the storage account for both the configured base URL of the site, and the URL on which you are accessing the site when saving the credentials. If you leave this field blank you will have to manually enable the appropriate CORS support for your Windows Azure Storage account using PowerShell.