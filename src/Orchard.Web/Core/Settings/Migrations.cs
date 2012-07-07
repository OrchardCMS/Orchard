using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Core.Settings {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ContentFieldDefinitionRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                );

            SchemaBuilder.CreateTable("ContentPartDefinitionRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                    .Column<bool>("Hidden")
                    .Column<string>("Settings", column => column.Unlimited())
                );

            SchemaBuilder.CreateTable("ContentPartFieldDefinitionRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                    .Column<string>("Settings", column => column.Unlimited())
                    .Column<int>("ContentFieldDefinitionRecord_id")
                    .Column<int>("ContentPartDefinitionRecord_Id")
                );

            SchemaBuilder.CreateTable("ContentTypeDefinitionRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                    .Column<string>("DisplayName")
                    .Column<bool>("Hidden")
                    .Column<string>("Settings", column => column.Unlimited())
                );

            SchemaBuilder.CreateTable("ContentTypePartDefinitionRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Settings", column => column.Unlimited())
                    .Column<int>("ContentPartDefinitionRecord_id")
                    .Column<int>("ContentTypeDefinitionRecord_Id")
                );

            SchemaBuilder.CreateTable("ShellDescriptorRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("SerialNumber")
                );

            SchemaBuilder.CreateTable("ShellFeatureRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                    .Column<int>("ShellDescriptorRecord_id"));

            SchemaBuilder.CreateTable("ShellFeatureStateRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                    .Column<string>("InstallState")
                    .Column<string>("EnableState")
                    .Column<int>("ShellStateRecord_Id")
                );

            SchemaBuilder.CreateTable("ShellParameterRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Component")
                    .Column<string>("Name")
                    .Column<string>("Value")
                    .Column<int>("ShellDescriptorRecord_id")
                );

            SchemaBuilder.CreateTable("ShellStateRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Unused")
                );

            SchemaBuilder.CreateTable("SiteSettingsPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("SiteSalt")
                    .Column<string>("SiteName")
                    .Column<string>("SuperUser")
                    .Column<string>("PageTitleSeparator")
                    .Column<string>("HomePage")
                    .Column<string>("SiteCulture")
                    .Column<string>("ResourceDebugMode", c => c.WithDefault("FromAppSetting"))
                    .Column<int>("PageSize")
                    .Column<string>("SiteTimeZone")
                );

            SchemaBuilder.CreateTable("SiteSettings2PartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("BaseUrl", c => c.Unlimited())
                );

            return 3;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("SiteSettings2PartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("BaseUrl", c => c.Unlimited())
                );

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("SiteSettingsPartRecord",
                table => table
                    .AddColumn<string>("SiteTimeZone")
                );

            return 3;
        }

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterTypeDefinition("Site", cfg => { });

            return 4;            
        }
    }
}