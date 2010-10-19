using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Comments {
    public class Migrations : DataMigrationImpl {
    
        public int Create() {
            SchemaBuilder.CreateTable("ClosedCommentsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("ContentItemId")
                );

            SchemaBuilder.CreateTable("CommentPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Author")
                .Column<string>("SiteName")
                .Column<string>("UserName")
                .Column<string>("Email")
                .Column<string>("Status")
                .Column<DateTime>("CommentDateUtc")
                .Column<string>("CommentText", column => column.Unlimited())
                .Column<int>("CommentedOn")
                .Column<int>("CommentedOnContainer")
                );

            SchemaBuilder.CreateTable("CommentSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("ModerateComments")
                .Column<bool>("EnableSpamProtection")
                .Column<string>("AkismetKey")
                .Column<string>("AkismetUrl")
               );

            SchemaBuilder.CreateTable("CommentsPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("CommentsShown")
                .Column<bool>("CommentsActive")
                );

            ContentDefinitionManager.AlterTypeDefinition("Comment",
               cfg => cfg
                   .WithPart("CommentPart")
                   .WithPart("CommonPart")
                );

            ContentDefinitionManager.AlterTypeDefinition("Blog",
               cfg => cfg
                   .WithPart("CommentsContainerPart")
                );

            ContentDefinitionManager.AlterPartDefinition("CommentsPart", builder => builder.Attachable());

            return 1;
        }
    }
}