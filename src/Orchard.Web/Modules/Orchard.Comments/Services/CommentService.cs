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

        public IEnumerable<Comment> GetComments() {
            return _contentManager
                .Query<Comment, CommentRecord>()
                .List();
        }

        public IEnumerable<Comment> GetComments(CommentStatus status) {
            return _contentManager
                .Query<Comment, CommentRecord>()
                .Where(c => c.Status == status)
                .List();
        }

        public IEnumerable<Comment> GetCommentsForCommentedContent(int id) {
            return _contentManager
                .Query<Comment, CommentRecord>()
                .Where(c => c.CommentedOn == id || c.CommentedOnContainer == id)
                .List();
        }

        public IEnumerable<Comment> GetCommentsForCommentedContent(int id, CommentStatus status) {
            return _contentManager
                .Query<Comment, CommentRecord>()
                .Where(c => c.CommentedOn == id || c.CommentedOnContainer == id)
                .Where(ctx => ctx.Status == status)
                .List();
        }

        public Comment GetComment(int id) {
            return _contentManager.Get<Comment>(id);
        }

        public ContentItemMetadata GetDisplayForCommentedContent(int id) {
            var content = _contentManager.Get(id);
            if (content == null)
                return null;
            return _contentManager.GetItemMetadata(content);
        }

        public Comment CreateComment(CreateCommentContext context, bool moderateComments) {
            var comment = _contentManager.Create<Comment>(CommentDriver.ContentType.Name);

            comment.Record.Author = context.Author;
            comment.Record.CommentDateUtc = _clock.UtcNow;
            comment.Record.CommentText = context.CommentText;
            comment.Record.Email = context.Email;
            comment.Record.SiteName = context.SiteName;
            comment.Record.UserName = (CurrentUser == null ? context.Author : CurrentUser.UserName);
            comment.Record.CommentedOn = context.CommentedOn;

            comment.Record.Status = _commentValidator.ValidateComment(comment) ? moderateComments ? CommentStatus.Pending : CommentStatus.Approved : CommentStatus.Spam;

            // store id of the next layer for large-grained operations, e.g. rss on blog
            //TODO:(rpaquay) Get rid of this (comment aspect takes care of container)
            var commentedOn = _contentManager.Get<ICommonPart>(comment.Record.CommentedOn);
            if (commentedOn != null && commentedOn.Container != null) {
                comment.Record.CommentedOnContainer = commentedOn.Container.ContentItem.Id;
            }

            return comment;
        }

        public void UpdateComment(int id, string name, string email, string siteName, string commentText, CommentStatus status) {
            Comment comment = GetComment(id);
            comment.Record.Author = name;
            comment.Record.Email = email;
            comment.Record.SiteName = siteName;
            comment.Record.CommentText = commentText;
            comment.Record.Status = status;
        }

        public void ApproveComment(int commentId) {
            Comment comment = GetComment(commentId);
            comment.Record.Status = CommentStatus.Approved;
        }

        public void PendComment(int commentId) {
            Comment comment = GetComment(commentId);
            comment.Record.Status = CommentStatus.Pending;
        }

        public void MarkCommentAsSpam(int commentId) {
            Comment comment = GetComment(commentId);
            comment.Record.Status = CommentStatus.Spam;
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
