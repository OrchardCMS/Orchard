using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

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

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("TagsPart", builder => builder
                .WithDescription("Allows to describe your content using non-hierarchical keywords."));

            return 2;
        }
    }

    [OrchardFeature("Orchard.Tags.TagCloud")]
    public class TagCloudMigrations : DataMigrationImpl {

        public int Create() {

            ContentDefinitionManager.AlterTypeDefinition(
                "TagCloud",
                cfg => cfg
                           .WithPart("TagCloudPart")
                           .WithPart("CommonPart")
                           .WithPart("WidgetPart")
                           .WithSetting("Stereotype", "Widget")
                );

            return 1;
        }
    }
}