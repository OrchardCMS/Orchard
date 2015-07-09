using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using System.IO;

namespace Orchard.Reports.Services {
    public class ReportsPersister : IReportsPersister {
        private readonly IAppDataFolder _appDataFolder;
        private readonly ShellSettings _shellSettings;
        private readonly string _reportsFileName;
        private readonly DataContractSerializer _dataContractSerializer;
        private readonly object _synLock = new object();

        public ReportsPersister(IAppDataFolder appDataFolder, ShellSettings shellSettings) {
            _appDataFolder = appDataFolder;
            _shellSettings = shellSettings;
            _dataContractSerializer = new DataContractSerializer(typeof(Report), new [] { typeof(ReportEntry) });
            _reportsFileName = Path.Combine(Path.Combine("Sites", _shellSettings.Name), "reports.dat");
        }

        public IEnumerable<Report> Fetch() {
            lock ( _synLock ) {
                if ( !_appDataFolder.FileExists(_reportsFileName) ) {
                    yield break;
                }

                var text = _appDataFolder.ReadFile(_reportsFileName);
                var xmlDocument = XDocument.Parse(text);
                var rootNode = xmlDocument.Root;
                if (rootNode == null) {
                    yield break;
                }

                foreach (var reportNode in rootNode.Elements()) {
                    var reader = new StringReader(reportNode.Value);
                    using (var xmlReader = XmlReader.Create(reader)) {
                        yield return (Report) _dataContractSerializer.ReadObject(xmlReader, true);
                    }
                }
            }
        }

        public void Save(IEnumerable<Report> reports) {
            lock ( _synLock ) {
                var xmlDocument = new XDocument();
                xmlDocument.Add(new XElement("Reports"));
                foreach (var report in reports) {
                    var reportNode = new XElement("Report");
                    var writer = new StringWriter();
                    using (var xmlWriter = XmlWriter.Create(writer)) {
                        _dataContractSerializer.WriteObject(xmlWriter, report);
                    }
                    reportNode.Value = writer.ToString();
                    xmlDocument.Root.Add(reportNode);
                }

                var saveWriter = new StringWriter();
                xmlDocument.Save(saveWriter);
                _appDataFolder.CreateFile(_reportsFileName, saveWriter.ToString());
            }
        }
    }
}

