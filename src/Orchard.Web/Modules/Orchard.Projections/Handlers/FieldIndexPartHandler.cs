using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Data;
using Orchard.Projections.Models;
using Orchard.Projections.Services;

namespace Orchard.Projections.Handlers {
    public class FieldIndexPartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IFieldIndexService _fieldIndexService;
        private readonly IFieldStorageProvider _fieldStorageProvider;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IDraftFieldIndexService _draftFieldIndexService;

        public FieldIndexPartHandler(
            IContentDefinitionManager contentDefinitionManager,
            IRepository<FieldIndexPartRecord> repository,
            IFieldIndexService fieldIndexService,
            IDraftFieldIndexService draftFieldIndexService,
            IFieldStorageProvider fieldStorageProvider,
            IEnumerable<IContentFieldDriver> contentFieldDrivers) {
            Filters.Add(StorageFilter.For(repository));
            _contentDefinitionManager = contentDefinitionManager;
            _fieldIndexService = fieldIndexService;
            _fieldStorageProvider = fieldStorageProvider;
            _contentFieldDrivers = contentFieldDrivers;
            _draftFieldIndexService = draftFieldIndexService;
            OnUpdated<FieldIndexPart>(Updated);
            OnPublishing<FieldIndexPart>(Publishing);
        }

        protected override void Activating(ActivatingContentContext context) {
            base.Activating(context);

            // weld the FieldIndexPart dynamically, if a field has been assigned to one of its parts
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;
            if (contentTypeDefinition.Parts.Any(p => p.PartDefinition.Fields.Any())) {
                context.Builder.Weld<FieldIndexPart>();
            }
        }
        private void Updated(UpdateContentContext context, FieldIndexPart fieldIndexPart) {
            if (context.UpdatingItemVersionRecord.Latest) { // updates projection draft indexes only if it is the latest version
                DescribeValuesToindex(fieldIndexPart, (indexServiceContext) => {
                    _draftFieldIndexService.Set(fieldIndexPart,
                    indexServiceContext.LocalPart.PartDefinition.Name,
                    indexServiceContext.LocalField.Name,
                    indexServiceContext.StorageName, indexServiceContext.FieldValue, indexServiceContext.StorageType);
                });
            }
        }


        public void Publishing(PublishContentContext context, FieldIndexPart fieldIndexPart) {
            DescribeValuesToindex(fieldIndexPart, (indexServiceContext) => {
                _fieldIndexService.Set(fieldIndexPart,
                indexServiceContext.LocalPart.PartDefinition.Name,
                indexServiceContext.LocalField.Name,
                indexServiceContext.StorageName, indexServiceContext.FieldValue, indexServiceContext.StorageType);

            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldIndexPart"></param>
        /// <param name="indexService"></param>
        private void DescribeValuesToindex(FieldIndexPart fieldIndexPart, Action<IndexServiceContext> indexService) {
            foreach (var part in fieldIndexPart.ContentItem.Parts) {
                foreach (var field in part.PartDefinition.Fields) {

                    // get all drivers for the current field type
                    // the driver will describe what values of the field should be indexed
                    var drivers = _contentFieldDrivers.Where(x => x.GetFieldInfo().Any(fi => fi.FieldTypeName == field.FieldDefinition.Name)).ToList();

                    ContentPart localPart = part;
                    ContentPartFieldDefinition localField = field;
                    var membersContext = new DescribeMembersContext(
                        (storageName, storageType, displayName, description) => {
                            var fieldStorage = _fieldStorageProvider.BindStorage(localPart, localField);

                            // fieldStorage.Get<T>(storageName)
                            var getter = typeof(IFieldStorage).GetMethod("Get").MakeGenericMethod(storageType);
                            var fieldValue = getter.Invoke(fieldStorage, new[] { storageName });
                            indexService(new IndexServiceContext {
                                LocalPart = localPart,
                                LocalField = localField,
                                StorageName = storageName,
                                FieldValue = fieldValue,
                                StorageType = storageType });
                        });

                    foreach (var driver in drivers) {
                        driver.Describe(membersContext);
                    }
                }
            }
        }
        private class IndexServiceContext {
            public ContentPart LocalPart { get; set; }
            public ContentPartFieldDefinition LocalField { get; set; }
            public string StorageName { get; set; }
            public object FieldValue { get; set; }
            public Type StorageType { get; set; }

        }
    }
}