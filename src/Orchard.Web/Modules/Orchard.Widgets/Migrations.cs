using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Events;
using Orchard.Widgets.Models;

namespace Orchard.Widgets {
    public interface IDefaultLayersInitializer : IDependency {
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

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("WidgetPage",
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("RoutePart")
                    .WithPart("WidgetBagPart")
                    .Creatable()
                );
            return 2;
        }
    }
}