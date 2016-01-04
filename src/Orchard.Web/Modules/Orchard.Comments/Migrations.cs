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
        private readonly IContentManager _contentManager;

        public Migrations(
            ICommentService commentService,
            IContentManager contentManager) {
            _commentService = commentService;
            _contentManager = contentManager;
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
                .Column<int>("RepliedOn", c => c.WithDefault(null))
                .Column<decimal>("Position")
                .Column<int>("CommentsPartRecord_id")
                );

            SchemaBuilder.CreateTable("CommentsPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("CommentsShown")
                .Column<bool>("CommentsActive")
                .Column<bool>("ThreadedComments")
                .Column<int>("CommentsCount")
                );

            ContentDefinitionManager.AlterPartDefinition("CommentPart", part => part
                .WithDescription("Used by the Comment content type."));

            ContentDefinitionManager.AlterPartDefinition("CommentsContainerPart", part => part
                .WithDescription("Adds support to a content type to contain comments."));

            ContentDefinitionManager.AlterTypeDefinition("Comment",
               cfg => cfg
                   .WithPart("CommentPart")
                   .WithPart("CommonPart", 
                        p => p
                            .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                            .WithSetting("DateEditorSettings.ShowDateEditor", "false"))
                   .WithIdentity()
                );

            ContentDefinitionManager.AlterTypeDefinition("Blog",
               cfg => cfg
                   .WithPart("CommentsContainerPart")
                );

            ContentDefinitionManager.AlterPartDefinition("CommentsPart", builder => builder
                .Attachable()
                .WithDescription("Allows content items to be commented on."));

            SchemaBuilder.AlterTable("CommentPartRecord",
               table => table
                   .CreateIndex("IDX_CommentedOn", "CommentedOn")
               );

            SchemaBuilder.AlterTable("CommentPartRecord",
                table => table
                    .CreateIndex("IDX_CommentedOnContainer", "CommentedOnContainer")
                );

            return 6;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("Comment", cfg => cfg.WithIdentity());

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

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterTypeDefinition("Comment",
               cfg => cfg
                   .WithPart("CommonPart",
                        p => p
                            .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                            .WithSetting("DateEditorSettings.ShowDateEditor", "false"))
                );

            SchemaBuilder.AlterTable("CommentSettingsPartRecord", table => table
                .DropColumn("AkismetKey")
               );

            SchemaBuilder.AlterTable("CommentSettingsPartRecord", table => table
                .DropColumn("AkismetUrl")
               );

            SchemaBuilder.AlterTable("CommentSettingsPartRecord", table => table
                .DropColumn("EnableSpamProtection")
               );

            SchemaBuilder.AlterTable("CommentPartRecord", table => table
                .AddColumn<int>("RepliedOn", c => c.WithDefault(null))
            );

            SchemaBuilder.AlterTable("CommentPartRecord", table => table
                .AddColumn<decimal>("Position")
            );

            SchemaBuilder.AlterTable("CommentsPartRecord", table => table
                .AddColumn<bool>("ThreadedComments")
                );

            // define the default value for positions
            foreach (var comment in _commentService.GetComments().List()) {
                comment.Position = comment.Id;

                // migrating the Spam value which is now deprecated
                if (comment.Status != CommentStatus.Approved) {
                    comment.Status = CommentStatus.Pending;
                }
            }
            
            return 4;
        }

        public int UpdateFrom4() {
            ContentDefinitionManager.AlterPartDefinition("CommentPart", part => part
                .WithDescription("Used by the Comment content type."));

            ContentDefinitionManager.AlterPartDefinition("CommentsContainerPart", part => part
                .WithDescription("Adds support to a content type to contain comments."));

            ContentDefinitionManager.AlterPartDefinition("CommentsPart", builder => builder
                .WithDescription("Allows content items to be commented on."));

            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("CommentsPartRecord", table => table
                .AddColumn<int>("CommentsCount")
                );

            SchemaBuilder.AlterTable("CommentPartRecord",
               table => table
                   .CreateIndex("IDX_CommentedOn", "CommentedOn")
               );

            SchemaBuilder.AlterTable("CommentPartRecord",
               table => table
                   .CreateIndex("IDX_CommentedOnContainer", "CommentedOnContainer")
               );

            // populate the CommentsPartRecord.CommentsCount property
            foreach (var commentsPart in _contentManager.Query<CommentsPart, CommentsPartRecord>().List()) {
                _commentService.ProcessCommentsCount(commentsPart.Id);
            }

            return 6;
        }
    }
}