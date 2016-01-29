using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Core.Navigation.Models;

namespace Orchard.Alias {
    [OrchardFeature("Orchard.Alias.BreadcrumbLink")]
    public class AliasBreadcrumbMigration : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("AliasBreadcrumbMenuItem",
                cfg => cfg
                    .WithPart("BreadcrumbMenuItemPart")
                    .WithPart("MenuPart")
                    .WithPart("CommonPart")
                    .WithIdentity()
                    .DisplayedAs("Alias Breadcrumb Link")
                    .WithSetting("Description", "A menu item that can be used to generate breadcrumb links, using Alias data, to content items without explicitly adding links to those content items in the menu.")
                    .WithSetting("Stereotype", "MenuItem")
                );

            return 1;
        }
    }
}