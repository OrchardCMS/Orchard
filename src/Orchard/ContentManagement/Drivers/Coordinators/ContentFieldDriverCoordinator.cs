using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public override Task BuildDisplayAsync(BuildDisplayContext context) {
            return _drivers.InvokeAsync(async driver => {
                context.Logger = Logger;
                var result = await driver.BuildDisplayShapeAsync(context);
                if (result != null)
                    await result.ApplyAsync(context);
            }, Logger);
        }

        public override Task BuildEditorAsync(BuildEditorContext context) {
            return _drivers.InvokeAsync(async driver => {
                context.Logger = Logger;
                var result = await driver.BuildEditorShapeAsync(context);
                if (result != null)
                    await result.ApplyAsync(context);
            }, Logger);
        }

        public override Task UpdateEditorAsync(UpdateEditorContext context) {
            return _drivers.InvokeAsync(async driver => {
                context.Logger = Logger;
                var result = await driver.UpdateEditorShapeAsync(context);
                if (result != null)
                    await result.ApplyAsync(context);
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