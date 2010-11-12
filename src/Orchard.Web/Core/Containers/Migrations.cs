using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Core.Containers {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ContainerPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<bool>("Paginated")
                    .Column<int>("PageSize")
                    .Column<string>("OrderByProperty")
                    .Column<int>("OrderByDirection"));

            ContentDefinitionManager.AlterTypeDefinition("ContainerWidget",
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithPart("ContainerWidgetPart")
                    .WithSetting("Stereotype", "Widget"));

            ContentDefinitionManager.AlterPartDefinition("ContainerPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("ContainablePart", builder => builder.Attachable());
 
            return 1;
        }

    }
}