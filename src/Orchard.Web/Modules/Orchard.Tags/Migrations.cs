using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Tags {
    public class TagsDataMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("TagsPartRecord",
                table => table
                    .ContentPartRecord()
                );

            SchemaBuilder.CreateTable("TagRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("TagName")
                );

            SchemaBuilder.CreateTable("ContentTagRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("TagRecord_Id")
                    .Column<int>("TagsPartRecord_Id")
                );

            ContentDefinitionManager.AlterPartDefinition("TagsPart", builder => builder.Attachable());

            return 1;
        }
    }
}