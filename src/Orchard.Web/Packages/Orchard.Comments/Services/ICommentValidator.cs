using Orchard.Comments.Models;

namespace Orchard.Comments.Services {
    public interface ICommentValidator : IDependency {
        bool ValidateComment(CommentRecord comment);
    }
}