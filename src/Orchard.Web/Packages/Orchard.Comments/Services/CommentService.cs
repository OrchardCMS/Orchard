using System.Collections.Generic;
using System.Linq;
using Orchard.Comments.Models;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Orchard.Comments.Services {
    public interface ICommentService : IDependency {
        IEnumerable<Comment> GetComments();
        IEnumerable<Comment> GetComments(CommentStatus status);
        Comment GetComment(int id);
        IContentDisplayInfo GetDisplayForCommentedContent(int id);
        void CreateComment(Comment comment);
        void MarkCommentAsSpam(int commentId);
        void DeleteComment(int commentId);
    }

    public class CommentService : ICommentService {
        private readonly IRepository<Comment> _commentRepository;
        private readonly ICommentValidator _commentValidator;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;

        public CommentService(IRepository<Comment> commentRepository, 
                              ICommentValidator commentValidator,
                              IContentManager contentManager,
                              IAuthorizer authorizer, 
                              INotifier notifier) {
            _commentRepository = commentRepository;
            _commentValidator = commentValidator;
            _contentManager = contentManager;
            _authorizer = authorizer;
            _notifier = notifier;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public ISite CurrentSite { get; set; }

        #region Implementation of ICommentService

        public IEnumerable<Comment> GetComments() {
            return from comment in _commentRepository.Table.ToList() select comment;
        }

        public IEnumerable<Comment> GetComments(CommentStatus status) {
            return from comment in _commentRepository.Table.ToList() where comment.Status == status select comment;
        }

        public Comment GetComment(int id) {
            return _commentRepository.Get(id);
        }

        public IContentDisplayInfo GetDisplayForCommentedContent(int id) {
            return _contentManager.Get(id).As<IContentDisplayInfo>();
        }

        public void CreateComment(Comment comment) {
            comment.Status = _commentValidator.ValidateComment(comment) ? CommentStatus.Approved : CommentStatus.Spam;
            _commentRepository.Create(comment);
        }

        public void MarkCommentAsSpam(int commentId) {
            Comment comment = GetComment(commentId);
            comment.Status = CommentStatus.Spam;
        }

        public void DeleteComment(int commentId) {
            _commentRepository.Delete(GetComment(commentId));
        }

        #endregion
    }
}
