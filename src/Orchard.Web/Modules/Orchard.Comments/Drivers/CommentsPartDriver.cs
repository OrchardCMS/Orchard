using System;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentsPartDriver : ContentPartDriver<CommentsPart> {
        private readonly ICommentService _commentService;
    
        public CommentsPartDriver(ICommentService commentService) {
            _commentService = commentService;
        }

        protected override DriverResult Display(CommentsPart part, string displayType, dynamic shapeHelper) {
            if (part.CommentsShown == false)
                return null;

            var commentsForCommentedContent = _commentService.GetCommentsForCommentedContent(part.ContentItem.Id);
            Func<int> pendingCount = () => commentsForCommentedContent.Where(x => x.Status == CommentStatus.Pending).Count();
            Func<int> approvedCount = () => commentsForCommentedContent.Where(x => x.Status == CommentStatus.Approved).Count();

            return Combined(
                ContentShape("Parts_Comments",
                    () => shapeHelper.Parts_Comments()),
                ContentShape("Parts_Comments_Count",
                    () => shapeHelper.Parts_Comments_Count(CommentCount: approvedCount(), PendingCount: pendingCount())), 
                ContentShape("Parts_Comments_Count_SummaryAdmin",
                    () => shapeHelper.Parts_Comments_Count_SummaryAdmin(CommentCount: approvedCount(), PendingCount: pendingCount()))
            );
        }

        protected override DriverResult Editor(CommentsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Comments_Enable",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Comments.Comments", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CommentsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(CommentsPart part, ContentManagement.Handlers.ImportContentContext context) {
            var commentsShown = context.Attribute(part.PartDefinition.Name, "CommentsShown");
            if (commentsShown != null) {
                part.CommentsShown = Convert.ToBoolean(commentsShown);
            }

            var commentsActive = context.Attribute(part.PartDefinition.Name, "CommentsActive");
            if (commentsActive != null) {
                part.CommentsActive = Convert.ToBoolean(commentsActive);
            }
        }

        protected override void Exporting(CommentsPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("CommentsShown", part.CommentsShown);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CommentsActive", part.CommentsActive);
        }
    }
}