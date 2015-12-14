using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.ImportExport.Services;
using Orchard.Security;
using Orchard.Core.Navigation.Services;
using Orchard.Settings;
using Orchard.Core.Title.Models;

namespace Orchard.ImportExport.Commands {
    public class ImportExportCommands : DefaultOrchardCommandHandler {
        private readonly IImportExportService _importExportService;

        public ImportExportCommands(IImportExportService importExportService) {
            _importExportService = importExportService;
        }

        [OrchardSwitch]
        public string Filename { get; set; }

        [CommandName("import file")]
        [CommandHelp("import file /Filename:<path> \r\n\t" + "Imports the content of a file.")]
        [OrchardSwitches("Filename")]
        public void ImportFile() {

            if (String.IsNullOrEmpty(Filename)) {
                Context.Output.WriteLine(T("Invalid file path"));
                return;
            }

            if (!File.Exists(Filename)) {
                Context.Output.WriteLine(T("File not found."));
                return;
            }

            _importExportService.Import(File.ReadAllText(Filename));

            Context.Output.WriteLine(T("Import running..."));
        }

    }
}