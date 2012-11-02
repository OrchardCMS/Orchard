using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Logging;
using Orchard.ContentManagement;

namespace Orchard.Comments.Services {
    [UsedImplicitly]
    public class CommentService : ICommentService {
        private readonly IOrchardServices _orchardServices;

        public CommentService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public CommentPart GetComment(int id) {
            return _orchardServices.ContentManager.Get<CommentPart>(id);
        }
        
        public IContentQuery<CommentPart, CommentPartRecord> GetComments() {
            return _orchardServices.ContentManager
                       .Query<CommentPart, CommentPartRecord>();
        }

        public IContentQuery<CommentPart, CommentPartRecord> GetComments(CommentStatus status) {
            return GetComments()
                       .Where(c => c.Status == status);
        }

        public IContentQuery<CommentPart, CommentPartRecord> GetCommentsForCommentedContent(int id) {
            return GetComments()
                       .Where(c => c.CommentedOn == id || c.CommentedOnContainer == id);
        }

        public IContentQuery<CommentPart, CommentPartRecord> GetCommentsForCommentedContent(int id, CommentStatus status) {
            return GetCommentsForCommentedContent(id)
                       .Where(c => c.Status == status);
        }

        public ContentItemMetadata GetDisplayForCommentedContent(int id) {
            var content = GetCommentedContent(id);
            if (content == null)
                return null;
            return _orchardServices.ContentManager.GetItemMetadata(content);
        }

        public ContentItem GetCommentedContent(int id) {
            var result = _orchardServices.ContentManager.Get(id, VersionOptions.Published);
            if (result == null)
                result = _orchardServices.ContentManager.Get(id, VersionOptions.Draft);
            return result;
        }

        public void ApproveComment(int commentId) {
            var commentPart = GetCommentWithQueryHints(commentId);
            commentPart.Record.Status = CommentStatus.Approved;
        }

        public void UnapproveComment(int commentId) {
            var commentPart = GetCommentWithQueryHints(commentId);
            commentPart.Record.Status = CommentStatus.Pending;
        }

        public void MarkCommentAsSpam(int commentId) {
            var commentPart = GetCommentWithQueryHints(commentId);
            commentPart.Record.Status = CommentStatus.Spam;
        }

        public void DeleteComment(int commentId) {
            _orchardServices.ContentManager.Remove(_orchardServices.ContentManager.Get(commentId));
        }

        public bool CommentsDisabledForCommentedContent(int id) {
            return !_orchardServices.ContentManager.Get<CommentsPart>(id, VersionOptions.Latest).CommentsActive;
        }

        public void DisableCommentsForCommentedContent(int id) {
            _orchardServices.ContentManager.Get<CommentsPart>(id, VersionOptions.Latest).CommentsActive = false;
        }

        public void EnableCommentsForCommentedContent(int id) {
            _orchardServices.ContentManager.Get<CommentsPart>(id, VersionOptions.Latest).CommentsActive = true;
        }

        private CommentPart GetCommentWithQueryHints(int id) {
            return _orchardServices.ContentManager.Get<CommentPart>(id, VersionOptions.Latest, new QueryHints().ExpandParts<CommentPart>());
        }
    }
}
