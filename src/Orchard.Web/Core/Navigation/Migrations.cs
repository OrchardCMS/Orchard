using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Core.Navigation {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("MenuPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterTypeDefinition("Page", cfg => cfg.WithPart("MenuPart"));
            
            SchemaBuilder.CreateTable("MenuItemPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("Url", column => column.WithLength(1024))
                );

            SchemaBuilder.CreateTable("MenuPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("MenuText")
                    .Column<string>("MenuPosition")
                    .Column<int>("MenuRecord_id")
                );

            ContentDefinitionManager.AlterTypeDefinition("MenuItem", cfg => cfg
                .WithPart("MenuPart")
                .WithPart("CommonPart")
                .DisplayedAs("Custom Link")
                .WithSetting("Description", "Represents a simple custom link with a text and an url.")
                .WithSetting("Stereotype", "MenuItem") // because we declare a new stereotype, the Shape MenuItem_Edit is needed
                );

            ContentDefinitionManager.AlterTypeDefinition("Menu", cfg => cfg
                .WithPart("CommonPart", p => p.WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false"))
                .WithPart("TitlePart")
                .WithPart("Identity")
                );

            SchemaBuilder.CreateTable("MenuWidgetPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("StartLevel")
                .Column<int>("Levels")
                .Column<bool>("Breadcrumb")
                .Column<int>("Menu_id")
                );

            ContentDefinitionManager.AlterTypeDefinition("MenuWidget", cfg => cfg
                .WithPart("CommonPart")
                .WithPart("WidgetPart")
                .WithPart("MenuWidgetPart")
                .WithSetting("Stereotype", "Widget")
                );

            return 3;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("AdminMenuPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("AdminMenuText")
                    .Column<string>("AdminMenuPosition")
                    .Column<bool>("OnAdminMenu")
                );
            ContentDefinitionManager.AlterPartDefinition("AdminMenuPart", builder => builder.Attachable());
            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition("MenuItem", cfg => cfg
                .WithPart("MenuPart")
                .WithPart("CommonPart")
                .DisplayedAs("Custom Link")
                .WithSetting("Description", "Represents a simple custom link with a text and an url.")
                .WithSetting("Stereotype", "MenuItem") // because we declare a new stereotype, the Shape MenuItem_Edit is needed
                );

            ContentDefinitionManager.AlterTypeDefinition("Menu", cfg => cfg
                .WithPart("CommonPart")
                .WithPart("TitlePart")
                .WithPart("Identity")
                );

            SchemaBuilder.CreateTable("MenuWidgetPartRecord",table => table
                .ContentPartRecord()
                .Column<int>("StartLevel")
                .Column<int>("Levels")
                .Column<bool>("Breadcrumb")
                .Column<int>("Menu_id")
                );

            ContentDefinitionManager.AlterTypeDefinition("MenuWidget", cfg => cfg
                .WithPart("CommonPart")
                .WithPart("WidgetPart")
                .WithPart("MenuWidgetPart")
                .WithSetting("Stereotype", "Widget")
                );

            SchemaBuilder
                .AlterTable("MenuPartRecord", table => table.DropColumn("OnMainMenu"))
                .AlterTable("MenuPartRecord", table => table.AddColumn<int>("MenuRecord_id"))
                ;

            return 3;
        }
    }
}