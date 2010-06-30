using Orchard.Data.Migration;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions.Models;

namespace Orchard.ContentManagement.DataMigrations {
    public class FrameworkDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Framework_ContentItemRecord (Id  integer, Data TEXT, ContentType_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("ContentItemRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Data")
                .Column<int>("ContentType_id")
                );

            //CREATE TABLE Orchard_Framework_ContentItemVersionRecord (Id  integer, Number INTEGER, Published INTEGER, Latest INTEGER, Data TEXT, ContentItemRecord_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("ContentItemVersionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<int>("Number")
                .Column<bool>("Published")
                .Column<bool>("Latest")
                .Column<string>("Data")
                .Column<int>("ContentItemRecord_id")
                );

            //CREATE TABLE Orchard_Framework_ContentTypeRecord (Id  integer, Name TEXT, primary key (Id));
            SchemaBuilder.CreateTable("ContentTypeRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Name")
                );

            //CREATE TABLE Orchard_Framework_CultureRecord (Id  integer, Culture TEXT, primary key (Id));
            SchemaBuilder.CreateTable("CultureRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Culture")
                );

            return 0010;
        }
    }
}