using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Orchard.FileSystems.AppData;
using Orchard.ImportExport.Models;
using Orchard.Logging;
using Orchard.Services;

namespace Orchard.ImportExport.Services {
    public class ImportExportService : Component, IImportExportService {
        private readonly IOrchardServices _orchardServices;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IClock _clock;
        private readonly IEnumerable<IExportAction> _exportActions;
        private readonly IEnumerable<IImportAction> _importActions;
        private const string ExportsDirectory = "Exports";

        public ImportExportService(
            IOrchardServices orchardServices,
            IAppDataFolder appDataFolder,
            IClock clock, 
            IEnumerable<IExportAction> exportActions, 
            IEnumerable<IImportAction> importActions) {

            _orchardServices = orchardServices;
            _appDataFolder = appDataFolder;
            _clock = clock;
            _exportActions = exportActions;
            _importActions = importActions;
        }

        public string Import(ImportActionContext context, IEnumerable<IImportAction> actions = null) {
            foreach (var action in actions ?? _importActions) {
                action.Execute(context);
            }
            return context.ExecutionId;
        }

        public void Export(ExportActionContext context, IEnumerable<IExportAction> actions = null) {
            foreach (var action in actions ?? _exportActions) {
                action.Execute(context);
            }
        }

        public string WriteExportFile(XDocument recipeDocument) {
            var exportFile = String.Format("Export-{0}-{1}.xml", _orchardServices.WorkContext.CurrentUser.UserName, _clock.UtcNow.Ticks);
            if (!_appDataFolder.DirectoryExists(ExportsDirectory)) {
                _appDataFolder.CreateDirectory(ExportsDirectory);
            }

            var path = _appDataFolder.Combine(ExportsDirectory, exportFile);
            
            using (var writer = new XmlTextWriter(_appDataFolder.CreateFile(path), Encoding.UTF8)) {
                writer.Formatting = Formatting.Indented;
                recipeDocument.WriteTo(writer);
            }

            return _appDataFolder.MapPath(path);
        }

        public IEnumerable<IExportAction> ParseExportActions(XDocument configurationDocument) {
            var actionElements = configurationDocument.Root.Elements();

            foreach (var actionElement in actionElements) {
                var action = _exportActions.SingleOrDefault(x => x.Name == actionElement.Name.LocalName);

                if (action == null) {
                    Logger.Warning("The export action '{0}' could not be found. Did you forget to enable a feature?", actionElement.Name.LocalName);
                    continue;
                }

                action.Configure(new ExportActionConfigurationContext(actionElement));
                yield return action;
            }
        }

        public void ConfigureImportActions(ConfigureImportActionsContext context) {
            var actionConfigElements = context.ConfigurationDocument.Root;

            foreach (var action in _importActions) {
                var actionConfigElement = actionConfigElements.Element(action.Name);

                if (actionConfigElement != null) {
                    action.Configure(new ImportActionConfigurationContext(actionConfigElement));
                }
            }
        }

        public IEnumerable<IImportAction> ParseImportActions(XDocument configurationDocument) {
            var actionElements = configurationDocument.Root.Elements();

            foreach (var actionElement in actionElements) {
                var action = _importActions.SingleOrDefault(x => x.Name == actionElement.Name.LocalName);

                if (action == null) {
                    Logger.Warning("The import action '{0}' could not be found. Did you forget to enable a feature?", actionElement.Name.LocalName);
                    continue;
                }

                action.Configure(new ImportActionConfigurationContext(actionElement));
                yield return action;
            }
        }
    }
}