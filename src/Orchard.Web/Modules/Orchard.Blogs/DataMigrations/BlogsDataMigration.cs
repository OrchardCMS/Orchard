using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Blogs.DataMigrations {
    public class BlogsDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Blogs_BlogArchiveRecord (Id  integer, Year INTEGER, Month INTEGER, PostCount INTEGER, Blog_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("BlogArchiveRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("Year")
                .Column<int>("Month")
                .Column<int>("PostCount")
                .Column<int>("Blog_id")
                );

            //CREATE TABLE Orchard_Blogs_BlogRecord (Id INTEGER not null, Description TEXT, PostCount INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("BlogRecord", table => table
                .ContentPartRecord()
                .Column<string>("Description")
                .Column<int>("PostCount")
                );

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("Blog",
                cfg => cfg
                    .WithPart("Blog")
                    .WithPart("CommonAspect")
                    .WithPart("IsRoutable")
                );

            ContentDefinitionManager.AlterTypeDefinition("BlogPost", 
                cfg => cfg
                    .WithPart("BlogPost")
                    .WithPart("CommonAspect")
                    .WithPart("PublishLaterPart")
                    .WithPart("IsRoutable")
                    .WithPart("BodyAspect")
                );

            return 2;
        }
    }
}