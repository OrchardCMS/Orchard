using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Comments.Drivers;
using Orchard.Comments.Models;
using Orchard.ContentManagement.Aspects;
using Orchard.Data;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Comments.Services {
    [UsedImplicitly]
    public class CommentService : ICommentService {
        private readonly IRepository<ClosedCommentsRecord> _closedCommentsRepository;
        private readonly IClock _clock;
        private readonly ICommentValidator _commentValidator;
        private readonly IContentManager _contentManager;

        public CommentService(IRepository<ClosedCommentsRecord> closedCommentsRepository,
                              IClock clock,
                              ICommentValidator commentValidator,
                              IContentManager contentManager) {
            _closedCommentsRepository = closedCommentsRepository;
            _clock = clock;
            _commentValidator = commentValidator;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        protected virtual IUser CurrentUser { get; [UsedImplicitly] private set; }

        public IEnumerable<CommentPart> GetComments() {
            return _contentManager
                .Query<CommentPart, CommentPartRecord>()
                .List();
        }

        public IEnumerable<CommentPart> GetComments(CommentStatus status) {
            return _contentManager
                .Query<CommentPart, CommentPartRecord>()
                .Where(c => c.Status == status)
                .List();
        }

        public IEnumerable<CommentPart> GetCommentsForCommentedContent(int id) {
            return _contentManager
                .Query<CommentPart, CommentPartRecord>()
                .Where(c => c.CommentedOn == id || c.CommentedOnContainer == id)
                .List();
        }

        public IEnumerable<CommentPart> GetCommentsForCommentedContent(int id, CommentStatus status) {
            return _contentManager
                .Query<CommentPart, CommentPartRecord>()
                .Where(c => c.CommentedOn == id || c.CommentedOnContainer == id)
                .Where(ctx => ctx.Status == status)
                .List();
        }

        public CommentPart GetComment(int id) {
            return _contentManager.Get<CommentPart>(id);
        }

        public ContentItemMetadata GetDisplayForCommentedContent(int id) {
            var content = _contentManager.Get(id);
            if (content == null)
                return null;
            return _contentManager.GetItemMetadata(content);
        }

        public CommentPart CreateComment(CreateCommentContext context, bool moderateComments) {
            var comment = _contentManager.Create<CommentPart>("Comment");

            comment.Record.Author = context.Author;
            comment.Record.CommentDateUtc = _clock.UtcNow;
            comment.Record.CommentText = context.CommentText;
            comment.Record.Email = context.Email;
            comment.Record.SiteName = context.SiteName;
            comment.Record.UserName = (CurrentUser == null ? context.Author : CurrentUser.UserName);
            comment.Record.CommentedOn = context.CommentedOn;

            comment.Record.Status = _commentValidator.ValidateComment(comment)
                ? moderateComments ? CommentStatus.Pending : CommentStatus.Approved
                : CommentStatus.Spam;

            // store id of the next layer for large-grained operations, e.g. rss on blog
            //TODO:(rpaquay) Get rid of this (comment aspect takes care of container)
            var commentedOn = _contentManager.Get<ICommonPart>(comment.Record.CommentedOn);
            if (commentedOn != null && commentedOn.Container != null) {
                comment.Record.CommentedOnContainer = commentedOn.Container.ContentItem.Id;
            }

            return comment;
        }

        public void UpdateComment(int id, string name, string email, string siteName, string commentText, CommentStatus status) {
            CommentPart commentPart = GetComment(id);
            commentPart.Record.Author = name;
            commentPart.Record.Email = email;
            commentPart.Record.SiteName = siteName;
            commentPart.Record.CommentText = commentText;
            commentPart.Record.Status = status;
        }

        public void ApproveComment(int commentId) {
            CommentPart commentPart = GetComment(commentId);
            commentPart.Record.Status = CommentStatus.Approved;
        }

        public void PendComment(int commentId) {
            CommentPart commentPart = GetComment(commentId);
            commentPart.Record.Status = CommentStatus.Pending;
        }

        public void MarkCommentAsSpam(int commentId) {
            CommentPart commentPart = GetComment(commentId);
            commentPart.Record.Status = CommentStatus.Spam;
        }

        public void DeleteComment(int commentId) {
            _contentManager.Remove(_contentManager.Get(commentId));
        }

        public bool CommentsClosedForCommentedContent(int id) {
            return _closedCommentsRepository.Fetch(x => x.ContentItemId == id).Count() >= 1;
        }

        public void CloseCommentsForCommentedContent(int id) {
            if (CommentsClosedForCommentedContent(id))
                return;
            _closedCommentsRepository.Create(new ClosedCommentsRecord { ContentItemId = id });
        }

        public void EnableCommentsForCommentedContent(int id) {
            var closedComments = _closedCommentsRepository.Fetch(x => x.ContentItemId == id);
            foreach (var c in closedComments) {
                _closedCommentsRepository.Delete(c);
            }
        }
    }
}
