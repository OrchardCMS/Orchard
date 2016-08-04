using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using Orchard.Warmup.Models;

namespace Orchard.Warmup.Services {
    public class WarmupReportManager : IWarmupReportManager {
        private readonly IAppDataFolder _appDataFolder;
        private const string WarmupReportFilename = "warmup.xml";
        private readonly string _warmupReportPath;

        public WarmupReportManager(
            ShellSettings shellSettings,
            IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;

            _warmupReportPath = _appDataFolder.Combine("Sites", _appDataFolder.Combine(shellSettings.Name, WarmupReportFilename));
        }

        public IEnumerable<ReportEntry> Read() {
            if(!_appDataFolder.FileExists(_warmupReportPath)) {
                yield break;
            }

            var warmupReportContent = _appDataFolder.ReadFile(_warmupReportPath);

            var doc = XDocument.Parse(warmupReportContent);
            foreach (var entryNode in doc.Root.Descendants("ReportEntry")) {
                yield return new ReportEntry {
                    CreatedUtc = XmlConvert.ToDateTime(entryNode.Attribute("CreatedUtc").Value, XmlDateTimeSerializationMode.Utc),
                    Filename = entryNode.Attribute("Filename").Value,
                    RelativeUrl = entryNode.Attribute("RelativeUrl").Value,
                    StatusCode = Int32.Parse(entryNode.Attribute("StatusCode").Value)
                };
            }            
        }

        public void Create(IEnumerable<ReportEntry> reportEntries) {
            var report = new XDocument(new XElement("WarmupReport"));

            foreach (var reportEntry in reportEntries) {
                report.Root.Add(
                    new XElement("ReportEntry",
                        new XAttribute("RelativeUrl", reportEntry.RelativeUrl),
                        new XAttribute("Filename", reportEntry.Filename),
                        new XAttribute("StatusCode", reportEntry.StatusCode),
                        new XAttribute("CreatedUtc", XmlConvert.ToString(reportEntry.CreatedUtc, XmlDateTimeSerializationMode.Utc))
                    )
                );
            }

            _appDataFolder.CreateFile(_warmupReportPath, report.ToString());
        }
    }
}