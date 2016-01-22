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

        public FieldIndexPartHandler(
            IContentDefinitionManager contentDefinitionManager,
            IRepository<FieldIndexPartRecord> repository,
            IFieldIndexService fieldIndexService,
            IFieldStorageProvider fieldStorageProvider,
            IEnumerable<IContentFieldDriver> contentFieldDrivers) {
            Filters.Add(StorageFilter.For(repository));
            _contentDefinitionManager = contentDefinitionManager;
            _fieldIndexService = fieldIndexService;
            _fieldStorageProvider = fieldStorageProvider;
            _contentFieldDrivers = contentFieldDrivers;

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

        public void Publishing(PublishContentContext context, FieldIndexPart fieldIndexPart) {
            foreach (var part in fieldIndexPart.ContentItem.Parts) {
                foreach(var field in part.PartDefinition.Fields) {
                    
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
                            var fieldValue = getter.Invoke(fieldStorage, new[] {storageName});

                            _fieldIndexService.Set(fieldIndexPart,
                                localPart.PartDefinition.Name,
                                localField.Name,
                                storageName, fieldValue, storageType);
                        });

                    foreach (var driver in drivers) {
                        driver.Describe(membersContext);
                    }
                }
            }
        }
    }
}