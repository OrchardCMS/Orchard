using System;
using System.IO;
using Orchard.Commands;
using Orchard.ImportExport.Services;

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

            using (var reader = new FileStream(Filename, FileMode.Open, FileAccess.Read)) {
                _importExportService.Import(reader, Filename);
            }

            Context.Output.WriteLine(T("Import running..."));
        }

    }
}