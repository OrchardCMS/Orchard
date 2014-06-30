using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Descriptor;
using Orchard.FileSystems.AppData;
using Orchard.ImportExport.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Services;
using VersionOptions = Orchard.ContentManagement.VersionOptions;

namespace Orchard.ImportExport.Services {
    [UsedImplicitly]
    public class ImportExportService : IImportExportService {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionWriter _contentDefinitionWriter;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IRecipeParser _recipeParser;
        private readonly IRecipeManager _recipeManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IClock _clock;
        private readonly IEnumerable<IExportEventHandler> _exportEventHandlers;
        private const string ExportsDirectory = "Exports";

        public ImportExportService(
            IOrchardServices orchardServices,
            IContentDefinitionManager contentDefinitionManager,
            IContentDefinitionWriter contentDefinitionWriter,
            IAppDataFolder appDataFolder,
            IRecipeParser recipeParser, 
            IRecipeManager recipeManager,
            IShellDescriptorManager shellDescriptorManager,
            IClock clock,
            IEnumerable<IExportEventHandler> exportEventHandlers) {
            _orchardServices = orchardServices;
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionWriter = contentDefinitionWriter;
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
            if (ShellUpdateRequired(recipe)) {
                UpdateShell();
            }
            return executionId;
        }

        public string Export(IEnumerable<string> contentTypes, ExportOptions exportOptions) {
            //items need to be retrieved
            if (exportOptions.ExportData) {
                var contentTypeList = contentTypes.ToArray();
                IEnumerable<ContentItem> contentItems =
                    _orchardServices.ContentManager.Query(
                        GetContentExportVersionOptions(exportOptions.VersionHistoryOptions),
                        contentTypeList).List();
                return Export(contentTypeList, contentItems, exportOptions);
            }

            return Export(contentTypes, null, exportOptions);
        }

