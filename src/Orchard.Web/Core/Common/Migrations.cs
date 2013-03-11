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

            SchemaBuilder.CreateTable("IdentityPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("Identifier", column => column.Unlimited())
                );

            ContentDefinitionManager.AlterPartDefinition("BodyPart", builder => builder
                .Attachable()
                .WithDescription("Allows the editing of text using an editor provided by the configured flavor (e.g. html, text, markdown)."));

            ContentDefinitionManager.AlterPartDefinition("CommonPart", builder => builder
                .Attachable()
                .WithDescription("Provides common information about a content item, such as Owner, Date Created, Date Published and Date Modified."));

            ContentDefinitionManager.AlterPartDefinition("IdentityPart", builder => builder
                .Attachable()
                .WithDescription("Automatically generates a unique identity for the content item, which is required in import/export scenarios where one content item references another."));

            return 3;
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

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition("BodyPart", builder => builder
                .WithDescription("Allows the editing of text using an editor provided by the configured flavor (e.g. html, text, markdown)."));

            ContentDefinitionManager.AlterPartDefinition("CommonPart", builder => builder
                .WithDescription("Provides common information about a content item, such as Owner, Date Created, Date Published and Date Modified."));

            ContentDefinitionManager.AlterPartDefinition("IdentityPart", builder => builder
                .WithDescription("Automatically generates a unique identity for the content item, which is required in import/export scenarios where one content item references another."));

            return 3;
        }
    }
}