using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Autoroute {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("AutoroutePartRecord",
                table => table
                    .ContentPartVersionRecord()
                            .Column<string>("CustomPattern", c => c.WithLength(2048))
                            .Column<bool>("UseCustomPattern", c=> c.WithDefault(false))
                            .Column<string>("DisplayAlias", c => c.WithLength(2048)));

            ContentDefinitionManager.AlterPartDefinition("AutoroutePart", part => part.Attachable());

            return 1;
        }
    }
}