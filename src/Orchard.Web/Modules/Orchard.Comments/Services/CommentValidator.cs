using System.Dynamic;
using Orchard.Comments.Models;

namespace Orchard.Comments.Services {
    public class AntiSpamFilterValidator : ICommentValidator {
        private readonly ICheckSpamEventHandler _spamEventHandler;

        public AntiSpamFilterValidator(ICheckSpamEventHandler spamEventHandler) {
            _spamEventHandler = spamEventHandler;
        }

        public bool ValidateComment(CommentPart commentPart) {
            // true == spam

            var text = commentPart.Author + System.Environment.NewLine
                       + commentPart.CommentText + System.Environment.NewLine
                       + commentPart.Email + System.Environment.NewLine
                       + commentPart.SiteName + System.Environment.NewLine;

            dynamic context = new ExpandoObject();
            context.Checked = false;
            context.IsSpam = false;
            context.Text = text;

            _spamEventHandler.CheckSpam(context);

            return context.IsSpam;
        }
    }
}