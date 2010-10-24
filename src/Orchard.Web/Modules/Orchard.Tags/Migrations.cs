using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Tags {
    public class TagsDataMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("Tag", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("TagName")
                );

            SchemaBuilder.CreateTable("TagsContentItems", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("TagId")
                    .Column<int>("ContentItemId")
                );

            ContentDefinitionManager.AlterPartDefinition("TagsPart", builder => builder.Attachable());

            return 1;
        }
    }
}