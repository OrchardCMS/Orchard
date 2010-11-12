using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Orchard.Caching;
using Orchard.Environment.Descriptor.Models;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Environment.Descriptor {
    /// <summary>
    /// Single service instance registered at the host level. Provides storage
    /// and recall of shell descriptor information. Default implementation uses
    /// app_data, but configured replacements could use other per-host writable location.
    /// </summary>
    public interface IShellDescriptorCache {
        /// <summary>
        /// Recreate the named configuration information. Used at startup. 
        /// Returns null on cache-miss.
        /// </summary>
        ShellDescriptor Fetch(string shellName);

        /// <summary>
        /// Commit named configuration to reasonable persistent storage.
        /// This storage is scoped to the current-server and current-webapp.
        /// Loss of storage is expected.
        /// </summary>
        void Store(string shellName, ShellDescriptor descriptor);
    }

    public class ShellDescriptorCache : IShellDescriptorCache {
        private readonly IAppDataFolder _appDataFolder;
        private const string DescriptorCacheFileName = "cache.dat";

        public ShellDescriptorCache(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #region Implementation of IShellDescriptorCache

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "StringReader closed by XmlReader.")]
        public ShellDescriptor Fetch(string name) {
            VerifyCacheFile();
            var text = _appDataFolder.ReadFile(DescriptorCacheFileName);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(text);
            XmlNode rootNode = xmlDocument.DocumentElement;
            foreach (XmlNode tenantNode in rootNode.ChildNodes) {
                if (String.Equals(tenantNode.Name, name, StringComparison.OrdinalIgnoreCase)) {
                    var serializer = new DataContractSerializer(typeof(ShellDescriptor));
                    var reader = new StringReader(tenantNode.InnerText);
                    using (var xmlReader = XmlReader.Create(reader)) {
                        return (ShellDescriptor)serializer.ReadObject(xmlReader, true);
                    }
                }
            }

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "writer closed by xmlWriter.")]
        public void Store(string name, ShellDescriptor descriptor) {
            VerifyCacheFile();
            var text = _appDataFolder.ReadFile(DescriptorCacheFileName);
            bool tenantCacheUpdated = false;
            var saveWriter = new StringWriter();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(text);
            XmlNode rootNode = xmlDocument.DocumentElement;
            foreach (XmlNode tenantNode in rootNode.ChildNodes) {
                if (String.Equals(tenantNode.Name, name, StringComparison.OrdinalIgnoreCase)) {
                    var serializer = new DataContractSerializer(typeof(ShellDescriptor));
                    var writer = new StringWriter();
                    using (var xmlWriter = XmlWriter.Create(writer)) {
                        serializer.WriteObject(xmlWriter, descriptor);
                    }
                    tenantNode.InnerText = writer.ToString();
                    tenantCacheUpdated = true;
                    break;
                }
            }
            if (!tenantCacheUpdated) {
                XmlElement newTenant = xmlDocument.CreateElement(name);
                var serializer = new DataContractSerializer(typeof(ShellDescriptor));
                var writer = new StringWriter();
                using (var xmlWriter = XmlWriter.Create(writer)) {
                    serializer.WriteObject(xmlWriter, descriptor);
                }
                newTenant.InnerText = writer.ToString();
                rootNode.AppendChild(newTenant);
            }

            xmlDocument.Save(saveWriter);
            _appDataFolder.CreateFile(DescriptorCacheFileName, saveWriter.ToString());
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void VerifyCacheFile() {
            if (!_appDataFolder.FileExists(DescriptorCacheFileName)) {
                var writer = new StringWriter();
                using (XmlWriter xmlWriter = XmlWriter.Create(writer)) {
                    if (xmlWriter != null) {
                        xmlWriter.WriteStartDocument();
                        xmlWriter.WriteStartElement("Tenants");
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndDocument();
                    }
                }
                _appDataFolder.CreateFile(DescriptorCacheFileName, writer.ToString());
            }
        }
    }
}
