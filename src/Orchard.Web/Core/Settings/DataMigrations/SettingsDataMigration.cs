using Orchard.DataMigration;

namespace Orchard.Core.Settings.DataMigrations {
    public class SettingsDataMigration : DataMigrationImpl {
        public override string Feature {
            get { return "Settings"; }
        }

        public int Create() {
            //CREATE TABLE Settings_ContentFieldDefinitionRecord (Id  integer, Name TEXT, primary key (Id));
            SchemaBuilder.CreateTable("Settings_ContentFieldDefinitionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Name")
                );

            //CREATE TABLE Settings_ContentPartDefinitionRecord (Id  integer, Name TEXT, Hidden INTEGER, Settings TEXT, primary key (Id));
            SchemaBuilder.CreateTable("Settings_ContentPartDefinitionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Name")
                .Column<int>("Hidden")
                .Column<string>("Settings")
                );

            //CREATE TABLE Settings_ContentPartFieldDefinitionRecord (Id  integer, Name TEXT, Settings TEXT, ContentFieldDefinitionRecord_id INTEGER,  INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("Settings_ContentPartFieldDefinitionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Name")
                .Column<string>("Settings")
                .Column<int>("ContentFieldDefinitionRecord_id")
                .Column<int>("ContentPartDefinitionRecord_Id")
                );

            //CREATE TABLE Settings_ContentTypeDefinitionRecord (Id  integer, Name TEXT, DisplayName TEXT, Hidden INTEGER, Settings TEXT, primary key (Id));
            SchemaBuilder.CreateTable("Settings_ContentTypeDefinitionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Name")
                .Column<string>("DisplayName")
                .Column<int>("Hidden")
                .Column<string>("Settings")
                );

            //CREATE TABLE Settings_ContentTypePartDefinitionRecord (Id  integer, Settings TEXT, ContentPartDefinitionRecord_id INTEGER, ContentTypeDefinitionRecord_Id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("Settings_ContentTypePartDefinitionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Settings")
                .Column<int>("ContentPartDefinitionRecord_id")
                .Column<int>("ContentTypeDefinitionRecord_Id")
                );

            //CREATE TABLE Settings_ShellDescriptorRecord (Id  integer, SerialNumber INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("Settings_ShellDescriptorRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<int>("SerialNumber")
                );

            //CREATE TABLE Settings_ShellFeatureRecord (Id  integer, Name TEXT, ShellDescriptorRecord_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("Settings_ShellFeatureRecord", table => table
                    .Column<int>("Id", column => column.PrimaryKey())
                    .Column<string>("Name")
                    .Column<int>("ShellDescriptorRecord_id"));

            //CREATE TABLE Settings_ShellFeatureStateRecord (Id  integer, Name TEXT, InstallState TEXT, EnableState TEXT, ShellStateRecord_Id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("Settings_ShellFeatureStateRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Name")
                .Column<string>("InstallState")
                .Column<string>("EnableState")
                .Column<int>("ShellStateRecord_Id")
                );

            //CREATE TABLE Settings_ShellParameterRecord (Id  integer, Component TEXT, Name TEXT, Value TEXT, ShellDescriptorRecord_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("Settings_ShellParameterRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Component")
                .Column<string>("Name")
                .Column<string>("Value")
                .Column<int>("ShellDescriptorRecord_id")
                );

            //CREATE TABLE Settings_ShellStateRecord (Id  integer, primary key (Id));
            SchemaBuilder.CreateTable("Settings_ShellStateRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Name")
                );

            return 0010;
        }
    }
}