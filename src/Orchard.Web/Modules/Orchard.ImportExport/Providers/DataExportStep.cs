using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;

namespace Orchard.ImportExport.Providers {
    public class DataExportStep : ExportStepProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentDefinitionWriter _contentDefinitionWriter;

        public DataExportStep(
            IContentDefinitionManager contentDefinitionManager, 
            IOrchardServices orchardServices, 
            IContentDefinitionWriter contentDefinitionWriter) {

            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _contentDefinitionWriter = contentDefinitionWriter;
        }

        public override string Name {
            get { return "Data"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Content Items and Definitions"); }
        }

        public override LocalizedString Description {
            get { return T("Exports content items and content item definitions."); }
        }

        public override int Position { get { return 10; } }

        public IList<string> SchemaContentTypes { get; set; }
        public IList<string> DataContentTypes { get; set; }
        public int? ImportBatchSize { get; set; }
        public VersionHistoryOptions VersionHistoryOptions { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var contentTypeViewModels = _contentDefinitionManager.ListTypeDefinitions()
                .OrderBy(x => x.Name)
                .Select(x => new ContentTypeEntry { Name = x.Name, DisplayName = x.DisplayName })
                .ToList();

            var viewModel = new DataExportStepViewModel {
                ContentTypes = contentTypeViewModels
            };

            if (updater != null && updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                DataContentTypes = viewModel.ContentTypes.Where(x => x.ExportData).Select(x => x.Name).ToList();
                SchemaContentTypes = viewModel.ContentTypes.Where(x => x.ExportSchema).Select(x => x.Name).ToList();
                VersionHistoryOptions = viewModel.VersionHistoryOptions;
            }

            return shapeFactory.EditorTemplate(TemplateName: "ExportSteps/Data", Model: viewModel, Prefix: Prefix);
        }

        public override void Export(ExportContext context) {
            var dataContentTypes = DataContentTypes;
            var schemaContentTypes = SchemaContentTypes;
            var exportVersionOptions = GetContentExportVersionOptions(VersionHistoryOptions);
            var contentItems = dataContentTypes.Any()
                ? _orchardServices.ContentManager.Query(exportVersionOptions, dataContentTypes.ToArray()).List().ToArray()
                : Enumerable.Empty<ContentItem>();

            if(schemaContentTypes.Any())
                context.Document.Element("Orchard").Add(ExportMetadata(schemaContentTypes));

            if(contentItems.Any())
                context.Document.Element("Orchard").Add(ExportData(dataContentTypes, contentItems, ImportBatchSize));
        }

        private XElement ExportMetadata(IEnumerable<string> contentTypes) {
            var typesElement = new XElement("Types");
            var partsElement = new XElement("Parts");
            var typesToExport = _contentDefinitionManager.ListTypeDefinitions()
                .Where(typeDefinition => contentTypes.Contains(typeDefinition.Name))
                .ToList();
            var partsToExport = new Dictionary<string, ContentPartDefinition>();

            foreach (var contentTypeDefinition in typesToExport.OrderBy(x => x.Name)) {
                foreach (var contentPartDefinition in contentTypeDefinition.Parts) {
                    if (partsToExport.ContainsKey(contentPartDefinition.PartDefinition.Name)) {
                        continue;
                    }
                    partsToExport.Add(contentPartDefinition.PartDefinition.Name, contentPartDefinition.PartDefinition);
                }
                typesElement.Add(_contentDefinitionWriter.Export(contentTypeDefinition));
            }

            foreach (var part in partsToExport.Values.OrderBy(x => x.Name)) {
                partsElement.Add(_contentDefinitionWriter.Export(part));
            }

            return new XElement("Metadata", typesElement, partsElement);
        }

        private XElement ExportData(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems, int? batchSize) {
            var data = new XElement("Data");

            if (batchSize.HasValue && batchSize.Value > 0)
                data.SetAttributeValue("BatchSize", batchSize);

            var orderedContentItemsQuery =
                from contentItem in contentItems
                let identity = _orchardServices.ContentManager.GetItemMetadata(contentItem).Identity.ToString()
                orderby identity
                select contentItem;

            var orderedContentItems = orderedContentItemsQuery.ToList();

            foreach (var contentType in contentTypes.OrderBy(x => x)) {
                var type = contentType;
                var items = orderedContentItems.Where(i => i.ContentType == type);
                foreach (var contentItem in items) {
                    var contentItemElement = ExportContentItem(contentItem);
                    if (contentItemElement != null)
                        data.Add(contentItemElement);
                }
            }

            return data;
        }

        private VersionOptions GetContentExportVersionOptions(VersionHistoryOptions versionHistoryOptions) {
            switch (versionHistoryOptions) {
                case VersionHistoryOptions.Draft:
                    return VersionOptions.Draft;
                case VersionHistoryOptions.Latest:
                    return VersionOptions.Latest;
                case VersionHistoryOptions.Published:
                default:
                    return VersionOptions.Published;
            }
        }

        private XElement ExportContentItem(ContentItem contentItem) {
            return _orchardServices.ContentManager.Export(contentItem);
        }
    }
}