using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Data.Migration;

namespace Orchard.Blogs {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Blogs_BlogPartArchiveRecord (Id  integer, Year INTEGER, Month INTEGER, PostCount INTEGER, Blog_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("BlogPartArchiveRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("Year")
                .Column<int>("Month")
                .Column<int>("PostCount")
                .Column<int>("BlogPart_id")
                );

            //CREATE TABLE Orchard_Blogs_BlogPartRecord (Id INTEGER not null, Description TEXT, PostCount INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("BlogPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Description")
                .Column<int>("PostCount")
                );

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("Blog",
                cfg => cfg
                    .WithPart("BlogPart")
                    .WithPart("CommonPart")
                    .WithPart("RoutePart")
                );

            ContentDefinitionManager.AlterTypeDefinition("BlogPost", 
                cfg => cfg
                    .WithPart("BlogPostPart")
                    .WithPart("CommonPart")
                    .WithPart("PublishLaterPart")
                    .WithPart("RoutePart")
                    .WithPart("BodyPart")
                );

            return 2;
        }
        
        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition(typeof(BlogPart).Name, cfg => cfg
                .WithLocation(new Dictionary<string, ContentLocation> {
                    {"Editor", new ContentLocation { Zone = "primary", Position = "1" }}
                }));
            return 3;
        }
    }
}