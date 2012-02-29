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
            context.For("Content", T("Content Items"), T("Content Items"))
                .Token("CommentedOn", T("Commented On"), T("The content item this comment was created on."))
                .Token("CommentMessage", T("Comment Message"), T("The text of the comment itself"))
                .Token("CommentAuthor", T("Comment Author"), T("The author of the comment."))
                ;
        }

        public void Evaluate(dynamic context) {
            context.For<IContent>("Content")
                .Token("CommentedOn", (Func<IContent, object>)(content => content.As<CommentPart>().Record.CommentedOn))
                .Chain("CommentedOn", "Content", (Func<IContent, object>)(content => _contentManager.Get(content.As<CommentPart>().Record.CommentedOn)))
                .Token("CommentMessage", (Func<IContent, object>)(content => content.As<CommentPart>().Record.CommentText))
                .Token("CommentAuthor", (Func<IContent, object>)CommentAuthor)
                ;
        }

        private static string CommentAuthor(IContent comment) {
            var commentPart = comment.As<CommentPart>();
            return String.IsNullOrWhiteSpace(commentPart.Record.UserName) ? commentPart.Record.Author : commentPart.Record.UserName;
        }
    }
}