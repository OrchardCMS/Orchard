using System;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Comments {
    public class Migrations : DataMigrationImpl {
        private readonly ICommentService _commentService;

        public Migrations(ICommentService commentService) {
            _commentService = commentService;
        }

        public int Create() {
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
                .Column<int>("CommentsPartRecord_id")
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
                   .WithPart("IdentityPart")
                );

            ContentDefinitionManager.AlterTypeDefinition("Blog",
               cfg => cfg
                   .WithPart("CommentsContainerPart")
                );

            ContentDefinitionManager.AlterPartDefinition("CommentsPart", builder => builder.Attachable());

            return 3;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("Comment", cfg => cfg.WithPart("IdentityPart"));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("CommentPartRecord", table => table
                .AddColumn<int>("CommentsPartRecord_id")
                );

            // populate the CommentsPartRecord.Comments property
            foreach(var comment in _commentService.GetComments().List()) {
                var commentedContent = _commentService.GetCommentedContent(comment.Record.CommentedOn);
                var commentsPart = commentedContent.As<CommentsPart>();
                
                // the comment part might have been removed since the comment was placed
                if(commentsPart != null) {
                    commentsPart.Record.CommentPartRecords.Add(comment.Record);
                }
            }
            
            return 3;
        }
    }
}