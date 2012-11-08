using System;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;

namespace Orchard.Comments.Tokens {
    public interface ITokenProvider : IEventHandler {
        void Describe(dynamic context);
        void Evaluate(dynamic context);
    }

    public class CommentTokens : ITokenProvider {
        private readonly IContentManager _contentManager;

        public CommentTokens(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic context) {
            context.For("Content", T("Comments"), T("Comments"))
                .Token("CommentedOn", T("Commented On"), T("The content item this comment was created on."))
                .Token("CommentMessage", T("Comment Message"), T("The text of the comment itself"))
                .Token("CommentAuthor", T("Comment Author"), T("The author of the comment."))
                .Token("CommentAuthorUrl", T("Comment Author Url"), T("The url provided by the author of the comment."))
                .Token("CommentAuthorEmail", T("Comment Author Email"), T("The email provided by the author of the comment."))
                ;
        }

        public void Evaluate(dynamic context) {
            context.For<IContent>("Content")
                .Token("CommentedOn", (Func<IContent, object>)(content => content.As<CommentPart>().CommentedOn))
                .Chain("CommentedOn", "Content", (Func<IContent, object>)(content => _contentManager.Get(content.As<CommentPart>().CommentedOn)))
                .Token("CommentMessage", (Func<IContent, object>)(content => content.As<CommentPart>().CommentText))
                .Token("CommentAuthor", (Func<IContent, object>)CommentAuthor)
                .Token("CommentAuthorUrl", (Func<IContent, object>)(content => content.As<CommentPart>().SiteName))
                .Token("CommentAuthorEmail", (Func<IContent, object>)(content => content.As<CommentPart>().Email))
                ;
        }

        private static string CommentAuthor(IContent comment) {
            var commentPart = comment.As<CommentPart>();
            return String.IsNullOrWhiteSpace(commentPart.UserName) ? commentPart.Author : commentPart.UserName;
        }
    }
}