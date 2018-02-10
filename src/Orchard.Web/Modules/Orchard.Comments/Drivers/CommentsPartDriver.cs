using System;
using System.Linq;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.Comments.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Comments.Drivers {
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

            return Combined(
                ContentShape("Parts_ListOfComments",
                    () => {
                        if (part.CommentsShown == false)
                            return null;

                        // create a hierarchy of shapes
                        var firstLevelShapes = new List<dynamic>();
                        var allShapes = new Dictionary<int, dynamic>();
                        var comments = _commentService
                            .GetCommentsForCommentedContent(part.ContentItem.Id)
                            .Where(x => x.Status == CommentStatus.Approved)
                            .OrderBy(x => x.Position)
                            .List()
                            .ToList();

                        foreach (var item in comments) {
                            var shape = shapeHelper.Parts_Comment(ContentPart: item, ContentItem: item.ContentItem);
                            allShapes.Add(item.Id, shape);
                        }

                        foreach (var item in comments) {
                            var shape = allShapes[item.Id];
                            if (item.RepliedOn.HasValue) {
                                allShapes[item.RepliedOn.Value].Add(shape);
                            }
                            else {
                                firstLevelShapes.Add(shape);
                            }
                        }

                        var list = shapeHelper.List(Items: firstLevelShapes);

                        return shapeHelper.Parts_ListOfComments(
                            List: list,
                            CommentCount: part.CommentsCount);
                    }),
                ContentShape("Parts_CommentForm",
                    () => {
                        if (part.CommentsShown == false)
                            return null;

                        var newComment = _contentManager.New("Comment");
                        if (newComment.Has<CommentPart>()) newComment.As<CommentPart>().CommentedOn = part.Id;
                        var editorShape = _contentManager.BuildEditor(newComment);

                        return shapeHelper.Parts_CommentForm(EditorShape: editorShape, CanStillComment: _commentService.CanStillCommentOn(part));
                    }),
                ContentShape("Parts_Comments_Count",
                    () => {
                        if (part.CommentsShown == false)
                            return null;

                        return shapeHelper.Parts_Comments_Count(
                            CommentCount: part.CommentsCount);
                    }),
                ContentShape("Parts_Comments_Count_SummaryAdmin",
                    () => {

                        var comments = _commentService
                            .GetCommentsForCommentedContent(part.ContentItem.Id);
                        var pendingCount = comments
                            .Where(x => x.Status == CommentStatus.Pending)
                            .Count();

                        return shapeHelper.Parts_Comments_Count_SummaryAdmin(
                            CommentCount: part.CommentsCount,
                            PendingCount: pendingCount);
                    })
            );
        }

        protected override DriverResult Editor(CommentsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Comments_Enable",
                                () => {
                                    // if the part is new, then apply threaded comments defaults
                                    if(!part.ContentItem.HasDraft() && !part.ContentItem.HasPublished()) {
                                        var settings = part.TypePartDefinition.Settings.GetModel<CommentsPartSettings>();
                                        part.ThreadedComments = settings.DefaultThreadedComments;
                                    }
                                    return shapeHelper.EditorTemplate(TemplateName: "Parts.Comments.Comments", Model: part, Prefix: Prefix);
                                });
        }

        protected override DriverResult Editor(CommentsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(CommentsPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "CommentsShown", commentsShown =>
                part.CommentsShown = Convert.ToBoolean(commentsShown)
            );

            context.ImportAttribute(part.PartDefinition.Name, "CommentsActive", commentsActive =>
                part.CommentsActive = Convert.ToBoolean(commentsActive)
            );

            context.ImportAttribute(part.PartDefinition.Name, "ThreadedComments", threadedComments =>
                part.ThreadedComments = Convert.ToBoolean(threadedComments)
            );
        }

        protected override void Exporting(CommentsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("CommentsShown", part.CommentsShown);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CommentsActive", part.CommentsActive);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ThreadedComments", part.ThreadedComments);
        }

        protected override void Cloning(CommentsPart originalPart, CommentsPart clonePart, CloneContentContext context) {
            clonePart.CommentsShown = originalPart.CommentsShown;
            clonePart.CommentsActive = originalPart.CommentsActive;
            // ThreadedComments will be overrided with settings default
        }
    }
}