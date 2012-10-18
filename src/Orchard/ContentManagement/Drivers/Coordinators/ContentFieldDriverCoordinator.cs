using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.Logging;

namespace Orchard.ContentManagement.Drivers.Coordinators {
    [UsedImplicitly]
    public class ContentFieldDriverCoordinator : ContentHandlerBase {
        private readonly IEnumerable<IContentFieldDriver> _drivers;
        private readonly IFieldStorageProviderSelector _fieldStorageProviderSelector;
        private readonly IEnumerable<IFieldStorageEvents> _fieldStorageEvents;

        public ContentFieldDriverCoordinator(
            IEnumerable<IContentFieldDriver> drivers,
            IFieldStorageProviderSelector fieldStorageProviderSelector,
            IEnumerable<IFieldStorageEvents> fieldStorageEvents) {
            _drivers = drivers;
            _fieldStorageProviderSelector = fieldStorageProviderSelector;
            _fieldStorageEvents = fieldStorageEvents;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override void Initializing(InitializingContentContext context) {
            var fieldInfos = _drivers.SelectMany(x => x.GetFieldInfo()).ToArray();
            var parts = context.ContentItem.Parts;
            foreach (var contentPart in parts) {
                foreach (var partFieldDefinition in contentPart.PartDefinition.Fields) {
                    var fieldTypeName = partFieldDefinition.FieldDefinition.Name;
                    var fieldInfo = fieldInfos.FirstOrDefault(x => x.FieldTypeName == fieldTypeName);
                    if (fieldInfo != null) {
                        var storage = _fieldStorageProviderSelector
                            .GetProvider(partFieldDefinition)
                            .BindStorage(contentPart, partFieldDefinition);

                        storage = new FieldStorageEventStorage(storage, partFieldDefinition, contentPart, _fieldStorageEvents);

                        var field = fieldInfo.Factory(partFieldDefinition, storage);
                        contentPart.Weld(field);
                    }
                }
            }
        }

        public override void GetContentItemMetadata(GetContentItemMetadataContext context) {
            context.Logger = Logger;
            _drivers.Invoke(driver => driver.GetContentItemMetadata(context), Logger);
        }

        public override void BuildDisplay(BuildDisplayContext context) {
            _drivers.Invoke(driver => {
                context.Logger = Logger;
                var result = driver.BuildDisplayShape(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public override void BuildEditor(BuildEditorContext context) {
            _drivers.Invoke(driver => {
                context.Logger = Logger;
                var result = driver.BuildEditorShape(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public override void UpdateEditor(UpdateEditorContext context) {
            _drivers.Invoke(driver => {
                context.Logger = Logger;
                var result = driver.UpdateEditorShape(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public override void Importing(ImportContentContext context) {
            context.Logger = Logger;
            foreach (var contentFieldDriver in _drivers) {
                contentFieldDriver.Importing(context);
            }
        }

        public override void Imported(ImportContentContext context) {
            context.Logger = Logger;
            foreach (var contentFieldDriver in _drivers) {
                contentFieldDriver.Imported(context);
            }
        }

        public override void Exporting(ExportContentContext context) {
            context.Logger = Logger;
            foreach (var contentFieldDriver in _drivers) {
                contentFieldDriver.Exporting(context);
            }
        }

        public override void Exported(ExportContentContext context) {
            context.Logger = Logger;
            foreach (var contentFieldDriver in _drivers) {
                contentFieldDriver.Exported(context);
            }
        }
    }
}