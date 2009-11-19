using Orchard.Comments.Models;
using Orchard.Logging;

namespace Orchard.Comments.Services {
    public interface ICommentValidator : IDependency {
        bool ValidateComment(Comment comment);
    }

    public class CommentValidator : ICommentValidator {
        public CommentValidator() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        #region Implementation of ICommentValidator

        public bool ValidateComment(Comment comment) {
            //TODO: integrate spam filter.
            return true;
        }

        #endregion
    }
}
