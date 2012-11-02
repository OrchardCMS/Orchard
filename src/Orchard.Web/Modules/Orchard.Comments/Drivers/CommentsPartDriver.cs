using System;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System.Collections.Generic;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentsPartDriver : ContentPartDriver<CommentsPart> {
        private readonly ICommentService _commentService;
        private readonly IContentManager _contentManager;

        public CommentsPartDriver(
            ICommentService commentService,
            IContentManager contentManager) {
            _commentService = commentService;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(CommentsPart part, string displayType, dynamic shapeHelper) {
            if (part.CommentsShown == false)
                return null;

            var commentsForCommentedContent = _commentService.GetCommentsForCommentedContent(part.ContentItem.Id);
            Func<int> pendingCount = () => commentsForCommentedContent.Where(x => x.Status == CommentStatus.Pending).Count();
            Func<int> approvedCount = () => commentsForCommentedContent.Where(x => x.Status == CommentStatus.Approved).Count();

            return Combined(
                ContentShape("Parts_ListOfComments",
                    () => {
                        // create a hierarchy of shapes
                        var commentShapes = new List<dynamic>();
                        var index = new Dictionary<int, dynamic>();

                        foreach (var item in part.Comments) {
                            var shape = _contentManager.BuildDisplay(item.ContentItem, "Summary");
                            index.Add(item.Id, shape);

                            if (!item.RepliedOn.HasValue) {
                                commentShapes.Add(shape);
                            }
                            else {
                                if (index.ContainsKey(item.RepliedOn.Value)) {
                                    var parent = index[item.RepliedOn.Value];
                                    if (parent.CommentShapes == null) {
                                        parent.CommentShapes = new List<dynamic>();
                                    }

                                    parent.CommentShapes.Add(shape);
                                }
                            }
                        }

                        return shapeHelper.Parts_ListOfComments(CommentShapes: commentShapes, CommentCount: approvedCount());
                    }),
                ContentShape("Parts_CommentForm",
                    () => {
                        var newComment = _contentManager.New("Comment");
                        if (newComment.Has<CommentPart>()) newComment.As<CommentPart>().CommentedOn = part.Id;
                        var editorShape = _contentManager.BuildEditor(newComment);

                        return shapeHelper.Parts_CommentForm(EditorShape: editorShape);
                    }),
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

            var threadedComments = context.Attribute(part.PartDefinition.Name, "ThreadedComments");
            if (threadedComments != null) {
                part.ThreadedComments = Convert.ToBoolean(threadedComments);
            }
        }

        protected override void Exporting(CommentsPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("CommentsShown", part.CommentsShown);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CommentsActive", part.CommentsActive);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ThreadedComments", part.ThreadedComments);
        }
    }
}