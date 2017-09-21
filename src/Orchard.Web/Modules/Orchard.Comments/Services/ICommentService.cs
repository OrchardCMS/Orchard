using System;
using Orchard.Comments.Models;
using Orchard.ContentManagement;

namespace Orchard.Comments.Services {
    public interface ICommentService : IDependency {
        IContentQuery<CommentPart, CommentPartRecord> GetComments();
        IContentQuery<CommentPart, CommentPartRecord> GetComments(CommentStatus status);
        IContentQuery<CommentPart, CommentPartRecord> GetCommentsForCommentedContent(int id);
        IContentQuery<CommentPart, CommentPartRecord> GetCommentsForCommentedContent(int id, CommentStatus status);
        IContentQuery<CommentPart, CommentPartRecord> GetCommentsForContainer(int id);
        CommentPart GetComment(int id);
        ContentItemMetadata GetDisplayForCommentedContent(int id);
        ContentItem GetCommentedContent(int id);
        void ProcessCommentsCount(int commentsPartId);
        void ApproveComment(int commentId);
        void UnapproveComment(int commentId);
        void DeleteComment(int commentId);
        bool CommentsDisabledForCommentedContent(int id);
        void DisableCommentsForCommentedContent(int id);
        void EnableCommentsForCommentedContent(int id);
        bool DecryptNonce(string nonce, out int id);
        string CreateNonce(CommentPart comment, TimeSpan delay);
        bool CanStillCommentOn(CommentsPart commentsPart);
        bool CanCreateComment(CommentPart commentPart);
        void SendNotificationEmail(CommentPart commentPart);
        string CreateProtectedUrl(string action, CommentPart part);
    }
}