using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.State;
using Orchard.Events;
using Orchard.Widgets.Models;

namespace Orchard.Widgets {
    public interface IDefaultLayersInitializer : IEventHandler {
        void CreateDefaultLayers();
    }

    public class DefaultLayersInitializer : IDefaultLayersInitializer {
        private readonly IContentManager _contentManager;

        public DefaultLayersInitializer(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void CreateDefaultLayers() {
            IContent defaultLayer = _contentManager.Create<LayerPart>("Layer", t => { t.Record.Name = "Default"; t.Record.LayerRule = "true"; });
            _contentManager.Publish(defaultLayer.ContentItem);
            IContent authenticatedLayer = _contentManager.Create<LayerPart>("Layer", t => { t.Record.Name = "Authenticated"; t.Record.LayerRule = "Authenticated"; });
            _contentManager.Publish(authenticatedLayer.ContentItem);
            IContent anonymousLayer = _contentManager.Create<LayerPart>("Layer", t => { t.Record.Name = "Anonymous"; t.Record.LayerRule = "not Authenticated"; });
            _contentManager.Publish(anonymousLayer.ContentItem);
        }
    }

    public class WidgetsDataMigration : DataMigrationImpl {
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly ShellDescriptor _shellDescriptor;

        public WidgetsDataMigration(IProcessingEngine processingEngine, ShellSettings shellSettings, ShellDescriptor shellDescriptor) {
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptor = shellDescriptor;
        }

        public int Create() {
            SchemaBuilder.CreateTable("LayerPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Name")
                .Column<string>("Description")
                .Column<string>("LayerRule")
                );

            SchemaBuilder.CreateTable("WidgetPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Title")
                .Column<string>("Position")
                .Column<string>("Zone")
                );

            ContentDefinitionManager.AlterTypeDefinition("Layer",
               cfg => cfg
                   .WithPart("LayerPart")
                );

            ContentDefinitionManager.AlterTypeDefinition("HtmlWidget",
                cfg => cfg
                    .WithPart("WidgetPart")
                    .WithPart("BodyPart")
                );

            CreateDefaultLayers();

            return 1;
        }

        private void CreateDefaultLayers() {
            _processingEngine.AddTask(
                    _shellSettings,
                    _shellDescriptor,
                    "IDefaultLayersInitializer.CreateDefaultLayers",
                    new Dictionary<string, object>());
        }
    }
}