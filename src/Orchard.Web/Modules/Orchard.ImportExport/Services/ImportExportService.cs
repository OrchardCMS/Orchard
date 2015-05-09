using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using NuGet;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Orchard.Environment.Descriptor;
using Orchard.FileSystems.AppData;
using Orchard.ImportExport.Models;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Services;
using ILogger = Orchard.Logging.ILogger;
using NullLogger = Orchard.Logging.NullLogger;
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
        private readonly IDeploymentPackageBuilder _packageBuilder;
        private const string ExportsDirectory = "Exports";
        private readonly string _recipeQueueFolder = "RecipeQueue" + Path.DirectorySeparatorChar;

        public ImportExportService(
            IOrchardServices orchardServices,
            IContentDefinitionManager contentDefinitionManager,
            IContentDefinitionWriter contentDefinitionWriter,
            IAppDataFolder appDataFolder,
            IRecipeParser recipeParser,
            IRecipeManager recipeManager,
            IShellDescriptorManager shellDescriptorManager,
            IClock clock,
            IEnumerable<IExportEventHandler> exportEventHandlers,
            IDeploymentPackageBuilder packageBuilder
            ) {
            _orchardServices = orchardServices;
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionWriter = contentDefinitionWriter;
            _appDataFolder = appDataFolder;
            _recipeParser = recipeParser;
            _recipeManager = recipeManager;
            _shellDescriptorManager = shellDescriptorManager;
            _clock = clock;
            _exportEventHandlers = exportEventHandlers;
            _packageBuilder = packageBuilder;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public string Import(Stream recipeOrPackage, string fileName) {
            var extension = Path.GetExtension(fileName);
            Debug.Assert(extension != null, T("Extension should not be null").Text);
            if (extension.Equals(".xml", StringComparison.InvariantCultureIgnoreCase)) {
                var recipeText = new StreamReader(recipeOrPackage).ReadToEnd();
                return ImportRecipe(recipeText);
            }
            if (extension.Equals(".nupkg", StringComparison.InvariantCultureIgnoreCase)) {
                return ImportPackage(recipeOrPackage);
            }
            throw new ArgumentException(T("Unrecognized file extension. Extension should be xml or nupkg.").Text, "fileName");
        }

        private string ImportPackage(Stream recipeOrPackage) {
            var executionId = Guid.NewGuid().ToString("n");
            // Store the package locally
            var importPath = Path.Combine(_appDataFolder.MapPath(_recipeQueueFolder), executionId);
            var packagePath = importPath + ".nupkg";
            using (var packageStream = _appDataFolder.CreateFile(packagePath)) {
                recipeOrPackage.Seek(0, SeekOrigin.Begin);
                recipeOrPackage.CopyTo(packageStream);
            }
            // Extract the recipe from the package
            var package = new ZipPackage(packagePath);
            var files = package.GetContentFiles().ToList();
            var recipeFile = files.Find(file => file.Path == "Content\\export.xml");
            var recipeText = recipeFile.GetStream().ReadToEnd();
            // Extract all files up-front
            var filesToImport = files
                .Where(file => file.Path != "Content\\export.xml")
                .Select(
                    file => new FileToImport {
                        Path = HttpUtility.UrlDecode(file.Path.Substring(8)),
                        GetStream = file.GetStream
                    }
                )
                .ToList();
            var filesPath = importPath + ".Files";
            _appDataFolder.CreateDirectory(filesPath);
            foreach (var fileToImport in filesToImport) {
                using (var fileStream = _appDataFolder.CreateFile(Path.Combine(filesPath, fileToImport.Path))) {
                    fileToImport.GetStream().CopyTo(fileStream);
                }
            }
            // We can delete the package now
            File.Delete(packagePath);
            // Finally, start the import
            return ImportRecipe(recipeText, filesPath, executionId);
        }

        public string ImportRecipe(string recipeText, string filesPath = null, string executionId = null) {
            if (executionId == null) {
                executionId = Guid.NewGuid().ToString("n");
            }
            if (filesPath == null) {
                filesPath = Path.Combine(_appDataFolder.MapPath(_recipeQueueFolder), executionId) + ".Files";
                if (!_appDataFolder.DirectoryExists(filesPath)) {
                    _appDataFolder.CreateDirectory(filesPath);
                }
            }
            var recipe = _recipeParser.ParseRecipe(recipeText);
            executionId = _recipeManager.Execute(recipe, filesPath, executionId);
            if (ShellUpdateRequired(recipe)) {
                UpdateShell();
            }
            return executionId;
        }

        public FilePathResult Export(IEnumerable<string> contentTypes, ExportOptions exportOptions) {
            //items need to be retrieved
            if (!exportOptions.ExportData) {
                return Export(contentTypes, null, exportOptions);
            }
            var contentTypeList = contentTypes.ToArray();
            var contentItems =
                _orchardServices.ContentManager.Query(
                    GetContentExportVersionOptions(exportOptions.VersionHistoryOptions),
                    contentTypeList).List();
            return Export(contentTypeList, contentItems, exportOptions);
        }

        public FilePathResult Export(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems, ExportOptions exportOptions) {
            var exportDocument = CreateExportRoot();
            var contentTypeList = contentTypes.ToList();

            var context = new ExportContext {
                Document = exportDocument,
                Files = new List<ExportedFileDescription>(),
                ContentTypes = contentTypeList,
                ExportOptions = exportOptions
            };

            _exportEventHandlers.Invoke(x => x.Exporting(context), Logger);

            var orchardElement = exportDocument.Element("Orchard");
            if (orchardElement == null) {
                throw new InvalidOperationException(T("Recipe is missing a top-level Orchard element.").Text);
            }
            if (exportOptions.ExportMetadata
                && (!exportOptions.ExportData || contentTypeList.Any())) {
                orchardElement.Add(ExportMetadata(contentTypeList));
            }

            if (exportOptions.ExportSiteSettings) {
                orchardElement.Add(ExportSiteSettings());
            }

            if (exportOptions.ExportData && contentTypeList.Any()) {
                orchardElement.Add(
                    ExportData(contentTypeList, contentItems, context, exportOptions.ImportBatchSize));
            }

            _exportEventHandlers.Invoke(x => x.Exported(context), Logger);

            if (exportOptions.ExportData && contentTypeList.Any() &&
                exportOptions.ExportFiles && context.Files.Any()) {
                var packageStream = _packageBuilder.BuildPackage(
                    "export.nupkg",
                    context.Document,
                    context.Files
                    );
                return WritePackageFile(packageStream);
            }
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
            var typesToExport = _contentDefinitionManager
                .ListTypeDefinitions()
                .Where(typeDefinition => contentTypes.Contains(typeDefinition.Name))
                .ToList();
            var partsToExport = new List<string>();

            foreach (var contentTypeDefinition in typesToExport) {
                foreach (var contentPartDefinition
                    in contentTypeDefinition
                        .Parts
                        .Where(contentPartDefinition =>
                            !partsToExport.Contains(contentPartDefinition.PartDefinition.Name))) {
                    partsToExport.Add(contentPartDefinition.PartDefinition.Name);
                    partsElement.Add(_contentDefinitionWriter.Export(contentPartDefinition.PartDefinition));
                }
                typesElement.Add(_contentDefinitionWriter.Export(contentTypeDefinition));
            }

            return new XElement("Metadata", typesElement, partsElement);
        }

        private XElement ExportSiteSettings() {
            var siteContentItem = _orchardServices.WorkContext.CurrentSite.ContentItem;
            var exportedElements = ExportContentItem(siteContentItem).Data.Elements().ToList();

            foreach (var contentPart in siteContentItem.Parts) {
                var exportedElement = exportedElements.FirstOrDefault(element => element.Name == contentPart.PartDefinition.Name);

                //Get all simple attributes if exported element is null
                //Get exclude the simple attributes that already exist if element is not null
                var simpleAttributes =
                    ExportSettingsPartAttributes(contentPart)
                        .Where(attribute => exportedElement == null || exportedElement.Attributes().All(xAttribute => xAttribute.Name != attribute.Name))
                        .ToList();

                if (!simpleAttributes.Any()) continue;

                if (exportedElement == null) {
                    exportedElement = new XElement(contentPart.PartDefinition.Name);
                    exportedElements.Add(exportedElement);
                }

                exportedElement.Add(simpleAttributes);
            }

            return new XElement("Settings", exportedElements);
        }

        private IEnumerable<XAttribute> ExportSettingsPartAttributes(ContentPart sitePart) {
            return sitePart.GetType().GetProperties()
                .Select(property => new { property, type = property.PropertyType })
                .Where(propertyAndType =>
                    (propertyAndType.type == typeof(string)
                     || propertyAndType.type == typeof(bool)
                     || propertyAndType.type == typeof(int))
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

        private XElement ExportData(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems, ExportContext context, int? batchSize) {
            var data = new XElement("Data");

            if (batchSize.HasValue && batchSize.Value > 0)
                data.SetAttributeValue("BatchSize", batchSize);

            var itemList = contentItems.ToList();

            foreach (var typeName in contentTypes) {
                var isTypeDraftable = _contentDefinitionManager.GetTypeDefinition(typeName)
                    .Settings.GetModel<ContentTypeSettings>().Draftable;
                foreach (var contentItemElement in itemList
                        .Where(i => i.ContentType == typeName)
                    .Select(contentItemElement => ExportContentItem(contentItemElement, context).Data)
                        .Where(contentItemElement => contentItemElement != null)
                    ) {
                    if (context.ExportOptions.ExportAsDraft && isTypeDraftable) {
                        contentItemElement.Attr("Status", "Draft");
                    }
                    data.Add(contentItemElement);
                }
            }

            return data;
        }

        private ExportContentContext ExportContentItem(ContentItem contentItem, ExportContext context = null) {
            // Call export handler for the item.
            var contentContext = new ExportContentContext(
                contentItem,
                new XElement(XmlConvert.EncodeLocalName(contentItem.ContentType)),
                context == null ? null : context.Files);
            return _orchardServices.ContentManager.Export(contentItem, contentContext);
        }

        private static VersionOptions GetContentExportVersionOptions(VersionHistoryOptions versionHistoryOptions) {
            return versionHistoryOptions.HasFlag(VersionHistoryOptions.Draft)
                ? VersionOptions.Draft
                : VersionOptions.Published;
        }

        private string BuildFileName(string extension) {
            return "Export-" + _orchardServices.WorkContext.CurrentUser.UserName
                   + "-" + DateTime.UtcNow.Ticks + "." + extension;
        }

        private FilePathResult WritePackageFile(Stream packageStream) {
            var exportFile = BuildFileName("nupkg");

            try {
                if (!_appDataFolder.DirectoryExists(ExportsDirectory)) {
                    _appDataFolder.CreateDirectory(ExportsDirectory);
                }

                var path = _appDataFolder.Combine(ExportsDirectory, exportFile);
                using (var stream = _appDataFolder.CreateFile(path)) {
                    packageStream.CopyTo(stream);
                }
                return new FilePathResult(_appDataFolder.MapPath(path), "application/zip") {
                    FileDownloadName = "export.nupkg"
                };
            }
            finally {
                packageStream.Close();
            }
        }

        private FilePathResult WriteExportFile(string exportDocument) {
            var exportFile = BuildFileName("xml");

            if (!_appDataFolder.DirectoryExists(ExportsDirectory)) {
                _appDataFolder.CreateDirectory(ExportsDirectory);
            }

            var path = _appDataFolder.Combine(ExportsDirectory, exportFile);
            _appDataFolder.CreateFile(path, exportDocument);

            return new FilePathResult(_appDataFolder.MapPath(path), "text/xml") {
                FileDownloadName = "export.xml"
            };
        }

        private static bool ShellUpdateRequired(Recipe recipe) {
            return recipe.RecipeSteps.Any(step => step.Name != "Data");
        }

        private void UpdateShell() {
            var descriptor = _shellDescriptorManager.GetShellDescriptor();
            _shellDescriptorManager.UpdateShellDescriptor(descriptor.SerialNumber, descriptor.Features, descriptor.Parameters);
        }
    }
}