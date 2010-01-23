using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement.Aspects;
using Orchard.Data;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Orchard.Comments.Services {
    public interface ICommentService : IDependency {
        IEnumerable<Comment> GetComments();
        IEnumerable<Comment> GetComments(CommentStatus status);
        IEnumerable<Comment> GetCommentsForCommentedContent(int id);
        IEnumerable<Comment> GetCommentsForCommentedContent(int id, CommentStatus status);
        Comment GetComment(int id);
        ContentItemMetadata GetDisplayForCommentedContent(int id);
        Comment CreateComment(CommentRecord commentRecord);
        void UpdateComment(int id, string name, string email, string siteName, string commentText, CommentStatus status);
        void ApproveComment(int commentId);
        void PendComment(int commentId);
        void MarkCommentAsSpam(int commentId);
        void DeleteComment(int commentId);
        bool CommentsClosedForCommentedContent(int id);
        void CloseCommentsForCommentedContent(int id);
        void EnableCommentsForCommentedContent(int id);
    }

    [UsedImplicitly]
    public class CommentService : ICommentService {
        private readonly IRepository<ClosedCommentsRecord> _closedCommentsRepository;
        private readonly ICommentValidator _commentValidator;
        private readonly IContentManager _contentManager;

        public CommentService(IRepository<CommentRecord> commentRepository,
                              IRepository<ClosedCommentsRecord> closedCommentsRepository,
                              IRepository<HasCommentsRecord> hasCommentsRepository,
                              ICommentValidator commentValidator,
                              IContentManager contentManager,
                              IAuthorizer authorizer,
                              INotifier notifier) {
            _closedCommentsRepository = closedCommentsRepository;
            _commentValidator = commentValidator;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }


        #region Implementation of ICommentService

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

        public Comment CreateComment(CommentRecord commentRecord) {
            var comment = _contentManager.Create<Comment>("comment");

            //TODO:(rpaquay) CommentRecord should never be used as "data object"
            comment.Record.Author = commentRecord.Author;
            comment.Record.CommentDateUtc = commentRecord.CommentDateUtc;
            comment.Record.CommentText = commentRecord.CommentText;
            comment.Record.Email = commentRecord.Email;
            comment.Record.SiteName = commentRecord.SiteName;
            comment.Record.UserName = commentRecord.UserName;
            comment.Record.CommentedOn = commentRecord.CommentedOn;

            comment.Record.Status = _commentValidator.ValidateComment(commentRecord) ? CommentStatus.Pending : CommentStatus.Spam;

            // store id of the next layer for large-grained operations, e.g. rss on blog
            //TODO:(rpaquay) Get rid of this (comment aspect takes care of container)
            var commentedOn = _contentManager.Get<ICommonAspect>(comment.Record.CommentedOn);
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
            closedComments.ForEach(c => _closedCommentsRepository.Delete(c));
        }

        #endregion
    }
}
