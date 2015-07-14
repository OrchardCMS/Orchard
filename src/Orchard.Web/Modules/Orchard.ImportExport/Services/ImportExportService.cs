using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Descriptor;
using Orchard.FileSystems.AppData;
using Orchard.ImportExport.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Services;
using Orchard.Services;

namespace Orchard.ImportExport.Services {
    public class ImportExportService : IImportExportService {
        private readonly IOrchardServices _orchardServices;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IRecipeParser _recipeParser;
        private readonly IRecipeManager _recipeManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IClock _clock;
        private readonly IEnumerable<IExportEventHandler> _exportEventHandlers;
        private const string ExportsDirectory = "Exports";

        public ImportExportService(
            IOrchardServices orchardServices,
            IAppDataFolder appDataFolder,
            IRecipeParser recipeParser, 
            IRecipeManager recipeManager,
            IShellDescriptorManager shellDescriptorManager,
            IClock clock,
            IEnumerable<IExportEventHandler> exportEventHandlers) {

            _orchardServices = orchardServices;
            _appDataFolder = appDataFolder;
            _recipeParser = recipeParser;
            _recipeManager = recipeManager;
            _shellDescriptorManager = shellDescriptorManager;
            _clock = clock;
            _exportEventHandlers = exportEventHandlers;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public string Import(string recipeText) {
            var recipe = _recipeParser.ParseRecipe(recipeText);
            var executionId = _recipeManager.Execute(recipe);
            UpdateShell();
            return executionId;
        }

        public string Export(IEnumerable<IExportStepProvider> steps, ExportOptions exportOptions) {
            var exportDocument = CreateExportRoot();

            var context = new ExportContext {
                Document = exportDocument,
                ExportOptions = exportOptions
            };

            _exportEventHandlers.Invoke(x => x.Exporting(context), Logger);

            foreach (var step in steps) {
                step.Export(context);
            }

            _exportEventHandlers.Invoke(x => x.Exported(context), Logger);

            return WriteExportFile(exportDocument.ToString());
        }

        private XDocument CreateExportRoot() {
            var exportRoot = new XDocument(
                new XDeclaration("1.0", "", "yes"),
                new XComment("Exported from Orchard"),
                new XElement("Orchard",
                    new XElement("Recipe",
                        new XElement("ExportUtc", XmlConvert.ToString(_clock.UtcNow, XmlDateTimeSerializationMode.Utc))
                    )
                )
            );
            return exportRoot;
        }
        
        private string WriteExportFile(string exportDocument) {
            var exportFile = String.Format("Export-{0}-{1}.xml", _orchardServices.WorkContext.CurrentUser.UserName, DateTime.UtcNow.Ticks);
            if (!_appDataFolder.DirectoryExists(ExportsDirectory)) {
                _appDataFolder.CreateDirectory(ExportsDirectory);
            }

            var path = _appDataFolder.Combine(ExportsDirectory, exportFile);
            _appDataFolder.CreateFile(path, exportDocument);

            return _appDataFolder.MapPath(path);
        }

        private void UpdateShell() {
            var descriptor = _shellDescriptorManager.GetShellDescriptor();
            _shellDescriptorManager.UpdateShellDescriptor(descriptor.SerialNumber, descriptor.Features, descriptor.Parameters);
        }
    }
}