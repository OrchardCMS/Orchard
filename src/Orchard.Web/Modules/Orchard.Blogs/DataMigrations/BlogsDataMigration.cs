using Orchard.Data.Migration;

namespace Orchard.Blogs.DataMigrations {
    public class BlogsDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Blogs_BlogArchiveRecord (Id  integer, Year INTEGER, Month INTEGER, PostCount INTEGER, Blog_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("BlogArchiveRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<int>("Year")
                .Column<int>("Month")
                .Column<int>("PostCount")
                .Column<int>("Blog_id")
                );

            //CREATE TABLE Orchard_Blogs_BlogRecord (Id INTEGER not null, Description TEXT, PostCount INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("BlogRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Description")
                .Column<int>("PostCount")
                );

            return 0010;
        }
    }
}