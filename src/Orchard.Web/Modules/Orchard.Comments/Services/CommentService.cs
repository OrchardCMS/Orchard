using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement.Aspects;
using Orchard.Data;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Services;

namespace Orchard.Comments.Services {
    [UsedImplicitly]
    public class CommentService : ICommentService {
        private readonly IClock _clock;
        private readonly ICommentValidator _commentValidator;
        private readonly IOrchardServices _orchardServices;

        public CommentService(IClock clock,
                              ICommentValidator commentValidator,
                              IOrchardServices orchardServices) {
            _clock = clock;
            _commentValidator = commentValidator;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IContentQuery<CommentPart, CommentPartRecord> GetComments() {
            return _orchardServices.ContentManager
                       .Query<CommentPart, CommentPartRecord>();
        }

        public IContentQuery<CommentPart, CommentPartRecord> GetComments(CommentStatus status) {
            return _orchardServices.ContentManager
                       .Query<CommentPart, CommentPartRecord>()
                       .Where(c => c.Status == status);
        }

        public IContentQuery<CommentPart, CommentPartRecord> GetCommentsForCommentedContent(int id) {
            return _orchardServices.ContentManager
                       .Query<CommentPart, CommentPartRecord>()
                       .Where(c => c.CommentedOn == id || c.CommentedOnContainer == id);
        }

        public IContentQuery<CommentPart, CommentPartRecord> GetCommentsForCommentedContent(int id, CommentStatus status) {
            return _orchardServices.ContentManager
                       .Query<CommentPart, CommentPartRecord>()
                       .Where(c => c.CommentedOn == id || c.CommentedOnContainer == id)
                       .Where(ctx => ctx.Status == status);
        }

        public CommentPart GetComment(int id) {
            return _orchardServices.ContentManager.Get<CommentPart>(id);
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

        public CommentPart CreateComment(CreateCommentContext context, bool moderateComments) {
            return _orchardServices.ContentManager.Create<CommentPart>("Comment", comment => {
                comment.Record.Author = context.Author;
                comment.Record.CommentDateUtc = _clock.UtcNow;
                comment.Record.CommentText = context.CommentText;
                comment.Record.Email = context.Email;
                comment.Record.SiteName = context.SiteName;
                comment.Record.UserName = (_orchardServices.WorkContext.CurrentUser != null ? _orchardServices.WorkContext.CurrentUser.UserName : null);
                comment.Record.CommentedOn = context.CommentedOn;
                comment.Record.Status = _commentValidator.ValidateComment(comment)
                                            ? moderateComments ? CommentStatus.Pending : CommentStatus.Approved
                                            : CommentStatus.Spam;
                var commentedOn = _orchardServices.ContentManager.Get<ICommonPart>(comment.Record.CommentedOn);
                if (commentedOn != null && commentedOn.Container != null) {
                    comment.Record.CommentedOnContainer = commentedOn.Container.ContentItem.Id;
                }
                commentedOn.As<CommentsPart>().Record.CommentPartRecords.Add(comment.Record);
            });
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

        public void UnapproveComment(int commentId) {
            CommentPart commentPart = GetComment(commentId);
            commentPart.Record.Status = CommentStatus.Pending;
        }

        public void MarkCommentAsSpam(int commentId) {
            CommentPart commentPart = GetComment(commentId);
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
    }
}
