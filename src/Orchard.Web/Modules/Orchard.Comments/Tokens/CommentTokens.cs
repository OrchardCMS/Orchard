using System;
using System.Web.Mvc;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Tokens;

namespace Orchard.Comments.Tokens {

    public class CommentTokens : ITokenProvider {
        private readonly IContentManager _contentManager;
        private readonly ICommentService _commentService;

        public CommentTokens(
            IContentManager contentManager,
            ICommentService commentService) {
            _contentManager = contentManager;
            _commentService = commentService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Content", T("Comments"), T("Comments"))
                .Token("CommentedOn", T("Commented On"), T("The content item this comment was created on."))
                .Token("CommentMessage", T("Comment Message"), T("The text of the comment itself"))
                .Token("CommentAuthor", T("Comment Author"), T("The author of the comment."))
                .Token("CommentAuthorUrl", T("Comment Author Url"), T("The url provided by the author of the comment."))
                .Token("CommentAuthorEmail", T("Comment Author Email"), T("The email provided by the author of the comment."))
                .Token("CommentApproveUrl", T("Comment approval Url"), T("The absolute url to follow in order to approve this comment."))
                .Token("CommentModerateUrl", T("Comment moderation Url"), T("The absolute url to follow in order to moderate this comment."))
                .Token("CommentDeleteUrl", T("Comment deletion Url"), T("The absolute url to follow in order to delete this comment."))
                ;
        }

        public void Evaluate(EvaluateContext context) {
            context.For<IContent>("Content")
                .Token("CommentedOn", content => content.As<CommentPart>().CommentedOn)
                .Chain("CommentedOn", "Content", content => _contentManager.Get(content.As<CommentPart>().CommentedOn))
                .Token("CommentMessage", content => content.As<CommentPart>().CommentText)
                .Chain("CommentMessage", "Text", content => content.As<CommentPart>().CommentText)
                .Token("CommentAuthor", CommentAuthor)
                .Chain("CommentAuthor", "Text", CommentAuthor)
                .Token("CommentAuthorUrl", content => content.As<CommentPart>().SiteName)
                .Chain("CommentAuthorUrl", "Text", content => content.As<CommentPart>().SiteName)
                .Token("CommentAuthorEmail", content => content.As<CommentPart>().Email)
                .Chain("CommentAuthorEmail", "Text", content => content.As<CommentPart>().Email)
                .Token("CommentApproveUrl", content => _commentService.CreateProtectedUrl("Approve", content.As<CommentPart>()))
                .Token("CommentModerateUrl", content => _commentService.CreateProtectedUrl("Moderate", content.As<CommentPart>()))
                .Token("CommentDeleteUrl", content => _commentService.CreateProtectedUrl("Delete", content.As<CommentPart>()))
                ;
        }

        private static string CommentAuthor(IContent comment) {
            var commentPart = comment.As<CommentPart>();
            return String.IsNullOrWhiteSpace(commentPart.UserName) ? commentPart.Author : commentPart.UserName;
        }
    }
}