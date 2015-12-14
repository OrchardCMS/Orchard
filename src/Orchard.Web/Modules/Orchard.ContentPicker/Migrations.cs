using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.ContentPicker {
    public class Migrations : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("ContentMenuItemPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("ContentMenuItemRecord_id")
                );

            ContentDefinitionManager.AlterTypeDefinition("ContentMenuItem", cfg => cfg
                .WithPart("MenuPart")
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("ContentMenuItemPart")
                .DisplayedAs("Content Menu Item")
                .WithSetting("Description", "Adds a Content Item to the menu.")
                .WithSetting("Stereotype", "MenuItem")
                );

            ContentDefinitionManager.AlterPartDefinition("NavigationPart", builder => builder
                .Attachable()
                .WithDescription("Allows the management of Content Menu Items associated with a Content Item."));
            
            return 1;
        }
    }
}