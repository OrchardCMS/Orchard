using System.Collections.Generic;
using Orchard.Comments.Models;
using Orchard.ContentManagement;

namespace Orchard.Comments.Services {
    public interface ICommentService : IDependency {
        IEnumerable<Comment> GetComments();
        IEnumerable<Comment> GetComments(CommentStatus status);
        IEnumerable<Comment> GetCommentsForCommentedContent(int id);
        IEnumerable<Comment> GetCommentsForCommentedContent(int id, CommentStatus status);
        Comment GetComment(int id);
        ContentItemMetadata GetDisplayForCommentedContent(int id);
        Comment CreateComment(CreateCommentContext commentRecord, bool moderateComments);
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