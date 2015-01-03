using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ObjectStoreEntry", table => table
                .Column<int>("Id", c => c.PrimaryKey().Identity())
                .Column<string>("EntryKey", c => c.WithLength(64))
                .Column<string>("Data", c => c.Unlimited())
                .Column<int>("UserId")
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("LastModifiedUtc"));

            SchemaBuilder.CreateTable("ElementBlueprint", table => table
                .Column<int>("Id", c => c.PrimaryKey().Identity())
                .Column<string>("BaseElementTypeName", c => c.WithLength(256))
                .Column<string>("ElementTypeName", c => c.WithLength(256))
                .Column<string>("ElementDisplayName", c => c.WithLength(256))
                .Column<string>("ElementCategory", c => c.WithLength(256))
                .Column<string>("BaseElementState", c => c.Unlimited()));

            SchemaBuilder.CreateTable("LayoutPartRecord", table => table
                .ContentPartVersionRecord()
                .Column<int>("TemplateId"));

            ContentDefinitionManager.AlterPartDefinition("LayoutPart", part => part
                .Attachable()
                .WithDescription("Adds a layout designer to your content type."));

            ContentDefinitionManager.AlterTypeDefinition("Layout", type => type
                .WithPart("CommonPart", p => p
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                    .WithSetting("DateEditorSettings.ShowDateEditor", "false"))
                .WithPart("TitlePart")
                .WithPart("LayoutPart", p => p
                    .WithSetting("LayoutTypePartSettings.IsTemplate", "True"))
                .DisplayedAs("Layout")
                .Listable()
                .Creatable()
                .Draftable());

            ContentDefinitionManager.AlterTypeDefinition("LayoutWidget", type => type
                .WithPart("CommonPart", p => p
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                    .WithSetting("DateEditorSettings.ShowDateEditor", "false"))
                .WithPart("WidgetPart")
                .WithPart("LayoutPart")
                .WithSetting("Stereotype", "Widget")
                .DisplayedAs("Layout Widget"));

            ContentDefinitionManager.AlterPartDefinition("BodyPart", part => part.Placable());
            ContentDefinitionManager.AlterPartDefinition("TitlePart", part => part.Placable());
            ContentDefinitionManager.AlterPartDefinition("CommonPart", part => part.Placable());
            ContentDefinitionManager.AlterPartDefinition("TagsPart", part => part.Placable());

            ContentDefinitionManager.AlterPartDefinition("ElementWrapperPart", part => part
                .Attachable()
                .WithDescription("Turns elements into content items."));

            DefineElementWidget("TextWidget", "Text Widget", "Orchard.Layouts.Elements.Text");
            DefineElementWidget("MediaWidget", "Media Widget", "Orchard.Layouts.Elements.MediaItem");
            DefineElementWidget("ContentWidget", "Content Widget", "Orchard.Layouts.Elements.ContentItem");

            ContentDefinitionManager.AlterTypeDefinition("Page", type => type
                .WithPart("LayoutPart", p => p
                    .WithSetting("LayoutTypePartSettings.IsTemplate", "False")));

            return 2;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("ObjectStoreEntry", table => table
                .Column<int>("Id", c => c.PrimaryKey().Identity())
                .Column<string>("EntryKey", c => c.WithLength(64))
                .Column<string>("Data", c => c.Unlimited())
                .Column<int>("UserId")
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("LastModifiedUtc"));

            return 2;
        }

        private void DefineElementWidget(string widgetTypeName, string widgetDisplayedAs, string elementTypeName) {
            ContentDefinitionManager.AlterTypeDefinition(widgetTypeName, type => type
                .WithPart("CommonPart", p => p
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                    .WithSetting("DateEditorSettings.ShowDateEditor", "false"))
                .WithPart("WidgetPart")
                .WithPart("ElementWrapperPart", p => p
                    .WithSetting("ElementWrapperPartSettings.ElementTypeName", elementTypeName))
                .WithSetting("Stereotype", "Widget")
                .DisplayedAs(widgetDisplayedAs));
        }
    }
}