using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Localization {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("LocalizationPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<int>("CultureId")
                    .Column<int>("MasterContentItemId")
                );

            ContentDefinitionManager.AlterPartDefinition("LocalizationPart", builder => builder.Attachable());

            return 1;
        }
    }
}