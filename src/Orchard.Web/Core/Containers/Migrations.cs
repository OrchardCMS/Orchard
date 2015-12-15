using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Core.Containers {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ContainerPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("Paginated")
                .Column<int>("PageSize")
                .Column<string>("ItemContentTypes")
                .Column<bool>("ItemsShown", c => c.NotNull())
                .Column<bool>("ShowOnAdminMenu", c => c.NotNull())
                .Column<string>("AdminMenuText", c => c.WithLength(50))
                .Column<string>("AdminMenuPosition", c => c.WithLength(50))
                .Column<string>("AdminMenuImageSet", c => c.WithLength(50))
                .Column<bool>("EnablePositioning")
                .Column<string>("AdminListViewName", c => c.WithLength(50))
                .Column<int>("ItemCount", c => c.NotNull()));

            SchemaBuilder.CreateTable("ContainerWidgetPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("ContainerId")
                .Column<int>("PageSize")
                .Column<string>("OrderByProperty", c => c.WithLength(64))
                .Column<int>("OrderByDirection")
                .Column<bool>("ApplyFilter")
                .Column<string>("FilterByProperty", c => c.WithLength(64))
                .Column<string>("FilterByOperator", c => c.WithLength(4))
                .Column<string>("FilterByValue", c => c.WithLength(128)));

            SchemaBuilder.CreateTable("ContainablePartRecord", table => table
                .ContentPartRecord()
                .Column<int>("Position"));

            ContentDefinitionManager.AlterTypeDefinition("ContainerWidget", type => type
                .WithPart("CommonPart")
                .WithPart("WidgetPart")
                .WithPart("ContainerWidgetPart")
                .WithSetting("Stereotype", "Widget"));

            ContentDefinitionManager.AlterPartDefinition("ContainerPart", part => part
                .Attachable()
                .WithDescription("Turns your content item into a container that is capable of containing content items that have the ContainablePart attached."));

            ContentDefinitionManager.AlterPartDefinition("ContainablePart", part => part
                .Attachable()
                .WithDescription("Allows your content item to be contained by a content item that has the ContainerPart attached."));

            return 6;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("ContainerPartRecord", table => table.AddColumn<string>("ItemContentType"));
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("ContainerPartRecord",  table => table
                .AddColumn<bool>("ItemsShown", column => column.WithDefault(true)));

            SchemaBuilder.CreateTable("ContainablePartRecord", table => table
                .ContentPartRecord()
                .Column<int>("Weight"));

            return 3;
        }

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition("ContainerPart", part => part
                .WithDescription("Turns your content item into a container that is capable of containing content items that have the ContainablePart attached."));

            ContentDefinitionManager.AlterPartDefinition("ContainablePart", part => part
                .WithDescription("Allows your content item to be contained by a content item that has the ContainerPart attached."));

            ContentDefinitionManager.AlterPartDefinition("CustomPropertiesPart", part => part
                .WithDescription("Adds 3 custom properties to your content item."));
            return 4;
        }

        public int UpdateFrom4() {
            ContentDefinitionManager.DeleteTypeDefinition("CustomPropertiesPart");
            SchemaBuilder.DropTable("CustomPropertiesPartRecord");
            SchemaBuilder.AlterTable("ContainerPartRecord", table => {
                table.DropColumn("ItemContentType");
                table.AddColumn<string>("ItemContentTypes");
                table.AddColumn<bool>("ShowOnAdminMenu");
                table.AddColumn<string>("AdminMenuText", c => c.WithLength(50));
                table.AddColumn<string>("AdminMenuPosition", c => c.WithLength(50));
                table.AddColumn<string>("AdminMenuImageSet", c => c.WithLength(50));
                table.AddColumn<bool>("EnablePositioning");
                table.AddColumn<string>("AdminListViewName", c => c.WithLength(50));
                table.AddColumn<int>("ItemCount");
            });

            SchemaBuilder.AlterTable("ContainablePartRecord", table => {
                table.DropColumn("Weight");
                table.AddColumn<int>("Position");
            });

            return 6;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("ContainerWidgetPartRecord", table => table
                .AddColumn<string>("OrderByProperty", c => c.WithLength(64)));

            SchemaBuilder.AlterTable("ContainerWidgetPartRecord", table => table
                .AddColumn<int>("OrderByDirection"));

            SchemaBuilder.AlterTable("ContainerWidgetPartRecord", table => table
                .AddColumn<bool>("ApplyFilter"));

            SchemaBuilder.AlterTable("ContainerWidgetPartRecord", table => table
                .AddColumn<string>("FilterByProperty", c => c.WithLength(64)));

            SchemaBuilder.AlterTable("ContainerWidgetPartRecord", table => table
                .AddColumn<string>("FilterByOperator", c => c.WithLength(4)));

            SchemaBuilder.AlterTable("ContainerWidgetPartRecord", table => table
                .AddColumn<string>("FilterByValue", c => c.WithLength(128)));

            return 6;
        }
    }
}