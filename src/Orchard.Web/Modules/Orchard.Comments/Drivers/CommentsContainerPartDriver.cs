using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentsContainerPartDriver : ContentPartDriver<CommentsContainerPart> {
        private readonly ICommentService _commentService;

        public CommentsContainerPartDriver(ICommentService commentService) {
            _commentService = commentService;
        }

        protected override DriverResult Display(CommentsContainerPart part, string displayType, dynamic shapeHelper) {
            var commentsForCommentedContent = _commentService.GetCommentsForCommentedContent(part.ContentItem.Id);
            
            return Combined(

                ContentShape("Parts_Comments_Count",
                    () => shapeHelper.Parts_Comments_Count(ContentPart: part, CommentCount: commentsForCommentedContent.Count(), PendingCount: commentsForCommentedContent.Where(x => x.Status == CommentStatus.Pending).Count())),
                ContentShape("Parts_Comments_Count_SummaryAdmin",
                    () => shapeHelper.Parts_Comments_Count_SummaryAdmin(ContentPart: part, CommentCount: commentsForCommentedContent.Count(), PendingCount: commentsForCommentedContent.Where(x => x.Status == CommentStatus.Pending).Count()))
                );
        }
    }
}