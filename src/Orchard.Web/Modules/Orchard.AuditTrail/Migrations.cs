using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.AuditTrail {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("AuditTrailEventRecord", table => table
                .Column<int>("Id", c => c.PrimaryKey().Identity())
                .Column<DateTime>("CreatedUtc")
                .Column<string>("UserName", c => c.WithLength(64))
                .Column<string>("EventName", c => c.WithLength(128))
                .Column<string>("FullEventName", c => c.WithLength(512))
                .Column<string>("Category", c => c.WithLength(64))
                .Column<int>("ContentItemVersion_Id")
                .Column<string>("EventData", c => c.Unlimited())
                .Column<string>("EventFilterKey", c => c.WithLength(16))
                .Column<string>("EventFilterData", c => c.WithLength(256))
                .Column<string>("Comment", c => c.Unlimited())
                .Column<string>("ClientIpAddress", c => c.WithLength(46)));

            ContentDefinitionManager.AlterPartDefinition("AuditTrailPart", part => part
                .Attachable()
                .WithDescription("Adds an inline audit trail to content items, and allows editors to enter a comment when saving content items."));

            return 2;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("AuditTrailEventRecord", table => table
                .AddColumn<string>("ClientIpAddress", c => c.WithLength(46)));
            return 2;
        }
    }
}