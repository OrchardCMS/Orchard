using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public interface IUserSuspensionConditionProvider : IDependency {
        IContentQuery<UserPart> AlterQuery(
            IContentQuery<UserPart> query);

        bool UserIsProtected(UserPart userPart);
    }
}
