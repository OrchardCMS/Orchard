using Orchard.Data.Migration;

namespace Orchard.Tags.DataMigrations {
    public class TagsDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Tags_Tag (Id  integer, TagName TEXT, primary key (Id));
            SchemaBuilder.CreateTable("Tag", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("TagName")
                );

            //CREATE TABLE Orchard_Tags_TagsContentItems (Id  integer, TagId INTEGER, ContentItemId INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("TagsContentItems", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("TagId")
                .Column<int>("ContentItemId")
                );

            return 0010;
        }
    }
}