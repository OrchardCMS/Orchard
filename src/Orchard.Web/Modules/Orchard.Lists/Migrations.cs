using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Lists {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("List", 
                cfg=>cfg
                    .WithPart("CommonPart")
                    .WithPart("RoutePart")
                    .WithPart("ContainerPart")
                    .WithPart("MenuPart")
                    .WithPart("AdminMenuPart")
                    .Creatable());

            return 2;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("List", cfg => cfg.WithPart("AdminMenuPart"));
            return 2;
        }
    }
}
