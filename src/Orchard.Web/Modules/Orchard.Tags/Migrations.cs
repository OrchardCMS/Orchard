using System.Collections.Generic;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Tags.Models;

namespace Orchard.Tags {
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

            return 1;
        }
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition(typeof(TagsPart).Name, cfg => cfg
                .WithLocation(new Dictionary<string, ContentLocation> {
                    {"Default", new ContentLocation { Zone = "Primary", Position = "49" }},
                    {"Editor", new ContentLocation { Zone = "Primary", Position = "9" }},
                }));
            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition("TagsPart", builder => builder.Attachable());
            return 3;
        }
    }
}