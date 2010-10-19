using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Core.Navigation {
    public class Migrations : DataMigrationImpl {

        public int Create() {
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
                    .Column<bool>("OnMainMenu")
                );

            ContentDefinitionManager.AlterTypeDefinition("Blog", cfg => cfg.WithPart("MenuPart"));
            ContentDefinitionManager.AlterTypeDefinition("Page", cfg => cfg.WithPart("MenuPart"));
            ContentDefinitionManager.AlterTypeDefinition("MenuItem", cfg => cfg.WithPart("MenuPart"));
            ContentDefinitionManager.AlterPartDefinition("MenuPart", builder => builder.Attachable());
            
            return 1;
        }

    }
}