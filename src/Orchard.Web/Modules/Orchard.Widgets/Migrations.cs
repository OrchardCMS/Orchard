using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
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
            IContent authenticatedLayer = _contentManager.Create<LayerPart>("Layer", t => { t.Record.Name = "Authenticated"; t.Record.LayerRule = "authenticated"; });
            _contentManager.Publish(authenticatedLayer.ContentItem);
            IContent anonymousLayer = _contentManager.Create<LayerPart>("Layer", t => { t.Record.Name = "Anonymous"; t.Record.LayerRule = "not authenticated"; });
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
                   .WithPart("CommonPart")
                );

            ContentDefinitionManager.AlterTypeDefinition("HtmlWidget",
                cfg => cfg
                    .WithPart("WidgetPart")
                    .WithPart("BodyPart")
                    .WithPart("CommonPart")
                    .WithSetting("Stereotype", "Widget")
                );

            CreateDefaultLayers();

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition(typeof(LayerPart).Name, 
                cfg => cfg
                    .WithLocation(new Dictionary<string, ContentLocation> {
                        {"Editor", new ContentLocation { Zone = "Primary", Position = "1" }}
                    })
                );

            ContentDefinitionManager.AlterPartDefinition(typeof(WidgetPart).Name, 
                cfg => cfg
                    .WithLocation(new Dictionary<string, ContentLocation> {
                        {"Editor", new ContentLocation { Zone = "Primary", Position = "1" }}
                   })
                );

            ContentDefinitionManager.AlterPartDefinition(typeof(WidgetBagPart).Name,
                cfg => cfg
                    .WithLocation(new Dictionary<string, ContentLocation> {
                        {"Editor", new ContentLocation {Zone = "Primary", Position = "5"}}
                    })
                );
            ContentDefinitionManager.AlterTypeDefinition("WidgetPage",
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("RoutePart")
                    .WithPart("WidgetBagPart")
                    .Creatable()
                );
            return 2;
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