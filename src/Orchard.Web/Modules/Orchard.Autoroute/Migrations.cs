using Orchard.ContentManagement;
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

            ContentDefinitionManager.AlterPartDefinition("AutoroutePart", part => part
                .Attachable()
                .WithDescription("Adds advanced url configuration options to your content type to completely customize the url pattern for a content item."));

            SchemaBuilder.AlterTable("AutoroutePartRecord", table => table
                .CreateIndex("IDX_AutoroutePartRecord_DisplayAlias", "DisplayAlias")
            );

            return 3;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("AutoroutePart", part => part
                .WithDescription("Adds advanced url configuration options to your content type to completely customize the url pattern for a content item."));
            return 2;
        }

        public int UpdateFrom2() {

            SchemaBuilder.AlterTable("AutoroutePartRecord", table => table
                .CreateIndex("IDX_AutoroutePartRecord_DisplayAlias", "DisplayAlias")
            );

            return 3;
        }
    }
}