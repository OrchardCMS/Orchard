using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Widgets {

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

            ContentDefinitionManager.AlterPartDefinition("WidgetPart", part => part
                .Attachable()
                .WithDescription("Turns a content type into a Widget. Note: you need to set the stereotype to \"Widget\" as well."));

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

            return 5;
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

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition("WidgetPart", builder => builder.Attachable());

            return 4;
        }

        public int UpdateFrom4() {
            ContentDefinitionManager.AlterPartDefinition("WidgetPart", part => part
                .WithDescription("Turns a content type into a Widget. Note: you need to set the stereotype to \"Widget\" as well."));
            return 5;
        }
    }
}