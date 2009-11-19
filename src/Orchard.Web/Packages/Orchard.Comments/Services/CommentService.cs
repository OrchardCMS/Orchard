using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Comments.Models;
using Orchard.Data;
using Orchard.Logging;

namespace Orchard.Comments.Services {
    public interface ICommentService : IDependency {
        IEnumerable<Comment> GetComments();
        IEnumerable<Comment> GetComments(CommentStatus status);
        Comment GetComment(int id);
        void MarkCommentAsSpam(int commentId);
        void DeleteComment(int commentId);
    }

    public class CommentService : ICommentService {
        private readonly IRepository<Comment> _commentRepository;

        public CommentService(IRepository<Comment> commentRepository) {
            _commentRepository = commentRepository;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        #region Implementation of ICommentService

        public IEnumerable<Comment> GetComments() {
            return from comment in _commentRepository.Table.ToList() select comment;
        }

        public IEnumerable<Comment> GetComments(CommentStatus status) {
            return from comment in _commentRepository.Table.ToList() where comment.Status == CommentStatus.Approved select comment;
        }

        public Comment GetComment(int id) {
            return _commentRepository.Get(id);
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
