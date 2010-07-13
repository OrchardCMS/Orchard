using Orchard.Data.Migration;

namespace Orchard.Core.Settings.DataMigrations {
    public class SettingsDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Settings_ContentFieldDefinitionRecord (Id  integer, Name TEXT, primary key (Id));
            SchemaBuilder.CreateTable("ContentFieldDefinitionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name")
                );

            //CREATE TABLE Settings_ContentPartDefinitionRecord (Id  integer, Name TEXT, Hidden INTEGER, Settings TEXT, primary key (Id));
            SchemaBuilder.CreateTable("ContentPartDefinitionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name")
                .Column<bool>("Hidden")
                .Column<string>("Settings")
                );

            //CREATE TABLE Settings_ContentPartFieldDefinitionRecord (Id  integer, Name TEXT, Settings TEXT, ContentFieldDefinitionRecord_id INTEGER,  INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("ContentPartFieldDefinitionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name")
                .Column<string>("Settings")
                .Column<int>("ContentFieldDefinitionRecord_id")
                .Column<int>("ContentPartDefinitionRecord_Id")
                );

            //CREATE TABLE Settings_ContentTypeDefinitionRecord (Id  integer, Name TEXT, DisplayName TEXT, Hidden INTEGER, Settings TEXT, primary key (Id));
            SchemaBuilder.CreateTable("ContentTypeDefinitionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name")
                .Column<string>("DisplayName")
                .Column<bool>("Hidden")
                .Column<string>("Settings")
                );

            //CREATE TABLE Settings_ContentTypePartDefinitionRecord (Id  integer, Settings TEXT, ContentPartDefinitionRecord_id INTEGER, ContentTypeDefinitionRecord_Id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("ContentTypePartDefinitionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Settings")
                .Column<int>("ContentPartDefinitionRecord_id")
                .Column<int>("ContentTypeDefinitionRecord_Id")
                );

            //CREATE TABLE Settings_ShellDescriptorRecord (Id  integer, SerialNumber INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("ShellDescriptorRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("SerialNumber")
                );

            //CREATE TABLE Settings_ShellFeatureRecord (Id  integer, Name TEXT, ShellDescriptorRecord_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("ShellFeatureRecord", table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                    .Column<int>("ShellDescriptorRecord_id"));

            //CREATE TABLE Settings_ShellFeatureStateRecord (Id  integer, Name TEXT, InstallState TEXT, EnableState TEXT, ShellStateRecord_Id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("ShellFeatureStateRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name")
                .Column<string>("InstallState")
                .Column<string>("EnableState")
                .Column<int>("ShellStateRecord_Id")
                );

            //CREATE TABLE Settings_ShellParameterRecord (Id  integer, Component TEXT, Name TEXT, Value TEXT, ShellDescriptorRecord_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("ShellParameterRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Component")
                .Column<string>("Name")
                .Column<string>("Value")
                .Column<int>("ShellDescriptorRecord_id")
                );

            //CREATE TABLE Settings_ShellStateRecord (Id  integer, primary key (Id));
            SchemaBuilder.CreateTable("ShellStateRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Unused")
                );

            //CREATE TABLE Settings_SiteSettingsRecord (Id INTEGER not null, SiteSalt TEXT, SiteName TEXT, SuperUser TEXT, PageTitleSeparator TEXT, HomePage TEXT, SiteCulture TEXT, primary key (Id));
            SchemaBuilder.CreateTable("SiteSettingsRecord", table => table
                .ContentPartRecord()
                .Column<string>("SiteSalt")
                .Column<string>("SiteName")
                .Column<string>("SuperUser")
                .Column<string>("PageTitleSeparator")
                .Column<string>("HomePage")
                .Column<string>("SiteCulture")
                );

            return 0010;
        }
    }
}