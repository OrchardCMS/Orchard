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
                .Column<string>("Event", c => c.WithLength(256))
                .Column<string>("Category", c => c.WithLength(64))
                .Column<int>("ContentItemVersion_Id")
                .Column<string>("EventData", c => c.Unlimited())
                .Column<string>("EventFilterKey", c => c.WithLength(16))
                .Column<string>("EventFilterData", c => c.WithLength(256))
                .Column<string>("Comment", c => c.Unlimited()));

            ContentDefinitionManager.AlterPartDefinition("AuditTrailCommentPart", part => part
                .Attachable()
                .WithDescription("Enables the user to enter a comment about the change when saving a content item."));

            return 1;
        }
    }
}