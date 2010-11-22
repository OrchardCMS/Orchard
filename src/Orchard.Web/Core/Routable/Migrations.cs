using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Core.Routable {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("RoutePartRecord", 
                table => table
                    .ContentPartVersionRecord()
                    .Column<string>("Title", column => column.WithLength(1024))
                    .Column<string>("Slug", column => column.WithLength(1024))
                    .Column<string>("Path", column => column.WithLength(2048))
                );

            ContentDefinitionManager.AlterPartDefinition("RoutePart", builder => builder.Attachable());

            return 1;
        }
    }
}