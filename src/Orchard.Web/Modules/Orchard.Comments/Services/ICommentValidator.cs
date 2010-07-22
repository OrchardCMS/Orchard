using Orchard.Comments.Models;

namespace Orchard.Comments.Services {
    public interface ICommentValidator : IDependency {
        bool ValidateComment(CommentPart commentPart);
    }
}