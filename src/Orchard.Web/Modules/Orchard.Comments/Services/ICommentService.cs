using Orchard.Comments.Models;
using Orchard.ContentManagement;

namespace Orchard.Comments.Services {
    public interface ICommentService : IDependency {
        IContentQuery<CommentPart, CommentPartRecord> GetComments();
        IContentQuery<CommentPart, CommentPartRecord> GetComments(CommentStatus status);
        IContentQuery<CommentPart, CommentPartRecord> GetCommentsForCommentedContent(int id);
        IContentQuery<CommentPart, CommentPartRecord> GetCommentsForCommentedContent(int id, CommentStatus status);
        CommentPart GetComment(int id);
        ContentItemMetadata GetDisplayForCommentedContent(int id);
        ContentItem GetCommentedContent(int id);
        CommentPart CreateComment(CreateCommentContext commentRecord, bool moderateComments);
        void UpdateComment(int id, string name, string email, string siteName, string commentText, CommentStatus status);
        void ApproveComment(int commentId);
        void UnapproveComment(int commentId);
        void MarkCommentAsSpam(int commentId);
        void DeleteComment(int commentId);
        bool CommentsDisabledForCommentedContent(int id);
        void DisableCommentsForCommentedContent(int id);
        void EnableCommentsForCommentedContent(int id);
    }
}