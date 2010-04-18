using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Orchard.Environment.Configuration;
using Orchard.Environment.Topology.Models;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Environment.Topology {
        /// <summary>
    /// Single service instance registered at the host level. Provides storage
    /// and recall of topology descriptor information. Default implementation uses
    /// app_data, but configured replacements could use other per-host writable location.
    /// </summary>
    public interface ITopologyDescriptorCache {
        /// <summary>
        /// Recreate the named configuration information. Used at startup. 
        /// Returns null on cache-miss.
        /// </summary>
        ShellTopologyDescriptor Fetch(string name);

        /// <summary>
        /// Commit named configuration to reasonable persistent storage.
        /// This storage is scoped to the current-server and current-webapp.
        /// Loss of storage is expected.
        /// </summary>
        void Store(string name, ShellTopologyDescriptor descriptor);
    }

    public class TopologyDescriptorCache : ITopologyDescriptorCache {
        private readonly IAppDataFolder _appDataFolder;
        private const string TopologyCacheFileName = "cache.dat";

        public TopologyDescriptorCache(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

        }

        public ILogger Logger { get; set; }
        private Localizer T { get; set; }

        #region Implementation of ITopologyDescriptorCache

        public ShellTopologyDescriptor Fetch(string name) {
            VerifyCacheFile();
            var text = _appDataFolder.ReadFile(TopologyCacheFileName);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(text);
            XmlNode rootNode = xmlDocument.DocumentElement;
            foreach (XmlNode tenantNode in rootNode.ChildNodes) {
                if (String.Equals(tenantNode.Name, name, StringComparison.OrdinalIgnoreCase)) {
                    var serializer = new DataContractSerializer(typeof(ShellTopologyDescriptor));
                    var reader = new StringReader(tenantNode.InnerText);
                    using (var xmlReader = XmlReader.Create(reader)) {
                        return (ShellTopologyDescriptor) serializer.ReadObject(xmlReader, true); 
                    }
                }
            }

            return null;
        }

        public void Store(string name, ShellTopologyDescriptor descriptor) {
            VerifyCacheFile();
            var text = _appDataFolder.ReadFile(TopologyCacheFileName);
            bool tenantCacheUpdated = false;
            var saveWriter = new StringWriter();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(text);
            XmlNode rootNode = xmlDocument.DocumentElement;
            foreach (XmlNode tenantNode in rootNode.ChildNodes) {
                if (String.Equals(tenantNode.Name, name, StringComparison.OrdinalIgnoreCase)) {
                    var serializer = new DataContractSerializer(typeof (ShellTopologyDescriptor));
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
                var serializer = new DataContractSerializer(typeof(ShellTopologyDescriptor));
                var writer = new StringWriter();
                using (var xmlWriter = XmlWriter.Create(writer)) {
                    serializer.WriteObject(xmlWriter, descriptor);
                }
                newTenant.InnerText = writer.ToString();
                rootNode.AppendChild(newTenant);
            }

            xmlDocument.Save(saveWriter);
            _appDataFolder.CreateFile(TopologyCacheFileName, saveWriter.ToString());
        }

        #endregion

        private void VerifyCacheFile() {
            if (!_appDataFolder.FileExists(TopologyCacheFileName)) {
                var writer = new StringWriter();
                using (XmlWriter xmlWriter = XmlWriter.Create(writer)) {
                    if (xmlWriter != null) {
                        xmlWriter.WriteStartDocument();
                        xmlWriter.WriteStartElement("Tenants"); 
                        xmlWriter.WriteEndElement();              
                        xmlWriter.WriteEndDocument();
                    }
                }
                _appDataFolder.CreateFile(TopologyCacheFileName, writer.ToString());
            }
        }
    }
}
