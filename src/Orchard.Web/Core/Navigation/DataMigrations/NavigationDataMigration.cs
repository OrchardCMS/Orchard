using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Core.Navigation.DataMigrations {
    public class NavigationDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Navigation_MenuItemRecord (Id INTEGER not null, Url TEXT, primary key (Id));
            SchemaBuilder.CreateTable("MenuItemRecord", table => table
                .ContentPartRecord()
                .Column<string>("Url")
                );

            //CREATE TABLE Navigation_MenuPartRecord (Id INTEGER not null, MenuText TEXT, MenuPosition TEXT, OnMainMenu INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("MenuPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("MenuText")
                .Column<string>("MenuPosition")
                .Column<bool>("OnMainMenu")
                );

            return 1;
        }

        public int UpdateFrom1() {

            ContentDefinitionManager.AlterTypeDefinition("Blog", cfg => cfg.WithPart("MenuPart"));
            ContentDefinitionManager.AlterTypeDefinition("Page", cfg => cfg.WithPart("MenuPart"));
            ContentDefinitionManager.AlterTypeDefinition("MenuItem", cfg => cfg.WithPart("MenuPart"));

            return 2;
        }
    }
}