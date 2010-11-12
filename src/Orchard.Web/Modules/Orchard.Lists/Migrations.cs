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
                    .Creatable());

            return 1;
        }
    }
}
