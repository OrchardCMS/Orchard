using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
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
            IContent disabledLayer = _contentManager.Create<LayerPart>("Layer", t => { t.Record.Name = "Disabled"; t.Record.LayerRule = "false"; });
            _contentManager.Publish(disabledLayer.ContentItem);
        }
    }

    public class WidgetsDataMigration : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("LayerPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("Name")
                    .Column<string>("Description", c => c.Unlimited())
                    .Column<string>("LayerRule", c => c.Unlimited())
                );

            SchemaBuilder.CreateTable("WidgetPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("Title")
                    .Column<string>("Position")
                    .Column<string>("Zone")
                    .Column<bool>("RenderTitle", c => c.WithDefault(true))
                    .Column<string>("Name")
                );

            ContentDefinitionManager.AlterTypeDefinition("Layer",
               cfg => cfg
                   .WithPart("LayerPart")
                   .WithPart("CommonPart", p => p.WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false"))
                );

            ContentDefinitionManager.AlterTypeDefinition("HtmlWidget",
                cfg => cfg
                    .WithPart("WidgetPart")
                    .WithPart("BodyPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 3;
        }
        
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("HtmlWidget", cfg => cfg.WithPart("IdentityPart"));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder
                .AlterTable("WidgetPartRecord", table => table.AddColumn<bool>("RenderTitle", c => c.WithDefault(true)))
                .AlterTable("WidgetPartRecord", table => table.AddColumn<string>("Name"));

            return 3;
        }

        public int UpdateFrom3()
        {
            ContentDefinitionManager.AlterPartDefinition("WidgetPart", builder => builder.Attachable());

            return 4;
        }
    }
}