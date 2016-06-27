using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Core.Title {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("TitlePartRecord", 
                table => table
                    .ContentPartVersionRecord()
                    .Column<string>("Title", column => column.WithLength(1024))
                );

            ContentDefinitionManager.AlterPartDefinition("TitlePart", builder => builder
                .Attachable()
                .WithDescription("Provides a Title for your content item."));

            return 2;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("TitlePart", builder => builder
                .WithDescription("Provides a Title for your content item."));
            return 2;
        }
    }
}