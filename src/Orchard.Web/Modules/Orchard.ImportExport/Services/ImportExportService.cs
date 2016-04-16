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
using System.IO;
using System.Web.Mvc;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;

namespace Orchard.ImportExport.Services {
    public class ImportExportService : Component, IImportExportService {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionWriter _contentDefinitionWriter;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IClock _clock;
        private readonly IEnumerable<IExportAction> _exportActions;
        private readonly IEnumerable<IImportAction> _importActions;
        private const string ExportsDirectory = "Exports";
        private readonly IDeploymentPackageBuilder _packageBuilder;
 

        public ImportExportService(
            IOrchardServices orchardServices,
            IAppDataFolder appDataFolder,
            IClock clock, 
            IEnumerable<IExportAction> exportActions, 
            IEnumerable<IImportAction> importActions,
            IDeploymentPackageBuilder packageBuilder,
            IContentDefinitionWriter contentDefinitionWriter,
            IContentDefinitionManager contentDefinitionManager) {

            _orchardServices = orchardServices;
            _appDataFolder = appDataFolder;
            _clock = clock;
            _exportActions = exportActions;
            _importActions = importActions;
            _packageBuilder = packageBuilder;
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionWriter = contentDefinitionWriter;
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

        public void Export(ExportActionContext context, IEnumerable<ContentItem> contentItems)
        {
            var contentTypeList = contentItems.Select(x => x.ContentType).Distinct();

            context.RecipeDocument.Add(
                    ExportData(contentTypeList, contentItems, context,64, false));
        }

        public string WriteExportFile(ExportActionContext exportActionContext)
        {
            var exportFile = String.Format("Export-{0}-{1}.xml", _orchardServices.WorkContext.CurrentUser.UserName, _clock.UtcNow.Ticks);
            if (!_appDataFolder.DirectoryExists(ExportsDirectory))
            {
                _appDataFolder.CreateDirectory(ExportsDirectory);
            }
            var recipeDocument = exportActionContext.RecipeDocument;
            var path = _appDataFolder.Combine(ExportsDirectory, exportFile);


            if (exportActionContext.Files.Any())
            {
                var packageStream = _packageBuilder.BuildPackage(
                    "export.nupkg",
                    exportActionContext.RecipeDocument,
                    exportActionContext.Files
                    );
                return WritePackageFile(packageStream).FileDownloadName;                
            }
            using (var writer = new XmlTextWriter(_appDataFolder.CreateFile(path), Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                recipeDocument.WriteTo(writer);
                return _appDataFolder.MapPath(path);
            }
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

        private FilePathResult WritePackageFile(Stream packageStream)
        {
            var exportFile = BuildFileName("nupkg");

            try
            {
                if (!_appDataFolder.DirectoryExists(ExportsDirectory))
                {
                    _appDataFolder.CreateDirectory(ExportsDirectory);
                }

                var path = _appDataFolder.Combine(ExportsDirectory, exportFile);
                using (var stream = _appDataFolder.CreateFile(path))
                {
                    packageStream.CopyTo(stream);
                }
                return new FilePathResult(_appDataFolder.MapPath(path), "application/zip")
                {
                    FileDownloadName = "export.nupkg"
                };
            }
            finally
            {
                packageStream.Close();
            }
        }

        private string BuildFileName(string extension)
        {
            return "Export-" + _orchardServices.WorkContext.CurrentUser.UserName
                   + "-" + DateTime.UtcNow.Ticks + "." + extension;
        }

        private XElement ExportData(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems, ExportActionContext context, int? batchSize, bool exportAsDraft)
        {
            var data = new XElement("Data");

            if (batchSize.HasValue && batchSize.Value > 0)
                data.SetAttributeValue("BatchSize", batchSize);

            var itemList = contentItems.ToList();

            foreach (var typeName in contentTypes)
            {
                var isTypeDraftable = _contentDefinitionManager.GetTypeDefinition(typeName)
                    .Settings.GetModel<ContentTypeSettings>().Draftable;
                foreach (var contentItemElement in itemList
                        .Where(i => i.ContentType == typeName)
                    .Select(contentItemElement => ExportContentItem(contentItemElement, null).Data)
                        .Where(contentItemElement => contentItemElement != null)
                    )
                {
                    if (exportAsDraft && isTypeDraftable)
                    {
                        contentItemElement.Attr("Status", "Draft");
                    }
                    data.Add(contentItemElement);
                }
            }

            return data;
        }

        private ExportContentContext ExportContentItem(ContentItem contentItem, ExportContext context = null)
        {
            // Call export handler for the item.
            var contentContext = new ExportContentContext(
                contentItem,
                new XElement(XmlConvert.EncodeLocalName(contentItem.ContentType)),
                context == null ? null : context.Files);
            return _orchardServices.ContentManager.Export(null, contentContext);
        }
    }
}