        public string Export(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems, ExportOptions exportOptions) {
            var exportDocument = CreateExportRoot();

            var context = new ExportContext {
                Document = exportDocument,
                ContentTypes = contentTypes,
                ExportOptions = exportOptions
            };

            _exportEventHandlers.Invoke(x => x.Exporting(context), Logger);

            if (exportOptions.ExportMetadata && (!exportOptions.ExportData || contentItems.Any())) {
                exportDocument.Element("Orchard").Add(ExportMetadata(contentTypes));
            }

            if (exportOptions.ExportSiteSettings) {
                exportDocument.Element("Orchard").Add(ExportSiteSettings());
            }

            if (exportOptions.ExportData && contentItems.Any()) {
                exportDocument.Element("Orchard").Add(ExportData(contentTypes, contentItems, exportOptions.ImportBatchSize));
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
                                          new XElement("Name", "Generated by Orchard.ImportExport"),
                                          new XElement("Author", _orchardServices.WorkContext.CurrentUser.UserName),
                                          new XElement("ExportUtc", XmlConvert.ToString(_clock.UtcNow, XmlDateTimeSerializationMode.Utc))
                                 )
                    )
                );
            return exportRoot;
        }

        private XElement ExportMetadata(IEnumerable<string> contentTypes) {
            var typesElement = new XElement("Types");
            var partsElement = new XElement("Parts");
            var typesToExport = _contentDefinitionManager.ListTypeDefinitions()
                .Where(typeDefinition => contentTypes.Contains(typeDefinition.Name))
                .ToList();
            var partsToExport = new List<string>();

            foreach (var contentTypeDefinition in typesToExport) {
                foreach (var contentPartDefinition in contentTypeDefinition.Parts) {
                    if (partsToExport.Contains(contentPartDefinition.PartDefinition.Name)) {
                        continue;
                    }
                    partsToExport.Add(contentPartDefinition.PartDefinition.Name);
                    partsElement.Add(_contentDefinitionWriter.Export(contentPartDefinition.PartDefinition));
                }
                typesElement.Add(_contentDefinitionWriter.Export(contentTypeDefinition));
            }

            return new XElement("Metadata", typesElement, partsElement);
        }

        private XElement ExportSiteSettings() {
            var siteContentItem = _orchardServices.WorkContext.CurrentSite.ContentItem;
            var exportedElements = ExportContentItem(siteContentItem).Elements().ToList();
            
            foreach (var contentPart in siteContentItem.Parts) {
                var exportedElement = exportedElements.FirstOrDefault(element => element.Name == contentPart.PartDefinition.Name);

                //Get all simple attributes if exported element is null
                //Get exclude the simple attributes that already exist if element is not null
                var simpleAttributes =
                    ExportSettingsPartAttributes(contentPart)
                    .Where(attribute => exportedElement == null || exportedElement.Attributes().All(xAttribute => xAttribute.Name != attribute.Name))
                    .ToList();

                if (simpleAttributes.Any()) {
                    if (exportedElement == null) {
                        exportedElement = new XElement(contentPart.PartDefinition.Name);
                        exportedElements.Add(exportedElement);
                    }

                    exportedElement.Add(simpleAttributes);
                }
            }

            return new XElement("Settings", exportedElements);
        }

        private IEnumerable<XAttribute> ExportSettingsPartAttributes(ContentPart sitePart) {
            return sitePart.GetType().GetProperties()
                .Select(property => new {property, type = property.PropertyType})
                .Where(propertyAndType => 
                    (propertyAndType.type == typeof (string)
                    || propertyAndType.type == typeof (bool)
                    || propertyAndType.type == typeof (int))
                    && (propertyAndType.property.GetSetMethod() != null))
                .Select(propertyNameAndValue => new {
                    name = propertyNameAndValue.property.Name,
                    value = propertyNameAndValue.property.GetValue(sitePart, null)
                })
                .Where(propertyNameAndValue => propertyNameAndValue.value != null)
                .Select(propertyTypeAndValue => new XAttribute(
                    propertyTypeAndValue.name,
                    propertyTypeAndValue.value));
        }

        private XElement ExportData(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems, int? batchSize) {
            var data = new XElement("Data");

            if (batchSize.HasValue && batchSize.Value > 0)
                data.SetAttributeValue("BatchSize", batchSize);

            foreach (var contentType in contentTypes) {
                var type = contentType;
                var items = contentItems.Where(i => i.ContentType == type);
                foreach (var contentItem in items) {
                    var contentItemElement = ExportContentItem(contentItem);
                    if (contentItemElement != null) {
                        data.Add(contentItemElement);
                    }
                }
            }

            return data;
        }

        private XElement ExportContentItem(ContentItem contentItem) {
            // Call export handler for the item.
            return _orchardServices.ContentManager.Export(contentItem);
        }

        private static VersionOptions GetContentExportVersionOptions(VersionHistoryOptions versionHistoryOptions) {
            if (versionHistoryOptions.HasFlag(VersionHistoryOptions.Draft)) {
                return VersionOptions.Draft;
            }
            return VersionOptions.Published;
        }

        private string WriteExportFile(string exportDocument) {
            var exportFile = string.Format("Export-{0}-{1}.xml", _orchardServices.WorkContext.CurrentUser.UserName, DateTime.UtcNow.Ticks);
            if (!_appDataFolder.DirectoryExists(ExportsDirectory)) {
                _appDataFolder.CreateDirectory(ExportsDirectory);
            }

            var path = _appDataFolder.Combine(ExportsDirectory, exportFile);
            _appDataFolder.CreateFile(path, exportDocument);

            return _appDataFolder.MapPath(path);
        }

        private bool ShellUpdateRequired(Recipe recipe) {
            return recipe.RecipeSteps.Any(step => step.Name != "Data");
        }

        private void UpdateShell() {
            var descriptor = _shellDescriptorManager.GetShellDescriptor();
            _shellDescriptorManager.UpdateShellDescriptor(descriptor.SerialNumber, descriptor.Features, descriptor.Parameters);
        }
    }
}