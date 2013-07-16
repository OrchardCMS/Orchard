using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Win32;
using Orchard.Caching;

namespace Orchard.FileSystems.Media {
    /// <summary>
    /// Returns the mime-type by looking into IIS configuration and the Registry
    /// </summary>
    public class ConfigurationMimeTypeProvider : IMimeTypeProvider {
        private readonly ICacheManager _cacheManager;

        public ConfigurationMimeTypeProvider(ICacheManager cacheManager) {
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// Returns the mime-type of the specified file path
        /// </summary>
        public string GetMimeType(string path) {
            string extension = Path.GetExtension(path);
            if (String.IsNullOrWhiteSpace(extension)) {
                return "application/unknown";
            }

            return _cacheManager.Get(extension, ctx => {
                try {
                    try {

                        string applicationHost = System.Environment.ExpandEnvironmentVariables(@"%windir%\system32\inetsrv\config\applicationHost.config");
                        string webConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~").FilePath;

                        // search for custom mime types in web.config and applicationhost.config
                        foreach (var configFile in new[] { webConfig, applicationHost }) {
                            if (File.Exists(configFile)) {
                                var xdoc = XDocument.Load(configFile);
                                var mimeMap = xdoc.XPathSelectElements("//staticContent/mimeMap[@fileExtension='" + extension + "']").FirstOrDefault();
                                if (mimeMap != null) {
                                    var mimeType = mimeMap.Attribute("mimeType");
                                    if (mimeType != null) {
                                        return mimeType.Value;
                                    }
                                }
                            }
                        }
                    }
                    catch {
                        // ignore issues with web.config to fall back to registry
                    }

                    // search into the registry
                    RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(extension.ToLower());
                    if (regKey != null) {
                        var contentType = regKey.GetValue("Content Type");
                        if (contentType != null) {
                            return contentType.ToString();
                        }
                    }
                }
                catch {
                    // if an exception occured return application/unknown
                    return "application/unknown";
                }

                return "application/unknown";
            });
        }       
    }
}
