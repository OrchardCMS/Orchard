using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Core.Common {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("BodyPartRecord", 
                table => table
                    .ContentPartVersionRecord()
                    .Column<string>("Text", column => column.Unlimited())
                    .Column<string>("Format")
                );

            SchemaBuilder.CreateTable("CommonPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<int>("OwnerId")
                    .Column<DateTime>("CreatedUtc")
                    .Column<DateTime>("PublishedUtc")
                    .Column<DateTime>("ModifiedUtc")
                    .Column<int>("Container_id")
                );
            
            SchemaBuilder.CreateTable("CommonPartVersionRecord", 
                table => table
                    .ContentPartVersionRecord()
                    .Column<DateTime>("CreatedUtc")
                    .Column<DateTime>("PublishedUtc")
                    .Column<DateTime>("ModifiedUtc")
                );

            ContentDefinitionManager.AlterPartDefinition("BodyPart", builder => builder.Attachable());
            ContentDefinitionManager.AlterPartDefinition("CommonPart", builder => builder.Attachable());

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("IdentityPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("Identifier", column => column.Unlimited())
                );
            ContentDefinitionManager.AlterPartDefinition("IdentityPart", builder => builder.Attachable());

            return 2;
        }
    }
}