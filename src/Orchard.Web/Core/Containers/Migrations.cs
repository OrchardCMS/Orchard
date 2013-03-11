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
                    .Column<int>("OrderByDirection")
                    .Column<string>("ItemContentType")
                    .Column<bool>("ItemsShown", column => column.WithDefault(true)));

            SchemaBuilder.CreateTable("ContainerWidgetPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("ContainerId")
                    .Column<int>("PageSize")
                    .Column<string>("OrderByProperty")
                    .Column<int>("OrderByDirection")
                    .Column<bool>("ApplyFilter")
                    .Column<string>("FilterByProperty")
                    .Column<string>("FilterByOperator")
                    .Column<string>("FilterByValue"));

            SchemaBuilder.CreateTable("CustomPropertiesPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("CustomOne")
                    .Column<string>("CustomTwo")
                    .Column<string>("CustomThree"));

            SchemaBuilder.CreateTable("ContainablePartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("Weight"));

            ContentDefinitionManager.AlterTypeDefinition("ContainerWidget",
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithPart("ContainerWidgetPart")
                    .WithSetting("Stereotype", "Widget"));

            ContentDefinitionManager.AlterPartDefinition("ContainerPart", builder => builder
                .Attachable()
                .WithDescription("Turns your content item into a container that is capable of containing content items that have the ContainablePart attached."));

            ContentDefinitionManager.AlterPartDefinition("ContainablePart", builder => builder
                .Attachable()
                .WithDescription("Allows your content item to be contained by a content item that has the ContainerPart attached"));

            ContentDefinitionManager.AlterPartDefinition("CustomPropertiesPart", builder => builder
                .Attachable()
                .WithDescription("Adds 3 custom properties to your content item."));
 
            return 4;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("ContainerPartRecord", table => table.AddColumn<string>("ItemContentType"));
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("ContainerPartRecord", 
                table => table.AddColumn<bool>("ItemsShown", column => column.WithDefault(true)));

            SchemaBuilder.CreateTable("ContainablePartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("Weight"));

            return 3;
        }

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition("ContainerPart", builder => builder
                .WithDescription("Turns your content item into a container that is capable of containing content items that have the ContainablePart attached."));

            ContentDefinitionManager.AlterPartDefinition("ContainablePart", builder => builder
                .WithDescription("Allows your content item to be contained by a content item that has the ContainerPart attached"));

            ContentDefinitionManager.AlterPartDefinition("CustomPropertiesPart", builder => builder
                .WithDescription("Adds 3 custom properties to your content item."));
            return 4;
        }
    }
}