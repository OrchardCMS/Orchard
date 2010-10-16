using System;
using System.Collections.Generic;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Core.Common {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            //CREATE TABLE Common_BodyPartRecord (Id INTEGER not null, Text TEXT, Format TEXT, ContentItemRecord_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("BodyPartRecord", table => table
                .ContentPartVersionRecord()
                .Column<string>("Text", column => column.Unlimited())
                .Column<string>("Format")
                );

            //CREATE TABLE Common_CommonPartRecord (Id INTEGER not null, OwnerId INTEGER, CreatedUtc DATETIME, PublishedUtc DATETIME, ModifiedUtc DATETIME, Container_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("CommonPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("OwnerId")
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("PublishedUtc")
                .Column<DateTime>("ModifiedUtc")
                .Column<int>("Container_id")
                );
            
            //CREATE TABLE Common_CommonPartVersionRecord (Id INTEGER not null, CreatedUtc DATETIME, PublishedUtc DATETIME, ModifiedUtc DATETIME, ContentItemRecord_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("CommonPartVersionRecord", table => table
                .ContentPartVersionRecord()
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("PublishedUtc")
                .Column<DateTime>("ModifiedUtc")
                );

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition(typeof(BodyPart).Name, cfg => cfg
                .WithLocation(new Dictionary<string, ContentLocation> {
                    {"Default", new ContentLocation { Zone = "Primary", Position = "5" }},
                }));
            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition(typeof(CommonPart).Name, cfg => cfg
                .WithLocation(new Dictionary<string, ContentLocation> {
                    {"Default", new ContentLocation { Zone = "metadata", Position = "5" }},
                    {"Editor", new ContentLocation { Zone = "Primary", Position = "20" }},
                }));
            return 3;
        }

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition("BodyPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("CommonPart", builder => builder.Attachable());
            return 4;
        }
    }
}