using System.Collections.Generic;
using Orchard.Comments.Models;
using Orchard.ContentManagement;

namespace Orchard.Comments.Services {
    public interface ICommentService : IDependency {
        IEnumerable<CommentPart> GetComments();
        IEnumerable<CommentPart> GetComments(CommentStatus status);
        IEnumerable<CommentPart> GetCommentsForCommentedContent(int id);
        IEnumerable<CommentPart> GetCommentsForCommentedContent(int id, CommentStatus status);
        CommentPart GetComment(int id);
        ContentItemMetadata GetDisplayForCommentedContent(int id);
        CommentPart CreateComment(CreateCommentContext commentRecord, bool moderateComments);
        void UpdateComment(int id, string name, string email, string siteName, string commentText, CommentStatus status);
        void ApproveComment(int commentId);
        void PendComment(int commentId);
        void MarkCommentAsSpam(int commentId);
        void DeleteComment(int commentId);
        bool CommentsClosedForCommentedContent(int id);
        void CloseCommentsForCommentedContent(int id);
        void EnableCommentsForCommentedContent(int id);
    }
}