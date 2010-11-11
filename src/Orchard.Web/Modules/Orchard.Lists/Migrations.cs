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

            ContentDefinitionManager.AlterTypeDefinition("ListWidget",
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithPart("ContainerPart")
                    .WithSetting("Stereotype", "Widget"));

            return 1;
        }
    }
}
